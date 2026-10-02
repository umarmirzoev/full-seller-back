using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Interfaces;

/// <summary>Количество заказов в конкретном статусе — для дашборда администратора.</summary>
public record OrderStatusCount(OrderStatus Status, int Count);

/// <summary>Сводная статистика по заказам для дашборда администратора.</summary>
public record OrderStats(int TotalOrders, int NewOrdersToday, decimal TotalRevenue, decimal RevenueToday, IReadOnlyList<OrderStatusCount> ByStatus);

/// <summary>Строка списка заказов в админ-панели — заказ + краткие данные покупателя, без позиций заказа.</summary>
public record AdminOrderSummary(
    Guid Id, string OrderNumber, Guid UserId, string UserPhone, string? UserFullName,
    OrderStatus Status, decimal TotalAmount, decimal DeliveryCost, DateTime CreatedAt);

public interface IOrderRepository
{
    Task<Guid> CreateAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Order>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<OrderStatusEvent>> GetStatusHistoryAsync(Guid orderId, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid orderId, OrderStatus status, string? cargoTrackingNumber, CancellationToken ct = default);
    Task AddStatusEventAsync(OrderStatusEvent statusEvent, CancellationToken ct = default);
    Task<bool> UserHasDeliveredOrderWithProductAsync(Guid userId, Guid productId, CancellationToken ct = default);

    /// <summary>Список всех заказов (всех покупателей) для админ-панели: фильтр по статусу, поиск по номеру заказа/телефону, постранично.</summary>
    Task<PagedResult<AdminOrderSummary>> GetAllAsync(int page, int pageSize, OrderStatus? status = null, string? search = null, CancellationToken ct = default);

    Task<OrderStats> GetStatsAsync(CancellationToken ct = default);
}
