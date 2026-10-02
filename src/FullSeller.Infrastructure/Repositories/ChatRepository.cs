using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class ChatRepository : SqlRepositoryBase, IChatRepository
{
    public ChatRepository(ISqlConnectionFactory factory) : base(factory) { }
    public ChatRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            SELECT Id, UserId, SenderType, Text, AttachmentUrl, SentAt, IsRead
            FROM ChatMessages WHERE UserId = @UserId ORDER BY SentAt
            """);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<ChatMessage>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new ChatMessage
            {
                Id = reader.GetGuid(0),
                UserId = reader.GetGuid(1),
                SenderType = (ChatSenderType)reader.GetInt32(2),
                Text = reader.GetString(3),
                AttachmentUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                SentAt = reader.GetDateTime(5),
                IsRead = reader.GetBoolean(6),
            });
        }
        return (IReadOnlyList<ChatMessage>)list;
    }, ct);

    public Task<Guid> AddMessageAsync(ChatMessage message, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        message.Id = message.Id == Guid.Empty ? Guid.NewGuid() : message.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO ChatMessages (Id, UserId, SenderType, Text, AttachmentUrl, SentAt, IsRead)
            VALUES (@Id, @UserId, @SenderType, @Text, @AttachmentUrl, @SentAt, @IsRead)
            """);
        cmd.Parameters.AddWithValue("@Id", message.Id);
        cmd.Parameters.AddWithValue("@UserId", message.UserId);
        cmd.Parameters.AddWithValue("@SenderType", (int)message.SenderType);
        cmd.Parameters.AddWithValue("@Text", message.Text);
        cmd.Parameters.AddWithValue("@AttachmentUrl", (object?)message.AttachmentUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SentAt", message.SentAt);
        cmd.Parameters.AddWithValue("@IsRead", message.IsRead);
        await cmd.ExecuteNonQueryAsync(ct);
        return message.Id;
    }, ct);

    public Task MarkReadAsync(Guid userId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE ChatMessages SET IsRead = TRUE WHERE UserId = @UserId AND IsRead = FALSE");
        cmd.Parameters.AddWithValue("@UserId", userId);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);
}
