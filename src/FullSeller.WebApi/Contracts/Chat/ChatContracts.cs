using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Chat;

public record ChatMessageDto(Guid Id, ChatSenderType SenderType, string Text, string? AttachmentUrl, DateTime SentAt, bool IsRead);
public record SendMessageRequest(string Text, string? AttachmentUrl);
