using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken ct = default);
    Task<PagedResult<Product>> SearchProductsAsync(CatalogSearchFilter filter, CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken ct = default);
    Task<IReadOnlyList<PriceTier>> GetPriceTiersAsync(Guid productId, CancellationToken ct = default);
    Task DecrementStockAsync(Guid variantId, int quantity, CancellationToken ct = default);

    // ---- Админ-панель: управление каталогом (ТЗ п.10 «Админ-панель → Товары») ----

    Task<Guid> CreateCategoryAsync(Category category, CancellationToken ct = default);
    Task UpdateCategoryAsync(Category category, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    Task<Guid> CreateBrandAsync(Brand brand, CancellationToken ct = default);
    Task UpdateBrandAsync(Brand brand, CancellationToken ct = default);
    Task DeleteBrandAsync(Guid id, CancellationToken ct = default);

    /// <summary>Постраничный список товаров для админ-панели — без фильтра IsActive (видно и скрытые товары).</summary>
    Task<PagedResult<Product>> GetAllProductsForAdminAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<Guid> CreateProductAsync(Product product, CancellationToken ct = default);
    Task UpdateProductAsync(Product product, CancellationToken ct = default);
    /// <summary>Мягкое удаление — товар помечается IsActive = false и пропадает из каталога, но остаётся в истории заказов/отзывов.</summary>
    Task SetProductActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);

    Task<Guid> CreateVariantAsync(ProductVariant variant, CancellationToken ct = default);
    Task UpdateVariantAsync(ProductVariant variant, CancellationToken ct = default);
    Task DeleteVariantAsync(Guid id, CancellationToken ct = default);
    Task SetStockAsync(Guid variantId, int stockQuantity, CancellationToken ct = default);

    /// <summary>Полностью заменяет оптовую price-tier сетку товара (удаляет старые строки, вставляет новые) и пересчитывает Products.MinPrice.</summary>
    Task ReplacePriceTiersAsync(Guid productId, IReadOnlyList<PriceTier> tiers, CancellationToken ct = default);
}
