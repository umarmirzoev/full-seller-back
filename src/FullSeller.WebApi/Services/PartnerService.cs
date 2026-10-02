using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;
using FullSeller.WebApi.Contracts.Auth;
using FullSeller.WebApi.Contracts.Catalog;
using FullSeller.WebApi.Contracts.Partner;
using Microsoft.AspNetCore.Identity;

namespace FullSeller.WebApi.Services;

/// <summary>
/// Кабинет партнёра (производителя/оптового поставщика): вход по логину+паролю (учётку создаёт
/// администратор — см. AdminPartnersController), профиль, список и добавление своих товаров в каталог.
/// Токены — тот же ITokenService/JwtTokenService, что и у AuthService, но с ролью "Partner" и отдельной
/// таблицей refresh-токенов (PartnerRefreshTokens), т.к. RefreshTokens ссылается на Users(Id).
/// </summary>
public class PartnerService
{
    private readonly IPartnerRepository _partnerRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<Partner> _passwordHasher = new();

    private const int RefreshTokenLifetimeDays = 30;

    public PartnerService(IPartnerRepository partnerRepository, ICatalogRepository catalogRepository, ITokenService tokenService)
    {
        _partnerRepository = partnerRepository;
        _catalogRepository = catalogRepository;
        _tokenService = tokenService;
    }

    public async Task<TokenPairResponse> LoginAsync(string login, string password, CancellationToken ct)
    {
        var partner = await _partnerRepository.GetByLoginAsync(login, ct)
            ?? throw new InvalidOperationException("Неверный логин или пароль.");

        if (!partner.IsActive)
            throw new InvalidOperationException("Доступ партнёра отключён администратором.");

        var verification = _passwordHasher.VerifyHashedPassword(partner, partner.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Неверный логин или пароль.");

        return await IssueAndPersistTokensAsync(partner, ct);
    }

    public async Task<TokenPairResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _partnerRepository.GetRefreshTokenByHashAsync(hash, ct)
            ?? throw new InvalidOperationException("Refresh-токен недействителен.");

        if (stored.RevokedAt is not null || stored.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Refresh-токен истёк или отозван.");

        var partner = await _partnerRepository.GetByIdAsync(stored.PartnerId, ct)
            ?? throw new InvalidOperationException("Партнёр не найден.");

        await _partnerRepository.RevokeRefreshTokenAsync(stored.Id, ct);
        return await IssueAndPersistTokensAsync(partner, ct);
    }

    public async Task<PartnerProfileDto> GetProfileAsync(Guid partnerId, CancellationToken ct)
    {
        var partner = await _partnerRepository.GetByIdAsync(partnerId, ct)
            ?? throw new InvalidOperationException("Партнёр не найден.");
        return ToDto(partner);
    }

    public async Task<AdminProductListResponse> GetProductsAsync(Guid partnerId, int page, int pageSize, CancellationToken ct)
    {
        var result = await _partnerRepository.GetProductsByPartnerAsync(partnerId, page, pageSize, ct);
        var items = result.Items.Select(ToProductDto).ToList();
        return new AdminProductListResponse(items, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<AdminProductDto> CreateProductAsync(Guid partnerId, CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description,
            Composition = request.Composition,
            BaseSku = request.BaseSku,
            WeightGrams = request.WeightGrams,
            ImageUrls = request.ImageUrls?.ToList() ?? new List<string>(),
            OldPrice = request.OldPrice,
            IsNew = request.IsNew,
            IsHit = request.IsHit,
            AudienceTag = request.AudienceTag,
            // Товары от партнёров публикуются только после одобрения администратором (AdminCatalogController.SetProductActive) —
            // не видны в общем каталоге, пока IsActive == false.
            IsActive = false,
        };
        var productId = await _partnerRepository.CreateProductForPartnerAsync(partnerId, product, ct);
        product.Id = productId;

        if (request.PriceTiers is { Count: > 0 })
        {
            var tiers = request.PriceTiers
                .Where(t => t.PricePerUnit > 0)
                .Select(t => new PriceTier { ProductId = productId, MinQuantity = t.MinQuantity, PricePerUnit = t.PricePerUnit })
                .ToList();
            if (tiers.Count > 0)
            {
                await _catalogRepository.ReplacePriceTiersAsync(productId, tiers, ct);
                product.MinPrice = tiers.Min(t => t.PricePerUnit);
            }
        }
        else if (request.Price is { } price && price > 0)
        {
            await _catalogRepository.ReplacePriceTiersAsync(productId,
                new[] { new PriceTier { ProductId = productId, MinQuantity = 1, PricePerUnit = price } }, ct);
            product.MinPrice = price;
        }

        if (request.Variants is { Count: > 0 })
        {
            foreach (var v in request.Variants)
            {
                await _catalogRepository.CreateVariantAsync(new ProductVariant
                {
                    ProductId = productId,
                    Size = v.Size,
                    Color = v.Color,
                    Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{request.BaseSku}-{v.Size}-{v.Color}" : v.Sku,
                    StockQuantity = v.StockQuantity,
                }, ct);
            }
        }

        return ToProductDto(product);
    }

    /// <summary>Варианты (размер/цвет/сток) товара партнёра — для предзаполнения формы редактирования на сайте.</summary>
    public async Task<IReadOnlyList<VariantDto>> GetProductVariantsAsync(Guid partnerId, Guid productId, CancellationToken ct)
    {
        var existing = await _catalogRepository.GetProductByIdAsync(productId, ct)
            ?? throw new InvalidOperationException("Товар не найден.");
        if (existing.PartnerId != partnerId)
            throw new InvalidOperationException("Товар не принадлежит партнёру.");

        var variants = await _catalogRepository.GetVariantsByProductIdAsync(productId, ct);
        return variants.Select(v => new VariantDto(v.Id, v.Size, v.Color, v.Sku, v.StockQuantity)).ToList();
    }

    /// <summary>Оптовая сетка цен ("мешки") товара партнёра — для предзаполнения формы редактирования на сайте.</summary>
    public async Task<IReadOnlyList<PriceTierRequest>> GetProductPriceTiersAsync(Guid partnerId, Guid productId, CancellationToken ct)
    {
        var existing = await _catalogRepository.GetProductByIdAsync(productId, ct)
            ?? throw new InvalidOperationException("Товар не найден.");
        if (existing.PartnerId != partnerId)
            throw new InvalidOperationException("Товар не принадлежит партнёру.");

        var tiers = await _catalogRepository.GetPriceTiersAsync(productId, ct);
        return tiers.Select(t => new PriceTierRequest(t.MinQuantity, t.PricePerUnit)).ToList();
    }

    /// <summary>Удаляет собственный товар партнёра; чужой/несуществующий товар — ошибка.</summary>
    public async Task DeleteProductAsync(Guid partnerId, Guid productId, CancellationToken ct)
    {
        var existing = await _catalogRepository.GetProductByIdAsync(productId, ct)
            ?? throw new InvalidOperationException("Товар не найден.");
        if (existing.PartnerId != partnerId)
            throw new InvalidOperationException("Товар не принадлежит партнёру.");

        await _catalogRepository.DeleteProductAsync(productId, ct);
    }

    /// <summary>Редактирует товар, ранее добавленный этим же партнёром; чужой/несуществующий товар — ошибка.
    /// После правки товар снова уходит на модерацию администратору (IsActive сбрасывается в false).</summary>
    public async Task<AdminProductDto> UpdateProductAsync(Guid partnerId, Guid productId, PartnerUpdateProductRequest request, CancellationToken ct)
    {
        var existing = await _catalogRepository.GetProductByIdAsync(productId, ct)
            ?? throw new InvalidOperationException("Товар не найден.");
        if (existing.PartnerId != partnerId)
            throw new InvalidOperationException("Товар не принадлежит партнёру.");

        existing.Name = request.Name;
        existing.CategoryId = request.CategoryId;
        existing.BrandId = request.BrandId;
        existing.Description = request.Description;
        existing.Composition = request.Composition;
        existing.BaseSku = request.BaseSku;
        existing.WeightGrams = request.WeightGrams;
        existing.ImageUrls = request.ImageUrls?.ToList() ?? existing.ImageUrls;
        existing.OldPrice = request.OldPrice;
        existing.IsNew = request.IsNew;
        existing.IsHit = request.IsHit;
        existing.AudienceTag = request.AudienceTag ?? existing.AudienceTag;
        existing.IsActive = false;

        await _catalogRepository.UpdateProductAsync(existing, ct);

        if (request.PriceTiers is { Count: > 0 })
        {
            var tiers = request.PriceTiers
                .Where(t => t.PricePerUnit > 0)
                .Select(t => new PriceTier { ProductId = productId, MinQuantity = t.MinQuantity, PricePerUnit = t.PricePerUnit })
                .ToList();
            if (tiers.Count > 0)
            {
                await _catalogRepository.ReplacePriceTiersAsync(productId, tiers, ct);
                existing.MinPrice = tiers.Min(t => t.PricePerUnit);
            }
        }
        else if (request.Price is { } price && price > 0)
        {
            await _catalogRepository.ReplacePriceTiersAsync(productId,
                new[] { new PriceTier { ProductId = productId, MinQuantity = 1, PricePerUnit = price } }, ct);
            existing.MinPrice = price;
        }

        if (request.Variants is { Count: > 0 })
        {
            var oldVariants = await _catalogRepository.GetVariantsByProductIdAsync(productId, ct);
            foreach (var ov in oldVariants)
                await _catalogRepository.DeleteVariantAsync(ov.Id, ct);

            foreach (var v in request.Variants)
            {
                await _catalogRepository.CreateVariantAsync(new ProductVariant
                {
                    ProductId = productId,
                    Size = v.Size,
                    Color = v.Color,
                    Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{request.BaseSku}-{v.Size}-{v.Color}" : v.Sku,
                    StockQuantity = v.StockQuantity,
                }, ct);
            }
        }

        return ToProductDto(existing);
    }

    private async Task<TokenPairResponse> IssueAndPersistTokensAsync(Partner partner, CancellationToken ct)
    {
        var pair = _tokenService.IssuePartnerTokens(partner.Id, partner.Login);
        await _partnerRepository.CreateRefreshTokenAsync(new PartnerRefreshToken
        {
            PartnerId = partner.Id,
            TokenHash = _tokenService.HashToken(pair.RefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays),
        }, ct);

        return new TokenPairResponse(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt);
    }

    private static PartnerProfileDto ToDto(Partner p) => new(p.Id, p.CompanyName, p.ContactName, p.Phone, p.Login, p.IsActive, p.CreatedAt);

    private static AdminProductDto ToProductDto(Product p) => new(
        p.Id, p.Name, p.CategoryId, p.BrandId, p.Description, p.Composition, p.BaseSku, p.WeightGrams,
        p.ImageUrls, p.IsActive, p.MinPrice, p.OldPrice, p.IsNew, p.IsHit, p.CreatedAt, p.AudienceTag);
}
