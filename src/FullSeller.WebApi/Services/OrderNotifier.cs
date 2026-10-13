using System.Net.Http.Json;
using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Services;

/// <summary>
/// Автоматическая отправка уведомления о новом заказе владельцу магазина — без участия клиента.
/// Telegram: Bot API (OrderNotifications:Telegram:BotToken — если пусто, берётся Telegram:AuthBotToken; ChatId — чат владельца).
/// WhatsApp: сервис CallMeBot (OrderNotifications:WhatsApp:Phone + ApiKey, ключ выдаёт сам CallMeBot после активации номера).
/// Если выбранный клиентом канал не настроен или не сработал — пробуем второй. Заказ при этом всегда уже сохранён и виден в админ-панели.
/// </summary>
public class OrderNotifier
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderNotifier> _logger;

    public OrderNotifier(IHttpClientFactory httpFactory, IConfiguration config, ILogger<OrderNotifier> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public void Enqueue(OrderNotifyChannel preferred, string text)
    {
        _ = Task.Run(async () =>
        {
            try { await SendWithFallbackAsync(preferred, text, CancellationToken.None); }
            catch (Exception ex) { _logger.LogError(ex, "Order notification failed unexpectedly"); }
        });
    }

    public async Task<bool> SendWithFallbackAsync(OrderNotifyChannel preferred, string text, CancellationToken ct)
    {
        var order = preferred == OrderNotifyChannel.WhatsApp
            ? new[] { OrderNotifyChannel.WhatsApp, OrderNotifyChannel.Telegram }
            : new[] { OrderNotifyChannel.Telegram, OrderNotifyChannel.WhatsApp };

        foreach (var channel in order)
        {
            if (await TrySendAsync(channel, text, ct))
            {
                _logger.LogInformation("Order notification delivered via {Channel}", channel);
                return true;
            }
        }

        _logger.LogWarning("Order notification was NOT delivered: configure OrderNotifications:Telegram (ChatId) or OrderNotifications:WhatsApp (ApiKey).");
        return false;
    }

    private async Task<bool> TrySendAsync(OrderNotifyChannel channel, string text, CancellationToken ct)
    {
        try
        {
            return channel == OrderNotifyChannel.Telegram
                ? await SendTelegramAsync(text, ct)
                : await SendWhatsAppAsync(text, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Order notification via {Channel} failed: {Message}", channel, ex.Message);
            return false;
        }
    }

    private async Task<bool> SendTelegramAsync(string text, CancellationToken ct)
    {
        var token = _config["OrderNotifications:Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(token)) token = _config["Telegram:AuthBotToken"];
        var chatIds = (_config["OrderNotifications:Telegram:ChatId"] ?? string.Empty)
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (string.IsNullOrWhiteSpace(token) || chatIds.Length == 0) return false;

        var client = _httpFactory.CreateClient("orderNotify");
        var body = text.Length > 4000 ? text[..4000] : text;
        var delivered = false;
        foreach (var chatId in chatIds)
        {
            using var resp = await client.PostAsJsonAsync($"https://api.telegram.org/bot{token}/sendMessage", new { chat_id = chatId, text = body }, ct);
            if (resp.IsSuccessStatusCode) delivered = true;
            else _logger.LogWarning("Telegram sendMessage to {ChatId} returned {Status}", chatId, (int)resp.StatusCode);
        }
        return delivered;
    }

    private async Task<bool> SendWhatsAppAsync(string text, CancellationToken ct)
    {
        var phone = _config["OrderNotifications:WhatsApp:Phone"];
        var apiKey = _config["OrderNotifications:WhatsApp:ApiKey"];
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(apiKey)) return false;
        if (!phone.StartsWith('+')) phone = "+" + phone;

        var body = text.Length > 1800 ? text[..1800] : text;
        var url = $"https://api.callmebot.com/whatsapp.php?phone={Uri.EscapeDataString(phone)}&text={Uri.EscapeDataString(body)}&apikey={Uri.EscapeDataString(apiKey)}";
        var client = _httpFactory.CreateClient("orderNotify");
        using var resp = await client.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) _logger.LogWarning("CallMeBot returned {Status}", (int)resp.StatusCode);
        return resp.IsSuccessStatusCode;
    }
}
