using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class RefreshTokenRepository : SqlRepositoryBase, IRefreshTokenRepository
{
    public RefreshTokenRepository(ISqlConnectionFactory factory) : base(factory) { }
    public RefreshTokenRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<Guid> CreateAsync(RefreshToken token, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        token.Id = token.Id == Guid.Empty ? Guid.NewGuid() : token.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO RefreshTokens (Id, UserId, TokenHash, ExpiresAt, RevokedAt, CreatedAt)
            VALUES (@Id, @UserId, @TokenHash, @ExpiresAt, @RevokedAt, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", token.Id);
        cmd.Parameters.AddWithValue("@UserId", token.UserId);
        cmd.Parameters.AddWithValue("@TokenHash", token.TokenHash);
        cmd.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);
        cmd.Parameters.AddWithValue("@RevokedAt", (object?)token.RevokedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return token.Id;
    }, ct);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, UserId, TokenHash, ExpiresAt, RevokedAt, CreatedAt
            FROM RefreshTokens WHERE TokenHash = @TokenHash
            """);
        cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task RevokeAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE RefreshTokens SET RevokedAt = now() WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static RefreshToken Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        UserId = r.GetGuid(1),
        TokenHash = r.GetString(2),
        ExpiresAt = r.GetDateTime(3),
        RevokedAt = r.IsDBNull(4) ? null : r.GetDateTime(4),
        CreatedAt = r.GetDateTime(5),
    };
}
