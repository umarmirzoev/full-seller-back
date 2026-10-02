using FullSeller.Domain.Enums;
using FullSeller.Domain.Exceptions;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Панель администратора: сводная статистика, список пользователей, список заказов, смена ролей.
/// Доступ — только Admin и SuperAdmin (см. AdminOnlyAttribute); Manager сюда не допускается.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize]
[AdminOnly]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IOrderRepository _orders;

    public AdminController(IUserRepository users, IOrderRepository orders)
    {
        _users = users;
        _orders = orders;
    }

    /// <summary>Сводка для главного экрана панели: пользователи, заказы, выручка, разбивка по статусам.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(CancellationToken ct)
    {
        var userStats = await _users.GetStatsAsync(ct);
        var orderStats = await _orders.GetStatsAsync(ct);

        return Ok(new AdminDashboardDto(
            userStats.TotalUsers, userStats.NewUsersToday, userStats.NewUsersThisWeek,
            orderStats.TotalOrders, orderStats.NewOrdersToday,
            orderStats.TotalRevenue, orderStats.RevenueToday,
            orderStats.ByStatus.Select(s => new OrderStatusCountDto(s.Status, s.Count)).ToList()));
    }

    /// <summary>Список всех зарегистрированных пользователей — появляются здесь сразу после регистрации.</summary>
    [HttpGet("users")]
    public async Task<ActionResult<AdminUserListResponse>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] UserRole? role = null, CancellationToken ct = default)
    {
        var result = await _users.GetAllAsync(page, pageSize, search, role, ct);
        var items = result.Items
            .Select(u => new AdminUserDto(u.Id, u.Phone, u.FullName, u.Role, u.IsLegalEntity, u.LegalName, u.LoyaltyPoints, u.CreatedAt))
            .ToList();
        return Ok(new AdminUserListResponse(items, result.TotalCount, result.Page, result.PageSize));
    }

    /// <summary>Смена роли пользователя. Назначать/менять роли Admin и SuperAdmin может только SuperAdmin —
    /// это защищает от того, что обычный админ повысит себя или кого-то ещё до полного доступа.</summary>
    [HttpPut("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        var target = await _users.GetByIdAsync(id, ct) ?? throw new NotFoundException("Пользователь", id);

        var escalatingToPrivileged = request.Role is UserRole.Admin or UserRole.SuperAdmin;
        var targetCurrentlyPrivileged = target.Role is UserRole.Admin or UserRole.SuperAdmin;
        if ((escalatingToPrivileged || targetCurrentlyPrivileged) && !User.IsSuperAdmin())
            throw new UnauthorizedDomainException("Назначать или изменять роль Admin/SuperAdmin может только SuperAdmin.");

        await _users.SetRoleAsync(id, request.Role, ct);
        return NoContent();
    }

    /// <summary>Список всех заказов всех покупателей — появляются здесь сразу после оформления.
    /// Фильтр по статусу и поиск по номеру заказа/телефону покупателя.</summary>
    [HttpGet("orders")]
    public async Task<ActionResult<AdminOrderListResponse>> GetOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _orders.GetAllAsync(page, pageSize, status, search, ct);
        var items = result.Items
            .Select(o => new AdminOrderDto(o.Id, o.OrderNumber, o.UserId, o.UserPhone, o.UserFullName, o.Status, o.TotalAmount, o.DeliveryCost, o.CreatedAt))
            .ToList();
        return Ok(new AdminOrderListResponse(items, result.TotalCount, result.Page, result.PageSize));
    }
}
