using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ChatSenderType SenderType { get; set; }
    public string Text { get; set; } = default!;
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
