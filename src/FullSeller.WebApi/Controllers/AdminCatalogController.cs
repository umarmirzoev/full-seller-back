using FullSeller.Domain.Entities;
using FullSeller.Domain.Exceptions;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Админ-панель → управление каталогом: категории, бренды, товары, варианты (размер/цвет/сток)
/// и оптовая шкала цен (ТЗ п.10 «Админ-панель → Товары»). Доступ — только Admin и SuperAdmin.
/// </summary>
[ApiController]
[Route("api/admin/catalog")]
[Authorize]
[AdminOnly]
public class AdminCatalogController : ControllerBase
{
    private readonly ICatalogRepository _catalog;

    public AdminCatalogController(ICatalogRepository catalog)
    {
        _catalog = catalog;
    }

    // ---- Категории ----

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category { Name = request.Name, Slug = request.Slug, ParentCategoryId = request.ParentCategoryId };
        await _catalog.CreateCategoryAsync(category, ct);
        return Ok(new CategoryDto(category.Id, category.Name, category.Slug, category.ParentCategoryId));
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        await _catalog.UpdateCategoryAsync(new Category { Id = id, Name = request.Name, Slug = request.Slug, ParentCategoryId = request.ParentCategoryId }, ct);
        return NoContent();
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        await _catalog.DeleteCategoryAsync(id, ct);
        return NoContent();
    }

    // ---- Бренды ----

    [HttpPost("brands")]
    public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandRequest request, CancellationToken ct)
    {
        var brand = new Brand { Name = request.Name, LogoUrl = request.LogoUrl };
        await _catalog.CreateBrandAsync(brand, ct);
        return Ok(new BrandDto(brand.Id, brand.Name, brand.LogoUrl));
    }

    [HttpPut("brands/{id:guid}")]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken ct)
    {
        await _catalog.UpdateBrandAsync(new Brand { Id = id, Name = request.Name, LogoUrl = request.LogoUrl }, ct);
        return NoContent();
    }

    [HttpDelete("brands/{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken ct)
    {
        await _catalog.DeleteBrandAsync(id, ct);
        return NoContent();
    }

    // ---- Товары ----

    /// <summary>Постраничный список товаров для админ-панели — включает скрытые (IsActive = false).</summary>
    [HttpGet("products")]
    public async Task<ActionResult<AdminProductListResponse>> GetProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _catalog.GetAllProductsForAdminAsync(page, pageSize, search, ct);
        var items = result.Items.Select(ToDto).ToList();
        return Ok(new AdminProductListResponse(items, result.TotalCount, result.Page, result.PageSize));
    }

    [HttpPost("products")]
    public async Task<ActionResult<AdminProductDto>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
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
            IsActive = true,
        };
        await _catalog.CreateProductAsync(product, ct);
        return Ok(ToDto(product));
    }

    [HttpPut("products/{id:guid}")]
    public async Task<ActionResult<AdminProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var existing = await _catalog.GetProductByIdAsync(id, ct) ?? throw new NotFoundException("Товар", id);
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

        await _catalog.UpdateProductAsync(existing, ct);
        return Ok(ToDto(existing));
    }

    /// <summary>Скрыть/показать товар в каталоге, не удаляя его (сохраняет историю заказов и отзывов).</summary>
    [HttpPut("products/{id:guid}/active")]
    public async Task<IActionResult> SetProductActive(Guid id, [FromBody] SetProductActiveRequest request, CancellationToken ct)
    {
        await _catalog.SetProductActiveAsync(id, request.IsActive, ct);
        return NoContent();
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        await _catalog.DeleteProductAsync(id, ct);
        return NoContent();
    }

    // ---- Варианты (размер/цвет/сток) ----

    [HttpPost("products/{productId:guid}/variants")]
    public async Task<ActionResult<ProductVariantDto>> CreateVariant(Guid productId, [FromBody] CreateVariantRequest request, CancellationToken ct)
    {
        var variant = new ProductVariant { ProductId = productId, Size = request.Size, Color = request.Color, Sku = request.Sku, StockQuantity = request.StockQuantity };
        await _catalog.CreateVariantAsync(variant, ct);
        return Ok(new ProductVariantDto(variant.Id, variant.Size, variant.Color, variant.Sku, variant.StockQuantity));
    }

    [HttpPut("variants/{id:guid}")]
    public async Task<IActionResult> UpdateVariant(Guid id, [FromBody] UpdateVariantRequest request, CancellationToken ct)
    {
        var existing = await _catalog.GetVariantByIdAsync(id, ct) ?? throw new NotFoundException("Вариант товара", id);
        existing.Size = request.Size;
        existing.Color = request.Color;
        existing.Sku = request.Sku;
        existing.StockQuantity = request.StockQuantity;

        await _catalog.UpdateVariantAsync(existing, ct);
        return NoContent();
    }

    [HttpDelete("variants/{id:guid}")]
    public async Task<IActionResult> DeleteVariant(Guid id, CancellationToken ct)
    {
        await _catalog.DeleteVariantAsync(id, ct);
        return NoContent();
    }

    [HttpPut("variants/{id:guid}/stock")]
    public async Task<IActionResult> SetStock(Guid id, [FromBody] SetStockRequest request, CancellationToken ct)
    {
        await _catalog.SetStockAsync(id, request.StockQuantity, ct);
        return NoContent();
    }

    // ---- Оптовая шкала цен ----

    /// <summary>Полностью заменяет оптовую price-tier сетку товара и пересчитывает его MinPrice (видимую цену в каталоге).</summary>
    [HttpPut("products/{productId:guid}/price-tiers")]
    public async Task<IActionResult> SetPriceTiers(Guid productId, [FromBody] SetPriceTiersRequest request, CancellationToken ct)
    {
        var tiers = request.Tiers.Select(t => new PriceTier { ProductId = productId, MinQuantity = t.MinQuantity, PricePerUnit = t.PricePerUnit }).ToList();
        await _catalog.ReplacePriceTiersAsync(productId, tiers, ct);
        return NoContent();
    }

    private static AdminProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.CategoryId, p.BrandId, p.Description, p.Composition, p.BaseSku, p.WeightGrams,
        p.ImageUrls, p.IsActive, p.MinPrice, p.OldPrice, p.IsNew, p.IsHit, p.CreatedAt, p.AudienceTag);
}
