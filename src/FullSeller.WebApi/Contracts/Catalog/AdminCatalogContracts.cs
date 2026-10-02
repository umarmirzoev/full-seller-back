namespace FullSeller.WebApi.Contracts.Catalog;

// ---- Категории и бренды ----

public record CreateCategoryRequest(string Name, string Slug, Guid? ParentCategoryId);
public record UpdateCategoryRequest(string Name, string Slug, Guid? ParentCategoryId);

public record CreateBrandRequest(string Name, string? LogoUrl);
public record UpdateBrandRequest(string Name, string? LogoUrl);

// ---- Товары (админ-панель, ТЗ п.10 «Товары») ----

public record AdminProductDto(
    Guid Id, string Name, Guid CategoryId, Guid? BrandId, string? Description, string? Composition,
    string BaseSku, int WeightGrams, IReadOnlyList<string> ImageUrls, bool IsActive,
    decimal MinPrice, decimal? OldPrice, bool IsNew, bool IsHit, DateTime CreatedAt, string? AudienceTag = null);

public record AdminProductListResponse(IReadOnlyList<AdminProductDto> Items, int TotalCount, int Page, int PageSize);

public record CreateProductRequest(
    string Name, Guid CategoryId, Guid? BrandId, string? Description, string? Composition,
    string BaseSku, int WeightGrams, IReadOnlyList<string>? ImageUrls,
    decimal? OldPrice, bool IsNew, bool IsHit,
    decimal? Price = null, IReadOnlyList<CreateVariantRequest>? Variants = null, string? AudienceTag = null,
    // Оптовая сетка "мешков" (напр. от 700, от 5000, от 10000 пар) — если задана, перекрывает Price
    // (который тогда остаётся только розничной "витринной" ценой за 1 шт).
    IReadOnlyList<PriceTierRequest>? PriceTiers = null);

public record UpdateProductRequest(
    string Name, Guid CategoryId, Guid? BrandId, string? Description, string? Composition,
    string BaseSku, int WeightGrams, IReadOnlyList<string>? ImageUrls,
    decimal? OldPrice, bool IsNew, bool IsHit);

/// <summary>Редактирование партнёром собственного товара (api/partner/products/{id}) — переиспользует поля создания (цена/варианты/аудитория); после правки товар снова уходит на модерацию (IsActive=false).</summary>
public record PartnerUpdateProductRequest(
    string Name, Guid CategoryId, Guid? BrandId, string? Description, string? Composition,
    string BaseSku, int WeightGrams, IReadOnlyList<string>? ImageUrls,
    decimal? OldPrice, bool IsNew, bool IsHit,
    decimal? Price = null, IReadOnlyList<CreateVariantRequest>? Variants = null, string? AudienceTag = null,
    IReadOnlyList<PriceTierRequest>? PriceTiers = null);

public record SetProductActiveRequest(bool IsActive);

// ---- Варианты (размер/цвет/сток) и оптовая шкала цен ----

public record CreateVariantRequest(string? Size, string? Color, string Sku, int StockQuantity);
/// <summary>Чтение варианта (размер/цвет/сток) — используется в api/partner/products/{id}/variants.</summary>
public record VariantDto(Guid Id, string? Size, string? Color, string Sku, int StockQuantity);
public record UpdateVariantRequest(string? Size, string? Color, string Sku, int StockQuantity);
public record SetStockRequest(int StockQuantity);

public record PriceTierRequest(int MinQuantity, decimal PricePerUnit);
public record SetPriceTiersRequest(IReadOnlyList<PriceTierRequest> Tiers);
