using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Exceptions;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Orders;
using FullSeller.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IOrderRepository _orders;
    private readonly ICatalogRepository _catalog;

    public OrdersController(OrderService orderService, IOrderRepository orders, ICatalogRepository catalog)
    {
        _orderService = orderService;
        _orders = orders;
        _catalog = catalog;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateOrderFromCartAsync(User.GetUserId(), request.AddressId, request.DeliveryCountry, request.PaymentMethod, ct);
        return Ok(await ToDtoAsync(order, ct));
    }

    [HttpGet]
    public async Task<ActionResult<OrderListResponse>> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _orders.GetByUserIdAsync(User.GetUserId(), page, pageSize, ct);
        var items = new List<OrderDto>();
        foreach (var order in result.Items) items.Add(await ToDtoAsync(order, ct));
        return Ok(new OrderListResponse(items, result.TotalCount, result.Page, result.PageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        if (order.UserId != User.GetUserId() && !User.IsManagerOrAdmin()) return Forbid();
        return Ok(await ToDtoAsync(order, ct));
    }

    [HttpGet("{id:guid}/status-history")]
    public async Task<ActionResult<IReadOnlyList<OrderStatusEventDto>>> GetStatusHistory(Guid id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        if (order.UserId != User.GetUserId() && !User.IsManagerOrAdmin()) return Forbid();

        var history = await _orders.GetStatusHistoryAsync(id, ct);
        return Ok(history.Select(h => new OrderStatusEventDto(h.Status, h.Comment, h.ChangedAt)).ToList());
    }

    [HttpPost("{id:guid}/repeat")]
    public async Task<IActionResult> RepeatOrder(Guid id, CancellationToken ct)
    {
        await _orderService.RepeatOrderAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    /// <summary>Смена статуса заказа — доступно только менеджеру/админу.</summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        if (!User.IsManagerOrAdmin()) return Forbid();

        var order = await _orders.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        await _orders.UpdateStatusAsync(id, request.Status, request.CargoTrackingNumber, ct);
        await _orders.AddStatusEventAsync(new OrderStatusEvent
        {
            OrderId = id,
            Status = request.Status,
            Comment = request.Comment,
        }, ct);

        return NoContent();
    }

    private async Task<OrderDto> ToDtoAsync(Order order, CancellationToken ct)
    {
        var items = new List<OrderItemDto>();
        foreach (var item in order.Items)
        {
            var variant = await _catalog.GetVariantByIdAsync(item.ProductVariantId, ct);
            var product = variant is not null ? await _catalog.GetProductByIdAsync(variant.ProductId, ct) : null;
            items.Add(new OrderItemDto(item.ProductVariantId, product?.Id ?? Guid.Empty, product?.Name ?? "—", item.Quantity, item.UnitPriceAtOrderTime));
        }

        return new OrderDto(
            order.Id, order.OrderNumber, order.Status, order.DeliveryCountry, order.PaymentMethod,
            order.TotalAmount, order.DeliveryCost, order.CargoTrackingNumber, order.CreatedAt, items);
    }
}
