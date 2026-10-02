namespace FullSeller.WebApi.Contracts.Settings;

public record AppSettingsDto(
    decimal UsdToRubRate, DateTime RateUpdatedAt, decimal MinOrderAmount,
    string? ContactPhone, string? ContactAddress, string? WhatsAppUrl, string? TelegramUrl);

/// <summary>Курс отдельно, потому что при его изменении сервер сам проставляет RateUpdatedAt = сейчас — клиент это поле не присылает.</summary>
public record UpdateSettingsRequest(
    decimal UsdToRubRate, decimal MinOrderAmount,
    string? ContactPhone, string? ContactAddress, string? WhatsAppUrl, string? TelegramUrl);
