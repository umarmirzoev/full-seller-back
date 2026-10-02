using FullSeller.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FullSeller.Infrastructure.Services;

/// <summary>Заготовка IPushNotifier: логирует вместо реальной отправки.
/// В проде — подключить Firebase Admin SDK и хранить device-токены пользователя (отдельная таблица).</summary>
public class FirebasePushNotifier : IPushNotifier
{
    private readonly ILogger<FirebasePushNotifier> _logger;

    public FirebasePushNotifier(ILogger<FirebasePushNotifier> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Guid userId, string title, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("[DEV PUSH] -> {UserId}: {Title} — {Body}", userId, title, body);
        return Task.CompletedTask;
    }
}
