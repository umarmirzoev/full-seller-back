using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Orders;

public record CreateOrderRequest(Guid AddressId, DeliveryCountry DeliveryCountry, PaymentMethod PaymentMethod);

public record OrderItemDto(Guid ProductVariantId, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record OrderStatusEventDto(OrderStatus Status, string? Comment, DateTime ChangedAt);

public record OrderDto(
    Guid Id, string OrderNumber, OrderStatus Status, DeliveryCountry DeliveryCountry,
    PaymentMethod PaymentMethod, decimal TotalAmount, decimal DeliveryCost,
    string? CargoTrackingNumber, DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items);

public record OrderListResponse(IReadOnlyList<OrderDto> Items, int TotalCount, int Page, int PageSize);
public record UpdateOrderStatusRequest(OrderStatus Status, string? CargoTrackingNumber, string? Comment);
