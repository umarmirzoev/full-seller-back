using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class FavoriteRepository : SqlRepositoryBase, IFavoriteRepository
{
    public FavoriteRepository(ISqlConnectionFactory factory) : base(factory) { }
    public FavoriteRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT Id, UserId, ProductId FROM Favorites WHERE UserId = @UserId");
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Favorite>();
        while (await reader.ReadAsync(ct))
            list.Add(new Favorite { Id = reader.GetGuid(0), UserId = reader.GetGuid(1), ProductId = reader.GetGuid(2) });
        return (IReadOnlyList<Favorite>)list;
    }, ct);

    public Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var check = CreateCommand(conn, tx, "SELECT COUNT(*) FROM Favorites WHERE UserId = @UserId AND ProductId = @ProductId");
        check.Parameters.AddWithValue("@UserId", userId);
        check.Parameters.AddWithValue("@ProductId", productId);
        var exists = (int)(long)(await check.ExecuteScalarAsync(ct) ?? 0L) > 0;
        if (exists) return;

        using var cmd = CreateCommand(conn, tx, "INSERT INTO Favorites (Id, UserId, ProductId) VALUES (@Id, @UserId, @ProductId)");
        cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM Favorites WHERE UserId = @UserId AND ProductId = @ProductId");
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);
}
