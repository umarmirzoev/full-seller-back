using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class CartRepository : SqlRepositoryBase, ICartRepository
{
    public CartRepository(ISqlConnectionFactory factory) : base(factory) { }
    public CartRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        Guid cartId;
        using (var find = CreateCommand(conn, tx, "SELECT Id FROM Carts WHERE UserId = @UserId"))
        {
            find.Parameters.AddWithValue("@UserId", userId);
            var existing = await find.ExecuteScalarAsync(ct);
            if (existing is Guid g)
            {
                cartId = g;
            }
            else
            {
                cartId = Guid.NewGuid();
                using var insert = CreateCommand(conn, tx, "INSERT INTO Carts (Id, UserId) VALUES (@Id, @UserId)");
                insert.Parameters.AddWithValue("@Id", cartId);
                insert.Parameters.AddWithValue("@UserId", userId);
                await insert.ExecuteNonQueryAsync(ct);
            }
        }

        using var itemsCmd = CreateCommand(conn, tx,
            "SELECT Id, CartId, ProductVariantId, Quantity FROM CartItems WHERE CartId = @CartId");
        itemsCmd.Parameters.AddWithValue("@CartId", cartId);
        using var reader = await itemsCmd.ExecuteReaderAsync(ct);
        var items = new List<CartItem>();
        while (await reader.ReadAsync(ct))
        {
            items.Add(new CartItem
            {
                Id = reader.GetGuid(0),
                CartId = reader.GetGuid(1),
                ProductVariantId = reader.GetGuid(2),
                Quantity = reader.GetInt32(3),
            });
        }
        return new Cart { Id = cartId, UserId = userId, Items = items };
    }, ct);

    public Task<Guid> AddItemAsync(Guid cartId, Guid productVariantId, int quantity, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var find = CreateCommand(conn, tx,
            "SELECT Id, Quantity FROM CartItems WHERE CartId = @CartId AND ProductVariantId = @VariantId");
        find.Parameters.AddWithValue("@CartId", cartId);
        find.Parameters.AddWithValue("@VariantId", productVariantId);
        using var reader = await find.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            var currentQty = reader.GetInt32(1);
            await reader.DisposeAsync();
            using var update = CreateCommand(conn, tx, "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @Id");
            update.Parameters.AddWithValue("@Quantity", currentQty + quantity);
            update.Parameters.AddWithValue("@Id", id);
            await update.ExecuteNonQueryAsync(ct);
            return id;
        }
        await reader.DisposeAsync();

        var newId = Guid.NewGuid();
        using var insert = CreateCommand(conn, tx, """
            INSERT INTO CartItems (Id, CartId, ProductVariantId, Quantity)
            VALUES (@Id, @CartId, @VariantId, @Quantity)
            """);
        insert.Parameters.AddWithValue("@Id", newId);
        insert.Parameters.AddWithValue("@CartId", cartId);
        insert.Parameters.AddWithValue("@VariantId", productVariantId);
        insert.Parameters.AddWithValue("@Quantity", quantity);
        await insert.ExecuteNonQueryAsync(ct);
        return newId;
    }, ct);

    public Task UpdateQuantityAsync(Guid cartItemId, int quantity, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE CartItems SET Quantity = @Quantity WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Quantity", quantity);
        cmd.Parameters.AddWithValue("@Id", cartItemId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task RemoveItemAsync(Guid cartItemId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM CartItems WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", cartItemId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task ClearAsync(Guid cartId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM CartItems WHERE CartId = @CartId");
        cmd.Parameters.AddWithValue("@CartId", cartId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);
}
