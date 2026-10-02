using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class PromoCodeRepository : SqlRepositoryBase, IPromoCodeRepository
{
    public PromoCodeRepository(ISqlConnectionFactory factory) : base(factory) { }
    public PromoCodeRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<PromoCode>> GetAllAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Code, DiscountType, DiscountValue, ExpiresAt, UsageLimit, TimesUsed
            FROM PromoCodes ORDER BY Code
            """);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PromoCode>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<PromoCode>)list;
    }, ct);

    public Task<PromoCode?> GetByCodeAsync(string code, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Code, DiscountType, DiscountValue, ExpiresAt, UsageLimit, TimesUsed
            FROM PromoCodes WHERE UPPER(Code) = UPPER(@Code)
            """);
        cmd.Parameters.AddWithValue("@Code", code);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<PromoCode?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Code, DiscountType, DiscountValue, ExpiresAt, UsageLimit, TimesUsed
            FROM PromoCodes WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<Guid> CreateAsync(PromoCode promoCode, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        promoCode.Id = promoCode.Id == Guid.Empty ? Guid.NewGuid() : promoCode.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO PromoCodes (Id, Code, DiscountType, DiscountValue, ExpiresAt, UsageLimit, TimesUsed)
            VALUES (@Id, @Code, @DiscountType, @DiscountValue, @ExpiresAt, @UsageLimit, 0)
            """);
        cmd.Parameters.AddWithValue("@Id", promoCode.Id);
        cmd.Parameters.AddWithValue("@Code", promoCode.Code.ToUpperInvariant());
        cmd.Parameters.AddWithValue("@DiscountType", (int)promoCode.DiscountType);
        cmd.Parameters.AddWithValue("@DiscountValue", promoCode.DiscountValue);
        cmd.Parameters.AddWithValue("@ExpiresAt", (object?)promoCode.ExpiresAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UsageLimit", (object?)promoCode.UsageLimit ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        return promoCode.Id;
    }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM PromoCodes WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task IncrementUsageAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE PromoCodes SET TimesUsed = TimesUsed + 1 WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static PromoCode Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Code = r.GetString(1),
        DiscountType = (DiscountType)r.GetInt32(2),
        DiscountValue = r.GetDecimal(3),
        ExpiresAt = r.IsDBNull(4) ? null : r.GetDateTime(4),
        UsageLimit = r.IsDBNull(5) ? null : r.GetInt32(5),
        TimesUsed = r.GetInt32(6),
    };
}
