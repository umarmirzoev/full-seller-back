namespace FullSeller.Domain.Interfaces.Services;

public record TelegramAuthPayload(
    long Id, string? FirstName, string? LastName, string? Username,
    string? PhotoUrl, long AuthDate, string Hash);

public interface ITelegramAuthValidator
{
    /// <summary>Проверяет подпись данных от Telegram Login Widget по секрету бота.</summary>
    bool Validate(TelegramAuthPayload payload);
}
