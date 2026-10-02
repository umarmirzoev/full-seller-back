using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class AddressRepository : SqlRepositoryBase, IAddressRepository
{
    public AddressRepository(ISqlConnectionFactory factory) : base(factory) { }
    public AddressRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "SELECT Id, UserId, Country, City, Line, IsDefault FROM Addresses WHERE UserId = @UserId ORDER BY IsDefault DESC");
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Address>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<Address>)list;
    }, ct);

    public Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "SELECT Id, UserId, Country, City, Line, IsDefault FROM Addresses WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<Guid> CreateAsync(Address address, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        address.Id = address.Id == Guid.Empty ? Guid.NewGuid() : address.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Addresses (Id, UserId, Country, City, Line, IsDefault)
            VALUES (@Id, @UserId, @Country, @City, @Line, @IsDefault)
            """);
        cmd.Parameters.AddWithValue("@Id", address.Id);
        cmd.Parameters.AddWithValue("@UserId", address.UserId);
        cmd.Parameters.AddWithValue("@Country", (int)address.Country);
        cmd.Parameters.AddWithValue("@City", address.City);
        cmd.Parameters.AddWithValue("@Line", address.Line);
        cmd.Parameters.AddWithValue("@IsDefault", address.IsDefault);
        await cmd.ExecuteNonQueryAsync(ct);
        return address.Id;
    }, ct);

    public Task SetDefaultAsync(Guid userId, Guid addressId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var reset = CreateCommand(conn, tx, "UPDATE Addresses SET IsDefault = FALSE WHERE UserId = @UserId");
        reset.Parameters.AddWithValue("@UserId", userId);
        await reset.ExecuteNonQueryAsync(ct);

        using var setDefault = CreateCommand(conn, tx, "UPDATE Addresses SET IsDefault = TRUE WHERE Id = @Id AND UserId = @UserId");
        setDefault.Parameters.AddWithValue("@Id", addressId);
        setDefault.Parameters.AddWithValue("@UserId", userId);
        await setDefault.ExecuteNonQueryAsync(ct);
    }, ct);

    private static Address Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        UserId = r.GetGuid(1),
        Country = (DeliveryCountry)r.GetInt32(2),
        City = r.GetString(3),
        Line = r.GetString(4),
        IsDefault = r.GetBoolean(5),
    };
}
