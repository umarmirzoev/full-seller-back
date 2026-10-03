using System.Text;
using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class UserRepository : SqlRepositoryBase, IUserRepository
{
    public UserRepository(ISqlConnectionFactory factory) : base(factory) { }
    public UserRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Phone, FullName, Role, IsLegalEntity, LegalName, TaxId,
                   LoyaltyPoints, Language, PreferredCurrency, CreatedAt
            FROM Users WHERE Id = @Id
            """);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, Phone, FullName, Role, IsLegalEntity, LegalName, TaxId,
                   LoyaltyPoints, Language, PreferredCurrency, CreatedAt
            FROM Users WHERE Phone = @Phone
            """);
        cmd.Parameters.AddWithValue("@Phone", phone);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }, ct);

    public Task<Guid> CreateAsync(User user, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Users (Id, Phone, FullName, Role, IsLegalEntity, LegalName, TaxId,
                                LoyaltyPoints, Language, PreferredCurrency, CreatedAt)
            VALUES (@Id, @Phone, @FullName, @Role, @IsLegalEntity, @LegalName, @TaxId,
                    @LoyaltyPoints, @Language, @PreferredCurrency, @CreatedAt)
            """);
        AddUserParams(cmd, user);
        await cmd.ExecuteNonQueryAsync(ct);
        return user.Id;
    }, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE Users SET FullName = @FullName, Role = @Role, IsLegalEntity = @IsLegalEntity,
                   LegalName = @LegalName, TaxId = @TaxId, LoyaltyPoints = @LoyaltyPoints,
                   Language = @Language, PreferredCurrency = @PreferredCurrency
            WHERE Id = @Id
            """);
        AddUserParams(cmd, user);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<string?> GetPasswordHashAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT PasswordHash FROM Users WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", userId);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : (string)result;
    }, ct);

    public Task SetPasswordHashAsync(Guid userId, string passwordHash, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Hash", passwordHash);
        cmd.Parameters.AddWithValue("@Id", userId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<PagedResult<User>> GetAllAsync(int page, int pageSize, string? search = null, UserRole? role = null, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var where = new StringBuilder("WHERE 1=1");
        if (!string.IsNullOrWhiteSpace(search)) where.Append(" AND (Phone ILIKE @Search OR FullName ILIKE @Search)");
        if (role is not null) where.Append(" AND Role = @Role");

        using var countCmd = CreateCommand(conn, tx, $"SELECT COUNT(*) FROM Users {where}");
        AddListFilterParams(countCmd, search, role);
        var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

        using var cmd = CreateCommand(conn, tx, $"""
            SELECT Id, Phone, FullName, Role, IsLegalEntity, LegalName, TaxId,
                   LoyaltyPoints, Language, PreferredCurrency, CreatedAt
            FROM Users
            {where}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """);
        AddListFilterParams(cmd, search, role);
        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        var items = new List<User>();
        while (await reader.ReadAsync(ct)) items.Add(Map(reader));
        return new PagedResult<User> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }, ct);

    private static void AddListFilterParams(NpgsqlCommand cmd, string? search, UserRole? role)
    {
        if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@Search", $"%{search}%");
        if (role is not null) cmd.Parameters.AddWithValue("@Role", (int)role.Value);
    }

    public Task<UserStats> GetStatsAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var todayStart = DateTime.UtcNow.Date;
        var weekStart = todayStart.AddDays(-7);

        using var cmd = CreateCommand(conn, tx, """
            SELECT COUNT(*),
                   COUNT(*) FILTER (WHERE CreatedAt >= @TodayStart),
                   COUNT(*) FILTER (WHERE CreatedAt >= @WeekStart)
            FROM Users
            """);
        cmd.Parameters.AddWithValue("@TodayStart", todayStart);
        cmd.Parameters.AddWithValue("@WeekStart", weekStart);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new UserStats((int)reader.GetInt64(0), (int)reader.GetInt64(1), (int)reader.GetInt64(2));
    }, ct);

    public Task SetRoleAsync(Guid userId, UserRole role, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Users SET Role = @Role WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Role", (int)role);
        cmd.Parameters.AddWithValue("@Id", userId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static void AddUserParams(NpgsqlCommand cmd, User user)
    {
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);
        cmd.Parameters.AddWithValue("@FullName", (object?)user.FullName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Role", (int)user.Role);
        cmd.Parameters.AddWithValue("@IsLegalEntity", user.IsLegalEntity);
        cmd.Parameters.AddWithValue("@LegalName", (object?)user.LegalName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TaxId", (object?)user.TaxId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LoyaltyPoints", user.LoyaltyPoints);
        cmd.Parameters.AddWithValue("@Language", user.Language);
        cmd.Parameters.AddWithValue("@PreferredCurrency", user.PreferredCurrency);
        cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
    }

    private static User Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Phone = r.GetString(1),
        FullName = r.IsDBNull(2) ? null : r.GetString(2),
        Role = (UserRole)r.GetInt32(3),
        IsLegalEntity = r.GetBoolean(4),
        LegalName = r.IsDBNull(5) ? null : r.GetString(5),
        TaxId = r.IsDBNull(6) ? null : r.GetString(6),
        LoyaltyPoints = r.GetInt32(7),
        Language = r.GetString(8),
        PreferredCurrency = r.GetString(9),
        CreatedAt = r.GetDateTime(10),
    };
}
