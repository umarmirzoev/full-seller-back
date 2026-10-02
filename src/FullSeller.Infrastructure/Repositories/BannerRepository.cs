using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class BannerRepository : SqlRepositoryBase, IBannerRepository
{
    public BannerRepository(ISqlConnectionFactory factory) : base(factory) { }
    public BannerRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<Banner>> GetActiveAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Title, Subtitle, ImageUrl, LinkUrl, SortOrder, IsActive, CreatedAt
            FROM Banners WHERE IsActive = TRUE ORDER BY SortOrder, CreatedAt
            """);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Banner>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<Banner>)list;
    }, ct);

    public Task<IReadOnlyList<Banner>> GetAllAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Title, Subtitle, ImageUrl, LinkUrl, SortOrder, IsActive, CreatedAt
            FROM Banners ORDER BY SortOrder, CreatedAt
            """);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Banner>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<Banner>)list;
    }, ct);

    public Task<Banner?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Title, Subtitle, ImageUrl, LinkUrl, SortOrder, IsActive, CreatedAt
            FROM Banners WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<Guid> CreateAsync(Banner banner, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        banner.Id = banner.Id == Guid.Empty ? Guid.NewGuid() : banner.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Banners (Id, Title, Subtitle, ImageUrl, LinkUrl, SortOrder, IsActive, CreatedAt)
            VALUES (@Id, @Title, @Subtitle, @ImageUrl, @LinkUrl, @SortOrder, @IsActive, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", banner.Id);
        cmd.Parameters.AddWithValue("@Title", banner.Title);
        cmd.Parameters.AddWithValue("@Subtitle", (object?)banner.Subtitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ImageUrl", (object?)banner.ImageUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LinkUrl", (object?)banner.LinkUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortOrder", banner.SortOrder);
        cmd.Parameters.AddWithValue("@IsActive", banner.IsActive);
        cmd.Parameters.AddWithValue("@CreatedAt", banner.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return banner.Id;
    }, ct);

    public Task UpdateAsync(Banner banner, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE Banners SET Title = @Title, Subtitle = @Subtitle, ImageUrl = @ImageUrl,
                LinkUrl = @LinkUrl, SortOrder = @SortOrder, IsActive = @IsActive
            WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", banner.Id);
        cmd.Parameters.AddWithValue("@Title", banner.Title);
        cmd.Parameters.AddWithValue("@Subtitle", (object?)banner.Subtitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ImageUrl", (object?)banner.ImageUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LinkUrl", (object?)banner.LinkUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortOrder", banner.SortOrder);
        cmd.Parameters.AddWithValue("@IsActive", banner.IsActive);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM Banners WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static Banner Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Title = r.GetString(1),
        Subtitle = r.IsDBNull(2) ? null : r.GetString(2),
        ImageUrl = r.IsDBNull(3) ? null : r.GetString(3),
        LinkUrl = r.IsDBNull(4) ? null : r.GetString(4),
        SortOrder = r.GetInt32(5),
        IsActive = r.GetBoolean(6),
        CreatedAt = r.GetDateTime(7),
    };
}
