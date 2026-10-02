using FullSeller.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FullSeller.Infrastructure.Services;

/// <summary>Заготовка ISmsSender для разработки: пишет код в лог вместо реальной отправки.
/// В проде — заменить на реализацию под конкретного SMS-провайдера (РФ/ТДЖ/УЗ), не меняя интерфейс.</summary>
public class ConsoleSmsSender : ISmsSender
{
    private readonly ILogger<ConsoleSmsSender> _logger;

    public ConsoleSmsSender(ILogger<ConsoleSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(string phone, string code, CancellationToken ct = default)
    {
        _logger.LogInformation("[DEV SMS] Код {Code} для номера {Phone}", code, phone);
        return Task.CompletedTask;
    }
}
