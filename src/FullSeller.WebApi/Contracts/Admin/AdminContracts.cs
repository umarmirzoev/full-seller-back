using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Admin;

public record AdminUserDto(
    Guid Id, string Phone, string? FullName, UserRole Role, bool IsLegalEntity,
    string? LegalName, int LoyaltyPoints, DateTime CreatedAt);

public record AdminUserListResponse(IReadOnlyList<AdminUserDto> Items, int TotalCount, int Page, int PageSize);

public record UpdateUserRoleRequest(UserRole Role);

public record AdminOrderDto(
    Guid Id, string OrderNumber, Guid UserId, string UserPhone, string? UserFullName,
    OrderStatus Status, decimal TotalAmount, decimal DeliveryCost, DateTime CreatedAt);

public record AdminOrderListResponse(IReadOnlyList<AdminOrderDto> Items, int TotalCount, int Page, int PageSize);

public record OrderStatusCountDto(OrderStatus Status, int Count);

public record AdminDashboardDto(
    int TotalUsers, int NewUsersToday, int NewUsersThisWeek,
    int TotalOrders, int NewOrdersToday,
    decimal TotalRevenue, decimal RevenueToday,
    IReadOnlyList<OrderStatusCountDto> OrdersByStatus);
