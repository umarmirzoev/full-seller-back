using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Notifications;

public record NotificationDto(Guid Id, NotificationType Type, string Title, string Body, bool IsRead, DateTime CreatedAt);
