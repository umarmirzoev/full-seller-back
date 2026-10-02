using System.Text.Json;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class ReviewRepository : SqlRepositoryBase, IReviewRepository
{
    public ReviewRepository(ISqlConnectionFactory factory) : base(factory) { }
    public ReviewRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, ProductId, UserId, OrderId, Rating, Text, PhotoUrlsJson, Status, CreatedAt
            FROM Reviews WHERE ProductId = @ProductId AND Status = @Approved
            ORDER BY CreatedAt DESC
            """);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        cmd.Parameters.AddWithValue("@Approved", (int)ReviewStatus.Approved);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Review>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<Review>)list;
    }, ct);

    public Task<IReadOnlyList<Review>> GetByStatusAsync(ReviewStatus status, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, ProductId, UserId, OrderId, Rating, Text, PhotoUrlsJson, Status, CreatedAt
            FROM Reviews WHERE Status = @Status ORDER BY CreatedAt
            """);
        cmd.Parameters.AddWithValue("@Status", (int)status);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Review>();
        while (await reader.ReadAsync(ct)) list.Add(Map(reader));
        return (IReadOnlyList<Review>)list;
    }, ct);

    public Task<Guid> CreateAsync(Review review, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        review.Id = review.Id == Guid.Empty ? Guid.NewGuid() : review.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Reviews (Id, ProductId, UserId, OrderId, Rating, Text, PhotoUrlsJson, Status, CreatedAt)
            VALUES (@Id, @ProductId, @UserId, @OrderId, @Rating, @Text, @PhotoUrlsJson, @Status, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", review.Id);
        cmd.Parameters.AddWithValue("@ProductId", review.ProductId);
        cmd.Parameters.AddWithValue("@UserId", review.UserId);
        cmd.Parameters.AddWithValue("@OrderId", review.OrderId);
        cmd.Parameters.AddWithValue("@Rating", review.Rating);
        cmd.Parameters.AddWithValue("@Text", (object?)review.Text ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PhotoUrlsJson", JsonSerializer.Serialize(review.PhotoUrls));
        cmd.Parameters.AddWithValue("@Status", (int)review.Status);
        cmd.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return review.Id;
    }, ct);

    public Task SetStatusAsync(Guid reviewId, ReviewStatus status, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Reviews SET Status = @Status WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Status", (int)status);
        cmd.Parameters.AddWithValue("@Id", reviewId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static Review Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        ProductId = r.GetGuid(1),
        UserId = r.GetGuid(2),
        OrderId = r.GetGuid(3),
        Rating = r.GetInt32(4),
        Text = r.IsDBNull(5) ? null : r.GetString(5),
        PhotoUrls = r.IsDBNull(6) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.GetString(6)) ?? new(),
        Status = (ReviewStatus)r.GetInt32(7),
        CreatedAt = r.GetDateTime(8),
    };
}
