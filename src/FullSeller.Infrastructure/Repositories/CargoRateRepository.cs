using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class CargoRateRepository : SqlRepositoryBase, ICargoRateRepository
{
    public CargoRateRepository(ISqlConnectionFactory factory) : base(factory) { }
    public CargoRateRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<CargoRate?> GetByCountryAsync(DeliveryCountry country, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT Id, Country, PricePerKg, Currency FROM CargoRates WHERE Country = @Country");
        cmd.Parameters.AddWithValue("@Country", (int)country);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return Map(reader);
    }, ct);

    public Task<IReadOnlyList<CargoRate>> GetAllAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT Id, Country, PricePerKg, Currency FROM CargoRates ORDER BY Country");
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<CargoRate>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<CargoRate>)list;
    }, ct);

    /// <summary>UPSERT по уникальному Country (см. UQ_CargoRates_Country в 0001_init.sql).</summary>
    public Task UpsertAsync(CargoRate rate, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        rate.Id = rate.Id == Guid.Empty ? Guid.NewGuid() : rate.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO CargoRates (Id, Country, PricePerKg, Currency)
            VALUES (@Id, @Country, @PricePerKg, @Currency)
            ON CONFLICT (Country) DO UPDATE SET PricePerKg = EXCLUDED.PricePerKg, Currency = EXCLUDED.Currency
            """);
        cmd.Parameters.AddWithValue("@Id", rate.Id);
        cmd.Parameters.AddWithValue("@Country", (int)rate.Country);
        cmd.Parameters.AddWithValue("@PricePerKg", rate.PricePerKg);
        cmd.Parameters.AddWithValue("@Currency", rate.Currency);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static CargoRate Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Country = (DeliveryCountry)r.GetInt32(1),
        PricePerKg = r.GetDecimal(2),
        Currency = r.GetString(3),
    };
}
