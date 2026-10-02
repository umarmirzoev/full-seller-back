using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favorites;
    private readonly ICatalogRepository _catalog;

    public FavoritesController(IFavoriteRepository favorites, ICatalogRepository catalog)
    {
        _favorites = favorites;
        _catalog = catalog;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetFavorites(CancellationToken ct)
    {
        var favorites = await _favorites.GetByUserIdAsync(User.GetUserId(), ct);
        var items = new List<ProductListItemDto>();
        foreach (var fav in favorites)
        {
            var product = await _catalog.GetProductByIdAsync(fav.ProductId, ct);
            if (product is null) continue;
            items.Add(new ProductListItemDto(product.Id, product.Name, product.CategoryId, product.BrandId, product.MinPrice, product.Rating, product.ReviewsCount, product.ImageUrls));
        }
        return Ok(items);
    }

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> Add(Guid productId, CancellationToken ct)
    {
        await _favorites.AddAsync(User.GetUserId(), productId, ct);
        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        await _favorites.RemoveAsync(User.GetUserId(), productId, ct);
        return NoContent();
    }
}
