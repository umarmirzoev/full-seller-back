namespace FullSeller.WebApi.Contracts.Catalog;

public record CategoryDto(Guid Id, string Name, string Slug, Guid? ParentCategoryId);
public record BrandDto(Guid Id, string Name, string? LogoUrl);

public record PriceTierDto(int MinQuantity, decimal PricePerUnit);
public record ProductVariantDto(Guid Id, string? Size, string? Color, string Sku, int StockQuantity);

public record ProductListItemDto(
    Guid Id, string Name, Guid CategoryId, Guid? BrandId,
    decimal MinPrice, decimal Rating, int ReviewsCount, IReadOnlyList<string> ImageUrls);

public record ProductDetailsDto(
    Guid Id, string Name, Guid CategoryId, Guid? BrandId, string? Description, string? Composition,
    int WeightGrams, IReadOnlyList<string> ImageUrls, decimal Rating, int ReviewsCount,
    IReadOnlyList<PriceTierDto> PriceTiers, IReadOnlyList<ProductVariantDto> Variants);

public record ProductListResponse(IReadOnlyList<ProductListItemDto> Items, int TotalCount, int Page, int PageSize);
