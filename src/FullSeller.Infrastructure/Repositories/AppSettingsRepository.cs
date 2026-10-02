using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

/// <summary>Таблица AppSettings всегда содержит ровно одну строку (создаётся миграцией 0002 со значениями по умолчанию).
/// GetAsync подстраховывается и создаёт строку сам, если её вдруг нет — чтобы сервис не падал на пустой БД.</summary>
public class AppSettingsRepository : SqlRepositoryBase, IAppSettingsRepository
{
    public AppSettingsRepository(ISqlConnectionFactory factory) : base(factory) { }
    public AppSettingsRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<AppSettings> GetAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using (var cmd = CreateCommand(conn, tx, """
            SELECT Id, UsdToRubRate, RateUpdatedAt, MinOrderAmount, ContactPhone, ContactAddress, WhatsAppUrl, TelegramUrl, UpdatedAt
            FROM AppSettings LIMIT 1
            """))
        {
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct)) return Map(reader);
        }

        var defaults = new AppSettings
        {
            Id = Guid.NewGuid(),
            UsdToRubRate = 90m,
            RateUpdatedAt = DateTime.UtcNow,
            MinOrderAmount = 2000m,
            UpdatedAt = DateTime.UtcNow,
        };
        using (var insert = CreateCommand(conn, tx, """
            INSERT INTO AppSettings (Id, UsdToRubRate, RateUpdatedAt, MinOrderAmount, ContactPhone, ContactAddress, WhatsAppUrl, TelegramUrl, UpdatedAt)
            VALUES (@Id, @UsdToRubRate, @RateUpdatedAt, @MinOrderAmount, NULL, NULL, NULL, NULL, @UpdatedAt)
            """))
        {
            insert.Parameters.AddWithValue("@Id", defaults.Id);
            insert.Parameters.AddWithValue("@UsdToRubRate", defaults.UsdToRubRate);
            insert.Parameters.AddWithValue("@RateUpdatedAt", defaults.RateUpdatedAt);
            insert.Parameters.AddWithValue("@MinOrderAmount", defaults.MinOrderAmount);
            insert.Parameters.AddWithValue("@UpdatedAt", defaults.UpdatedAt);
            await insert.ExecuteNonQueryAsync(ct);
        }
        return defaults;
    }, ct);

    public Task UpdateAsync(AppSettings settings, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        settings.UpdatedAt = DateTime.UtcNow;
        using var cmd = CreateCommand(conn, tx, """
            UPDATE AppSettings SET
                UsdToRubRate = @UsdToRubRate, RateUpdatedAt = @RateUpdatedAt, MinOrderAmount = @MinOrderAmount,
                ContactPhone = @ContactPhone, ContactAddress = @ContactAddress,
                WhatsAppUrl = @WhatsAppUrl, TelegramUrl = @TelegramUrl, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", settings.Id);
        cmd.Parameters.AddWithValue("@UsdToRubRate", settings.UsdToRubRate);
        cmd.Parameters.AddWithValue("@RateUpdatedAt", settings.RateUpdatedAt);
        cmd.Parameters.AddWithValue("@MinOrderAmount", settings.MinOrderAmount);
        cmd.Parameters.AddWithValue("@ContactPhone", (object?)settings.ContactPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContactAddress", (object?)settings.ContactAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@WhatsAppUrl", (object?)settings.WhatsAppUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TelegramUrl", (object?)settings.TelegramUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpdatedAt", settings.UpdatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static AppSettings Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        UsdToRubRate = r.GetDecimal(1),
        RateUpdatedAt = r.GetDateTime(2),
        MinOrderAmount = r.GetDecimal(3),
        ContactPhone = r.IsDBNull(4) ? null : r.GetString(4),
        ContactAddress = r.IsDBNull(5) ? null : r.GetString(5),
        WhatsAppUrl = r.IsDBNull(6) ? null : r.GetString(6),
        TelegramUrl = r.IsDBNull(7) ? null : r.GetString(7),
        UpdatedAt = r.GetDateTime(8),
    };
}
