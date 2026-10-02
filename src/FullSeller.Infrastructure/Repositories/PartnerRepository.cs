using System.Text.Json;
using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class PartnerRepository : SqlRepositoryBase, IPartnerRepository
{
    public PartnerRepository(ISqlConnectionFactory factory) : base(factory) { }
    public PartnerRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    // Тот же порядок колонок, что и ProductColumns в CatalogRepository, плюс PartnerId —
    // отдельная константа, т.к. это самостоятельный (не шарящийся с CatalogRepository) набор запросов.
    private const string ProductColumns = """
        Id, Name, CategoryId, BrandId, Description, Composition,
        BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive,
        MinPrice, OldPrice, IsNew, IsHit, PartnerId, AudienceTag, CreatedAt
        """;

    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, CompanyName, ContactName, Phone, Login, PasswordHash, IsActive, CreatedAt
            FROM Partners WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapPartner(reader) : null;
    }, ct);

    public Task<Partner?> GetByLoginAsync(string login, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, CompanyName, ContactName, Phone, Login, PasswordHash, IsActive, CreatedAt
            FROM Partners WHERE Login = @Login
            """);
        cmd.Parameters.AddWithValue("@Login", login);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapPartner(reader) : null;
    }, ct);

    public Task<IReadOnlyList<Partner>> GetAllAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, CompanyName, ContactName, Phone, Login, PasswordHash, IsActive, CreatedAt
            FROM Partners ORDER BY CreatedAt DESC
            """);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Partner>();
        while (await reader.ReadAsync(ct)) list.Add(MapPartner(reader));
        return (IReadOnlyList<Partner>)list;
    }, ct);

    public Task<Guid> CreateAsync(Partner partner, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        partner.Id = partner.Id == Guid.Empty ? Guid.NewGuid() : partner.Id;
        if (partner.CreatedAt == default) partner.CreatedAt = DateTime.UtcNow;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Partners (Id, CompanyName, ContactName, Phone, Login, PasswordHash, IsActive, CreatedAt)
            VALUES (@Id, @CompanyName, @ContactName, @Phone, @Login, @PasswordHash, @IsActive, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", partner.Id);
        cmd.Parameters.AddWithValue("@CompanyName", partner.CompanyName);
        cmd.Parameters.AddWithValue("@ContactName", partner.ContactName);
        cmd.Parameters.AddWithValue("@Phone", partner.Phone);
        cmd.Parameters.AddWithValue("@Login", partner.Login);
        cmd.Parameters.AddWithValue("@PasswordHash", partner.PasswordHash);
        cmd.Parameters.AddWithValue("@IsActive", partner.IsActive);
        cmd.Parameters.AddWithValue("@CreatedAt", partner.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return partner.Id;
    }, ct);

    public Task<PagedResult<Product>> GetProductsByPartnerAsync(Guid partnerId, int page, int pageSize, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using (var countCmd = CreateCommand(conn, tx, "SELECT COUNT(*) FROM Products WHERE PartnerId = @PartnerId"))
        {
            countCmd.Parameters.AddWithValue("@PartnerId", partnerId);
            var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

            using var pageCmd = CreateCommand(conn, tx, $"""
                SELECT {ProductColumns} FROM Products WHERE PartnerId = @PartnerId
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """);
            pageCmd.Parameters.AddWithValue("@PartnerId", partnerId);
            pageCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            pageCmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await pageCmd.ExecuteReaderAsync(ct);
            var items = new List<Product>();
            while (await reader.ReadAsync(ct)) items.Add(MapProduct(reader));
            return new PagedResult<Product> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }
    }, ct);

    public Task<Guid> CreateProductForPartnerAsync(Guid partnerId, Product product, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        product.Id = product.Id == Guid.Empty ? Guid.NewGuid() : product.Id;
        if (product.CreatedAt == default) product.CreatedAt = DateTime.UtcNow;
        product.PartnerId = partnerId;

        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams,
                                   ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, OldPrice, IsNew, IsHit, PartnerId, AudienceTag, CreatedAt)
            VALUES (@Id, @Name, @CategoryId, @BrandId, @Description, @Composition, @BaseSku, @WeightGrams,
                    @ImageUrlsJson, 0, 0, @IsActive, @MinPrice, @OldPrice, @IsNew, @IsHit, @PartnerId, @AudienceTag, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", product.Id);
        cmd.Parameters.AddWithValue("@Name", product.Name);
        cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        cmd.Parameters.AddWithValue("@BrandId", (object?)product.BrandId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Composition", (object?)product.Composition ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BaseSku", product.BaseSku);
        cmd.Parameters.AddWithValue("@WeightGrams", product.WeightGrams);
        cmd.Parameters.AddWithValue("@ImageUrlsJson", JsonSerializer.Serialize(product.ImageUrls));
        cmd.Parameters.AddWithValue("@IsActive", product.IsActive);
        cmd.Parameters.AddWithValue("@MinPrice", product.MinPrice);
        cmd.Parameters.AddWithValue("@OldPrice", (object?)product.OldPrice ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsNew", product.IsNew);
        cmd.Parameters.AddWithValue("@IsHit", product.IsHit);
        cmd.Parameters.AddWithValue("@PartnerId", partnerId);
        cmd.Parameters.AddWithValue("@AudienceTag", (object?)product.AudienceTag ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return product.Id;
    }, ct);

    public Task<Guid> CreateRefreshTokenAsync(PartnerRefreshToken token, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        token.Id = token.Id == Guid.Empty ? Guid.NewGuid() : token.Id;
        if (token.CreatedAt == default) token.CreatedAt = DateTime.UtcNow;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO PartnerRefreshTokens (Id, PartnerId, TokenHash, ExpiresAt, RevokedAt, CreatedAt)
            VALUES (@Id, @PartnerId, @TokenHash, @ExpiresAt, @RevokedAt, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", token.Id);
        cmd.Parameters.AddWithValue("@PartnerId", token.PartnerId);
        cmd.Parameters.AddWithValue("@TokenHash", token.TokenHash);
        cmd.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);
        cmd.Parameters.AddWithValue("@RevokedAt", (object?)token.RevokedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return token.Id;
    }, ct);

    public Task<PartnerRefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, PartnerId, TokenHash, ExpiresAt, RevokedAt, CreatedAt
            FROM PartnerRefreshTokens WHERE TokenHash = @TokenHash
            """);
        cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapRefreshToken(reader) : null;
    }, ct);

    public Task RevokeRefreshTokenAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE PartnerRefreshTokens SET RevokedAt = now() WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static Partner MapPartner(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        CompanyName = r.GetString(1),
        ContactName = r.GetString(2),
        Phone = r.GetString(3),
        Login = r.GetString(4),
        PasswordHash = r.GetString(5),
        IsActive = r.GetBoolean(6),
        CreatedAt = r.GetDateTime(7),
    };

    private static Product MapProduct(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Name = r.GetString(1),
        CategoryId = r.GetGuid(2),
        BrandId = r.IsDBNull(3) ? null : r.GetGuid(3),
        Description = r.IsDBNull(4) ? null : r.GetString(4),
        Composition = r.IsDBNull(5) ? null : r.GetString(5),
        BaseSku = r.GetString(6),
        WeightGrams = r.GetInt32(7),
        ImageUrls = r.IsDBNull(8) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.GetString(8)) ?? new(),
        Rating = r.GetDecimal(9),
        ReviewsCount = r.GetInt32(10),
        IsActive = r.GetBoolean(11),
        MinPrice = r.GetDecimal(12),
        OldPrice = r.IsDBNull(13) ? null : r.GetDecimal(13),
        IsNew = r.GetBoolean(14),
        IsHit = r.GetBoolean(15),
        PartnerId = r.IsDBNull(16) ? null : r.GetGuid(16),
        AudienceTag = r.IsDBNull(17) ? null : r.GetString(17),
        CreatedAt = r.GetDateTime(18),
    };

    private static PartnerRefreshToken MapRefreshToken(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        PartnerId = r.GetGuid(1),
        TokenHash = r.GetString(2),
        ExpiresAt = r.GetDateTime(3),
        RevokedAt = r.IsDBNull(4) ? null : r.GetDateTime(4),
        CreatedAt = r.GetDateTime(5),
    };
}
