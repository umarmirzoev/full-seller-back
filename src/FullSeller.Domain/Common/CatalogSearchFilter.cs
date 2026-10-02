namespace FullSeller.Domain.Common;

public class CatalogSearchFilter
{
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? SearchText { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } // "popularity" | "price_asc" | "price_desc" | "newest"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
