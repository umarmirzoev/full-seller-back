using System.Text;
using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class OrderRepository : SqlRepositoryBase, IOrderRepository
{
    public OrderRepository(ISqlConnectionFactory factory) : base(factory) { }
    public OrderRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<Guid> CreateAsync(Order order, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        // Если репозиторий работает автономно (без UnitOfWork), сами открываем локальную транзакцию,
        // чтобы вставка заказа и его позиций были атомарны.
        var ownsTransaction = tx is null;
        var transaction = tx ?? (NpgsqlTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            order.Id = order.Id == Guid.Empty ? Guid.NewGuid() : order.Id;

            using (var cmd = CreateCommand(conn, transaction, """
                INSERT INTO Orders (Id, OrderNumber, UserId, Status, DeliveryCountry, AddressId,
                                     PaymentMethod, TotalAmount, DeliveryCost, CargoTrackingNumber, CreatedAt)
                VALUES (@Id, @OrderNumber, @UserId, @Status, @DeliveryCountry, @AddressId,
                        @PaymentMethod, @TotalAmount, @DeliveryCost, @CargoTrackingNumber, @CreatedAt)
                """))
            {
                cmd.Parameters.AddWithValue("@Id", order.Id);
                cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                cmd.Parameters.AddWithValue("@UserId", order.UserId);
                cmd.Parameters.AddWithValue("@Status", (int)order.Status);
                cmd.Parameters.AddWithValue("@DeliveryCountry", (int)order.DeliveryCountry);
                cmd.Parameters.AddWithValue("@AddressId", order.AddressId);
                cmd.Parameters.AddWithValue("@PaymentMethod", (int)order.PaymentMethod);
                cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                cmd.Parameters.AddWithValue("@DeliveryCost", order.DeliveryCost);
                cmd.Parameters.AddWithValue("@CargoTrackingNumber", (object?)order.CargoTrackingNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            foreach (var item in order.Items)
            {
                item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                using var cmd = CreateCommand(conn, transaction, """
                    INSERT INTO OrderItems (Id, OrderId, ProductVariantId, Quantity, UnitPriceAtOrderTime)
                    VALUES (@Id, @OrderId, @ProductVariantId, @Quantity, @UnitPrice)
                    """);
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.Parameters.AddWithValue("@OrderId", order.Id);
                cmd.Parameters.AddWithValue("@ProductVariantId", item.ProductVariantId);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPriceAtOrderTime);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            using (var cmd = CreateCommand(conn, transaction, """
                INSERT INTO OrderStatusEvents (Id, OrderId, Status, Comment, ChangedByUserId, ChangedAt)
                VALUES (@Id, @OrderId, @Status, @Comment, NULL, @ChangedAt)
                """))
            {
                cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@OrderId", order.Id);
                cmd.Parameters.AddWithValue("@Status", (int)order.Status);
                cmd.Parameters.AddWithValue("@Comment", "Заказ создан");
                cmd.Parameters.AddWithValue("@ChangedAt", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (ownsTransaction) await transaction.CommitAsync(ct);
            return order.Id;
        }
        catch
        {
            if (ownsTransaction) await transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (ownsTransaction) await transaction.DisposeAsync();
        }
    }, ct);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        Order? order;
        using (var cmd = CreateCommand(conn, tx, """
            SELECT Id, OrderNumber, UserId, Status, DeliveryCountry, AddressId,
                   PaymentMethod, TotalAmount, DeliveryCost, CargoTrackingNumber, CreatedAt
            FROM Orders WHERE Id = @Id
            """))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;
            order = MapOrder(reader);
        }

        using (var itemsCmd = CreateCommand(conn, tx,
            "SELECT Id, OrderId, ProductVariantId, Quantity, UnitPriceAtOrderTime FROM OrderItems WHERE OrderId = @OrderId"))
        {
            itemsCmd.Parameters.AddWithValue("@OrderId", id);
            using var reader = await itemsCmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                order.Items.Add(new OrderItem
                {
                    Id = reader.GetGuid(0),
                    OrderId = reader.GetGuid(1),
                    ProductVariantId = reader.GetGuid(2),
                    Quantity = reader.GetInt32(3),
                    UnitPriceAtOrderTime = reader.GetDecimal(4),
                });
            }
        }
        return order;
    }, ct);

    public Task<PagedResult<Order>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var countCmd = CreateCommand(conn, tx, "SELECT COUNT(*) FROM Orders WHERE UserId = @UserId");
        countCmd.Parameters.AddWithValue("@UserId", userId);
        var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, OrderNumber, UserId, Status, DeliveryCountry, AddressId,
                   PaymentMethod, TotalAmount, DeliveryCost, CargoTrackingNumber, CreatedAt
            FROM Orders WHERE UserId = @UserId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var items = new List<Order>();
        while (await reader.ReadAsync(ct)) items.Add(MapOrder(reader));
        return new PagedResult<Order> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }, ct);

    public Task<IReadOnlyList<OrderStatusEvent>> GetStatusHistoryAsync(Guid orderId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, OrderId, Status, Comment, ChangedByUserId, ChangedAt
            FROM OrderStatusEvents WHERE OrderId = @OrderId ORDER BY ChangedAt
            """);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<OrderStatusEvent>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new OrderStatusEvent
            {
                Id = reader.GetGuid(0),
                OrderId = reader.GetGuid(1),
                Status = (OrderStatus)reader.GetInt32(2),
                Comment = reader.IsDBNull(3) ? null : reader.GetString(3),
                ChangedByUserId = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                ChangedAt = reader.GetDateTime(5),
            });
        }
        return (IReadOnlyList<OrderStatusEvent>)list;
    }, ct);

    public Task UpdateStatusAsync(Guid orderId, OrderStatus status, string? cargoTrackingNumber, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE Orders SET Status = @Status,
                   CargoTrackingNumber = COALESCE(@CargoTrackingNumber, CargoTrackingNumber)
            WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Status", (int)status);
        cmd.Parameters.AddWithValue("@CargoTrackingNumber", (object?)cargoTrackingNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", orderId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task AddStatusEventAsync(OrderStatusEvent statusEvent, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        statusEvent.Id = statusEvent.Id == Guid.Empty ? Guid.NewGuid() : statusEvent.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO OrderStatusEvents (Id, OrderId, Status, Comment, ChangedByUserId, ChangedAt)
            VALUES (@Id, @OrderId, @Status, @Comment, @ChangedByUserId, @ChangedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", statusEvent.Id);
        cmd.Parameters.AddWithValue("@OrderId", statusEvent.OrderId);
        cmd.Parameters.AddWithValue("@Status", (int)statusEvent.Status);
        cmd.Parameters.AddWithValue("@Comment", (object?)statusEvent.Comment ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ChangedByUserId", (object?)statusEvent.ChangedByUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ChangedAt", statusEvent.ChangedAt);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<bool> UserHasDeliveredOrderWithProductAsync(Guid userId, Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT COUNT(*) FROM Orders o
            JOIN OrderItems oi ON oi.OrderId = o.Id
            JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId
            WHERE o.UserId = @UserId AND pv.ProductId = @ProductId AND o.Status = @DeliveredStatus
            """);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        cmd.Parameters.AddWithValue("@DeliveredStatus", (int)OrderStatus.Delivered);
        var count = (int)(long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
        return count > 0;
    }, ct);

    public Task<PagedResult<AdminOrderSummary>> GetAllAsync(int page, int pageSize, OrderStatus? status = null, string? search = null, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var where = new StringBuilder("WHERE 1=1");
        if (status is not null) where.Append(" AND o.Status = @Status");
        if (!string.IsNullOrWhiteSpace(search)) where.Append(" AND (o.OrderNumber ILIKE @Search OR u.Phone ILIKE @Search)");

        using var countCmd = CreateCommand(conn, tx, $"SELECT COUNT(*) FROM Orders o JOIN Users u ON u.Id = o.UserId {where}");
        AddAdminFilterParams(countCmd, status, search);
        var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

        using var cmd = CreateCommand(conn, tx, $"""
            SELECT o.Id, o.OrderNumber, o.UserId, u.Phone, u.FullName,
                   o.Status, o.TotalAmount, o.DeliveryCost, o.CreatedAt
            FROM Orders o
            JOIN Users u ON u.Id = o.UserId
            {where}
            ORDER BY o.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """);
        AddAdminFilterParams(cmd, status, search);
        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var items = new List<AdminOrderSummary>();
        while (await reader.ReadAsync(ct))
        {
            items.Add(new AdminOrderSummary(
                reader.GetGuid(0), reader.GetString(1), reader.GetGuid(2), reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                (OrderStatus)reader.GetInt32(5), reader.GetDecimal(6), reader.GetDecimal(7), reader.GetDateTime(8)));
        }
        return new PagedResult<AdminOrderSummary> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }, ct);

    private static void AddAdminFilterParams(NpgsqlCommand cmd, OrderStatus? status, string? search)
    {
        if (status is not null) cmd.Parameters.AddWithValue("@Status", (int)status.Value);
        if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@Search", $"%{search}%");
    }

    public Task<OrderStats> GetStatsAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var todayStart = DateTime.UtcNow.Date;
        int totalOrders, newOrdersToday;
        decimal totalRevenue, revenueToday;

        using (var cmd = CreateCommand(conn, tx, """
            SELECT COUNT(*),
                   COUNT(*) FILTER (WHERE CreatedAt >= @TodayStart),
                   COALESCE(SUM(TotalAmount) FILTER (WHERE Status <> @Cancelled), 0),
                   COALESCE(SUM(TotalAmount) FILTER (WHERE Status <> @Cancelled AND CreatedAt >= @TodayStart), 0)
            FROM Orders
            """))
        {
            cmd.Parameters.AddWithValue("@TodayStart", todayStart);
            cmd.Parameters.AddWithValue("@Cancelled", (int)OrderStatus.Cancelled);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);
            totalOrders = (int)reader.GetInt64(0);
            newOrdersToday = (int)reader.GetInt64(1);
            totalRevenue = reader.GetDecimal(2);
            revenueToday = reader.GetDecimal(3);
        }

        var byStatus = new List<OrderStatusCount>();
        using (var cmd = CreateCommand(conn, tx, "SELECT Status, COUNT(*) FROM Orders GROUP BY Status ORDER BY Status"))
        {
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                byStatus.Add(new OrderStatusCount((OrderStatus)reader.GetInt32(0), (int)reader.GetInt64(1)));
        }

        return new OrderStats(totalOrders, newOrdersToday, totalRevenue, revenueToday, byStatus);
    }, ct);

    private static Order MapOrder(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        OrderNumber = r.GetString(1),
        UserId = r.GetGuid(2),
        Status = (OrderStatus)r.GetInt32(3),
        DeliveryCountry = (DeliveryCountry)r.GetInt32(4),
        AddressId = r.GetGuid(5),
        PaymentMethod = (PaymentMethod)r.GetInt32(6),
        TotalAmount = r.GetDecimal(7),
        DeliveryCost = r.GetDecimal(8),
        CargoTrackingNumber = r.IsDBNull(9) ? null : r.GetString(9),
        CreatedAt = r.GetDateTime(10),
    };
}
