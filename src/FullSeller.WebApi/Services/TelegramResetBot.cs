using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace FullSeller.WebApi.Services;

/// <summary>
/// Временное хранилище заявок на сброс пароля через Telegram: nonce → номер телефона (живёт 15 минут).
/// </summary>
public class PasswordResetTelegramStore
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    public PasswordResetTelegramStore(IMemoryCache cache) => _cache = cache;

    /// <summary>Username бота, полученный через getMe (если не задан в конфиге).</summary>
    public string? DetectedUsername { get; set; }

    public string CreateNonce(string phone)
    {
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        _cache.Set("tgreset:" + nonce, phone, Lifetime);
        return nonce;
    }

    public string? GetPhone(string nonce) => _cache.TryGetValue("tgreset:" + nonce, out string? phone) ? phone : null;

    public void Remove(string nonce) => _cache.Remove("tgreset:" + nonce);

    /// <summary>Какой nonce ждёт контакт от этого Telegram-чата.</summary>
    public void SetPendingChat(long chatId, string nonce) => _cache.Set("tgchat:" + chatId, nonce, Lifetime);

    public string? GetPendingChat(long chatId) => _cache.TryGetValue("tgchat:" + chatId, out string? nonce) ? nonce : null;

    public void RemovePendingChat(long chatId) => _cache.Remove("tgchat:" + chatId);
}

/// <summary>
/// Бесплатная альтернатива SMS: Telegram-бот (long polling) выдаёт код для сброса пароля.
/// Сценарий: приложение открывает t.me/&lt;bot&gt;?start=reset_&lt;nonce&gt; → бот просит поделиться номером →
/// если номер Telegram совпадает с номером аккаунта, бот создаёт одноразовый код (та же таблица OTP,
/// что и для SMS) и присылает его. Дальше приложение вызывает POST /api/auth/password/reset.
/// Токен бота: Telegram:AuthBotToken (appsettings или переменная окружения Telegram__AuthBotToken).
/// </summary>
public class TelegramResetBotService : BackgroundService
{
    private const int OtpLifetimeMinutes = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpFactory;
    private readonly PasswordResetTelegramStore _store;
    private readonly IConfiguration _config;
    private readonly ILogger<TelegramResetBotService> _logger;

    public TelegramResetBotService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpFactory,
        PasswordResetTelegramStore store,
        IConfiguration config,
        ILogger<TelegramResetBotService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpFactory = httpFactory;
        _store = store;
        _config = config;
        _logger = logger;
    }

    private string? Token => _config["Telegram:AuthBotToken"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            _logger.LogInformation("Telegram reset bot disabled: Telegram:AuthBotToken is empty.");
            return;
        }

        var http = _httpFactory.CreateClient("telegram");
        http.Timeout = TimeSpan.FromSeconds(60);
        var api = $"https://api.telegram.org/bot{Token}/";

        try
        {
            // Long polling не работает, если у бота включён webhook.
            await http.GetAsync(api + "deleteWebhook", stoppingToken);
            var me = await http.GetFromJsonAsync<JsonObject>(api + "getMe", stoppingToken);
            _store.DetectedUsername = me?["result"]?["username"]?.GetValue<string>();
            _logger.LogInformation("Telegram reset bot started as @{Bot}", _store.DetectedUsername);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Telegram reset bot: getMe failed");
        }

        long offset = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resp = await http.GetFromJsonAsync<JsonObject>(
                    $"{api}getUpdates?timeout=25&offset={offset}&allowed_updates=%5B%22message%22%5D", stoppingToken);
                if (resp?["result"] is not JsonArray updates) continue;

                foreach (var upd in updates)
                {
                    if (upd is null) continue;
                    offset = Math.Max(offset, upd["update_id"]!.GetValue<long>() + 1);
                    if (upd["message"] is JsonObject msg)
                    {
                        try { await HandleMessageAsync(http, api, msg, stoppingToken); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Telegram reset bot: message handling failed"); }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Telegram reset bot: polling error, retry in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(HttpClient http, string api, JsonObject msg, CancellationToken ct)
    {
        var chatId = msg["chat"]?["id"]?.GetValue<long>() ?? 0;
        if (chatId == 0) return;
        var text = msg["text"]?.GetValue<string>()?.Trim() ?? string.Empty;

        // 1) /start reset_<nonce> — пришли из приложения
        if (text.StartsWith("/start", StringComparison.Ordinal))
        {
            var arg = text.Length > 6 ? text[6..].Trim() : string.Empty;
            if (arg.StartsWith("reset_", StringComparison.Ordinal))
            {
                var nonce = arg["reset_".Length..];
                if (_store.GetPhone(nonce) is null)
                {
                    await SendAsync(http, api, chatId, "⏳ Ссылка устарела. Вернитесь в приложение FULL SELLER и нажмите «Получить код в Telegram» ещё раз.", ct);
                    return;
                }
                _store.SetPendingChat(chatId, nonce);
                await SendAsync(http, api, chatId,
                    "🔐 <b>Восстановление пароля FULL SELLER</b>\n\nНажмите кнопку ниже, чтобы подтвердить номер телефона. Номер в Telegram должен совпадать с номером аккаунта.",
                    ct, contactButton: true);
                return;
            }
            await SendAsync(http, api, chatId,
                "👋 Это бот FULL SELLER для восстановления пароля.\n\nОткройте приложение → «Забыли пароль?» → «Получить код в Telegram».", ct);
            return;
        }

        // 2) Пользователь поделился контактом
        if (msg["contact"] is JsonObject contact)
        {
            var nonce = _store.GetPendingChat(chatId);
            if (nonce is null)
            {
                await SendAsync(http, api, chatId, "Сначала откройте ссылку из приложения FULL SELLER («Забыли пароль?»).", ct, removeKeyboard: true);
                return;
            }

            var fromId = msg["from"]?["id"]?.GetValue<long>() ?? -1;
            var contactUserId = contact["user_id"]?.GetValue<long>() ?? -2;
            if (fromId != contactUserId)
            {
                await SendAsync(http, api, chatId, "Пожалуйста, отправьте <b>свой</b> номер кнопкой «📱 Подтвердить номер».", ct, contactButton: true);
                return;
            }

            var expectedPhone = _store.GetPhone(nonce);
            if (expectedPhone is null)
            {
                _store.RemovePendingChat(chatId);
                await SendAsync(http, api, chatId, "⏳ Время истекло. Запросите код в приложении ещё раз.", ct, removeKeyboard: true);
                return;
            }

            var tgDigits = new string((contact["phone_number"]?.GetValue<string>() ?? string.Empty).Where(char.IsDigit).ToArray());
            var expectedDigits = new string(expectedPhone.Where(char.IsDigit).ToArray());
            if (tgDigits.Length == 0 || tgDigits != expectedDigits)
            {
                await SendAsync(http, api, chatId,
                    "❌ Номер вашего Telegram не совпадает с номером аккаунта. Код можно получить только на Telegram с тем же номером.",
                    ct, removeKeyboard: true);
                return;
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            using (var scope = _scopeFactory.CreateScope())
            {
                var otpRepo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
                await otpRepo.CreateAsync(new OtpCode
                {
                    Phone = expectedPhone,
                    CodeHash = tokens.HashToken(code),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes),
                }, ct);
            }

            _store.Remove(nonce);
            _store.RemovePendingChat(chatId);
            await SendAsync(http, api, chatId,
                $"✅ Ваш код для сброса пароля:\n\n<code>{code}</code>\n\nВведите его в приложении FULL SELLER. Код действует {OtpLifetimeMinutes} минут. Никому его не сообщайте.",
                ct, removeKeyboard: true);
            return;
        }

        await SendAsync(http, api, chatId, "Чтобы восстановить пароль, откройте приложение FULL SELLER → «Забыли пароль?».", ct);
    }

    private static async Task SendAsync(HttpClient http, string api, long chatId, string text, CancellationToken ct,
        bool contactButton = false, bool removeKeyboard = false)
    {
        var body = new JsonObject
        {
            ["chat_id"] = chatId,
            ["text"] = text,
            ["parse_mode"] = "HTML",
        };
        if (contactButton)
        {
            body["reply_markup"] = new JsonObject
            {
                ["keyboard"] = new JsonArray(new JsonArray(new JsonObject
                {
                    ["text"] = "📱 Подтвердить номер",
                    ["request_contact"] = true,
                })),
                ["resize_keyboard"] = true,
                ["one_time_keyboard"] = true,
            };
        }
        else if (removeKeyboard)
        {
            body["reply_markup"] = new JsonObject { ["remove_keyboard"] = true };
        }

        using var content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
        await http.PostAsync(api + "sendMessage", content, ct);
    }
}
