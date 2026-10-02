using FullSeller.Domain.Common;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Contracts.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogRepository _catalog;

    public CatalogController(ICatalogRepository catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        var categories = await _catalog.GetCategoriesAsync(ct);
        return Ok(categories.Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.ParentCategoryId)));
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetBrands(CancellationToken ct)
    {
        var brands = await _catalog.GetBrandsAsync(ct);
        return Ok(brands.Select(b => new BrandDto(b.Id, b.Name, b.LogoUrl)));
    }

    [HttpGet("products")]
    public async Task<ActionResult<ProductListResponse>> GetProducts(
        [FromQuery] Guid? categoryId, [FromQuery] Guid? brandId, [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice, [FromQuery] string? sortBy, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new CatalogSearchFilter
        {
            CategoryId = categoryId, BrandId = brandId, MinPrice = minPrice, MaxPrice = maxPrice,
            SortBy = sortBy, Page = page, PageSize = pageSize,
        };
        var result = await _catalog.SearchProductsAsync(filter, ct);
        return Ok(ToResponse(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ProductListResponse>> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var filter = new CatalogSearchFilter { SearchText = q, Page = page, PageSize = pageSize };
        var result = await _catalog.SearchProductsAsync(filter, ct);
        return Ok(ToResponse(result));
    }

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<ProductDetailsDto>> GetProduct(Guid id, CancellationToken ct)
    {
        var product = await _catalog.GetProductByIdAsync(id, ct);
        if (product is null) return NotFound();

        var variants = await _catalog.GetVariantsByProductIdAsync(id, ct);
        var tiers = await _catalog.GetPriceTiersAsync(id, ct);

        return Ok(new ProductDetailsDto(
            product.Id, product.Name, product.CategoryId, product.BrandId, product.Description, product.Composition,
            product.WeightGrams, product.ImageUrls, product.Rating, product.ReviewsCount,
            tiers.Select(t => new PriceTierDto(t.MinQuantity, t.PricePerUnit)).ToList(),
            variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.Sku, v.StockQuantity)).ToList()));
    }

    private static ProductListResponse ToResponse(PagedResult<Domain.Entities.Product> result) => new(
        result.Items.Select(p => new ProductListItemDto(p.Id, p.Name, p.CategoryId, p.BrandId, p.MinPrice, p.Rating, p.ReviewsCount, p.ImageUrls)).ToList(),
        result.TotalCount, result.Page, result.PageSize);
}
