using System.Security.Cryptography;
using System.Text;
using FullSeller.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace FullSeller.Infrastructure.Services;

/// <summary>Проверка подписи Telegram Login Widget по алгоритму из документации Telegram:
/// secret = SHA256(bot_token); hash = HMAC-SHA256(data_check_string, secret).</summary>
public class TelegramAuthValidator : ITelegramAuthValidator
{
    private readonly string _botToken;

    public TelegramAuthValidator(IConfiguration configuration)
    {
        _botToken = configuration["Telegram:BotToken"] ?? string.Empty;
    }

    public bool Validate(TelegramAuthPayload payload)
    {
        if (string.IsNullOrEmpty(_botToken)) return false;

        var fields = new SortedDictionary<string, string>
        {
            ["id"] = payload.Id.ToString(),
            ["auth_date"] = payload.AuthDate.ToString(),
        };
        if (!string.IsNullOrEmpty(payload.FirstName)) fields["first_name"] = payload.FirstName;
        if (!string.IsNullOrEmpty(payload.LastName)) fields["last_name"] = payload.LastName;
        if (!string.IsNullOrEmpty(payload.Username)) fields["username"] = payload.Username;
        if (!string.IsNullOrEmpty(payload.PhotoUrl)) fields["photo_url"] = payload.PhotoUrl;

        var dataCheckString = string.Join('\n', fields.Select(kv => $"{kv.Key}={kv.Value}"));

        var secretKey = SHA256.HashData(Encoding.UTF8.GetBytes(_botToken));
        var computedHash = Convert.ToHexString(HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString)))
            .ToLowerInvariant();

        var authAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - payload.AuthDate;
        return computedHash == payload.Hash.ToLowerInvariant() && authAge is >= 0 and < 86400;
    }
}
