using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class NotificationRepository : SqlRepositoryBase, INotificationRepository
{
    public NotificationRepository(ISqlConnectionFactory factory) : base(factory) { }
    public NotificationRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, UserId, Type, Title, Body, IsRead, CreatedAt
            FROM Notifications WHERE UserId = @UserId ORDER BY CreatedAt DESC
            """);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Notification>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new Notification
            {
                Id = reader.GetGuid(0),
                UserId = reader.GetGuid(1),
                Type = (NotificationType)reader.GetInt32(2),
                Title = reader.GetString(3),
                Body = reader.GetString(4),
                IsRead = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
            });
        }
        return (IReadOnlyList<Notification>)list;
    }, ct);

    public Task<Guid> CreateAsync(Notification notification, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Notifications (Id, UserId, Type, Title, Body, IsRead, CreatedAt)
            VALUES (@Id, @UserId, @Type, @Title, @Body, @IsRead, @CreatedAt)
            """);
        cmd.Parameters.AddWithValue("@Id", notification.Id);
        cmd.Parameters.AddWithValue("@UserId", notification.UserId);
        cmd.Parameters.AddWithValue("@Type", (int)notification.Type);
        cmd.Parameters.AddWithValue("@Title", notification.Title);
        cmd.Parameters.AddWithValue("@Body", notification.Body);
        cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);
        cmd.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return notification.Id;
    }, ct);

    public Task MarkReadAsync(Guid notificationId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Notifications SET IsRead = TRUE WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", notificationId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);
}
