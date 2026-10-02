using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartRepository _carts;
    private readonly ICatalogRepository _catalog;

    public CartController(ICartRepository carts, ICatalogRepository catalog)
    {
        _carts = carts;
        _catalog = catalog;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken ct)
    {
        var cart = await _carts.GetOrCreateByUserIdAsync(User.GetUserId(), ct);
        return Ok(await ToDtoAsync(cart, ct));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        var cart = await _carts.GetOrCreateByUserIdAsync(User.GetUserId(), ct);
        await _carts.AddItemAsync(cart.Id, request.ProductVariantId, request.Quantity, ct);
        cart = await _carts.GetOrCreateByUserIdAsync(User.GetUserId(), ct);
        return Ok(await ToDtoAsync(cart, ct));
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            await _carts.RemoveItemAsync(itemId, ct);
            return NoContent();
        }
        await _carts.UpdateQuantityAsync(itemId, request.Quantity, ct);
        return NoContent();
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        await _carts.RemoveItemAsync(itemId, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var cart = await _carts.GetOrCreateByUserIdAsync(User.GetUserId(), ct);
        await _carts.ClearAsync(cart.Id, ct);
        return NoContent();
    }

    /// <summary>Массовая загрузка позиций корзины по артикулам (для оптовых клиентов, загрузка списком/Excel-CSV).</summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkCartUploadResponse>> BulkUpload([FromBody] BulkCartUploadRequest request, CancellationToken ct)
    {
        var cart = await _carts.GetOrCreateByUserIdAsync(User.GetUserId(), ct);
        var results = new List<BulkCartUploadResultRow>();

        foreach (var row in request.Rows)
        {
            var variant = await _catalog.SearchProductsAsync(new CatalogSearchFilter { SearchText = row.Sku, PageSize = 1 }, ct);
            var product = variant.Items.FirstOrDefault();
            if (product is null)
            {
                results.Add(new BulkCartUploadResultRow(row.Sku, row.Quantity, false, "Товар с таким артикулом не найден."));
                continue;
            }

            var variants = await _catalog.GetVariantsByProductIdAsync(product.Id, ct);
            var match = variants.FirstOrDefault(v => v.Sku == row.Sku) ?? variants.FirstOrDefault();
            if (match is null)
            {
                results.Add(new BulkCartUploadResultRow(row.Sku, row.Quantity, false, "У товара нет доступных вариантов."));
                continue;
            }

            await _carts.AddItemAsync(cart.Id, match.Id, row.Quantity, ct);
            results.Add(new BulkCartUploadResultRow(row.Sku, row.Quantity, true, null));
        }

        return Ok(new BulkCartUploadResponse(results));
    }

    private async Task<CartDto> ToDtoAsync(Cart cart, CancellationToken ct)
    {
        var items = new List<CartItemDto>();
        var totalWeight = 0;
        decimal itemsTotal = 0;

        foreach (var item in cart.Items)
        {
            var variant = await _catalog.GetVariantByIdAsync(item.ProductVariantId, ct);
            if (variant is null) continue;
            var product = await _catalog.GetProductByIdAsync(variant.ProductId, ct);
            if (product is null) continue;

            var tiers = await _catalog.GetPriceTiersAsync(product.Id, ct);
            var unitPrice = tiers.Count == 0
                ? product.MinPrice
                : (tiers.Where(t => t.MinQuantity <= item.Quantity).OrderByDescending(t => t.MinQuantity).FirstOrDefault()
                    ?? tiers.OrderBy(t => t.MinQuantity).First()).PricePerUnit;

            items.Add(new CartItemDto(item.Id, variant.Id, product.Id, product.Name, item.Quantity, unitPrice, unitPrice * item.Quantity));
            totalWeight += product.WeightGrams * item.Quantity;
            itemsTotal += unitPrice * item.Quantity;
        }

        return new CartDto(cart.Id, items, totalWeight, itemsTotal);
    }
}
