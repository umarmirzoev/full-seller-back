using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class OtpRepository : SqlRepositoryBase, IOtpRepository
{
    public OtpRepository(ISqlConnectionFactory factory) : base(factory) { }
    public OtpRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<Guid> CreateAsync(OtpCode otp, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        otp.Id = otp.Id == Guid.Empty ? Guid.NewGuid() : otp.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO OtpCodes (Id, Phone, CodeHash, ExpiresAt, Attempts, IsUsed, CreatedAt)
            VALUES (@Id, @Phone, @CodeHash, @ExpiresAt, @Attempts, @IsUsed, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", otp.Id);
        cmd.Parameters.AddWithValue("@Phone", otp.Phone);
        cmd.Parameters.AddWithValue("@CodeHash", otp.CodeHash);
        cmd.Parameters.AddWithValue("@ExpiresAt", otp.ExpiresAt);
        cmd.Parameters.AddWithValue("@Attempts", otp.Attempts);
        cmd.Parameters.AddWithValue("@IsUsed", otp.IsUsed);
        cmd.Parameters.AddWithValue("@CreatedAt", otp.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return otp.Id;
    }, ct);

    public Task<OtpCode?> GetActiveByPhoneAsync(string phone, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Phone, CodeHash, ExpiresAt, Attempts, IsUsed, CreatedAt
            FROM OtpCodes
            WHERE Phone = @Phone AND IsUsed = FALSE AND ExpiresAt > now()
            ORDER BY CreatedAt DESC
            LIMIT 1
            """);
        cmd.Parameters.AddWithValue("@Phone", phone);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task IncrementAttemptsAsync(Guid otpId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE OtpCodes SET Attempts = Attempts + 1 WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", otpId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task MarkUsedAsync(Guid otpId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE OtpCodes SET IsUsed = TRUE WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", otpId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static OtpCode Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Phone = r.GetString(1),
        CodeHash = r.GetString(2),
        ExpiresAt = r.GetDateTime(3),
        Attempts = r.GetInt32(4),
        IsUsed = r.GetBoolean(5),
        CreatedAt = r.GetDateTime(6),
    };
}
