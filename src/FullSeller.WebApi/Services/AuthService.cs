using System.Security.Cryptography;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;
using FullSeller.WebApi.Contracts.Auth;

namespace FullSeller.WebApi.Services;

public class AuthService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISmsSender _smsSender;
    private readonly ITelegramAuthValidator _telegramAuthValidator;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    private const int OtpLifetimeMinutes = 5;
    private const int MaxOtpAttempts = 5;
    private const int RefreshTokenLifetimeDays = 30;
    private const int MinPasswordLength = 6;

    /// <summary>Единый формат номера: "+" и только цифры ("+992 97 911-70-07" → "+992979117007").</summary>
    public static string NormalizePhone(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length < 9 || digits.Length > 15)
            throw new InvalidOperationException("Введите корректный номер телефона.");
        return "+" + digits;
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new InvalidOperationException($"Пароль должен быть не короче {MinPasswordLength} символов.");
        if (password.Length > 100)
            throw new InvalidOperationException("Пароль слишком длинный.");
    }

    /// <summary>Тестовый аккаунт для проверки в Google Play / App Store: номер и постоянный код
    /// задаются в appsettings (Auth:ReviewPhone, Auth:ReviewCode); SMS на него не отправляется.</summary>
    private bool IsReviewLogin(string phone, string? code = null)
    {
        var reviewPhone = _configuration["Auth:ReviewPhone"];
        var reviewCode = _configuration["Auth:ReviewCode"];
        if (string.IsNullOrWhiteSpace(reviewPhone) || string.IsNullOrWhiteSpace(reviewCode)) return false;
        string normalized;
        try { normalized = NormalizePhone(reviewPhone); } catch { return false; }
        return normalized == phone && (code is null || code.Trim() == reviewCode.Trim());
    }

    /// <summary>Проверяет одноразовый код и помечает его использованным.</summary>
    private async Task VerifyOtpAsync(string phone, string code, CancellationToken ct)
    {
        if (IsReviewLogin(phone, code)) return;

        var otp = await _otpRepository.GetActiveByPhoneAsync(phone, ct)
            ?? throw new InvalidOperationException("Код не найден или истёк. Запросите новый.");

        if (otp.Attempts >= MaxOtpAttempts)
            throw new InvalidOperationException("Превышено число попыток. Запросите новый код.");

        if (otp.CodeHash != _tokenService.HashToken((code ?? string.Empty).Trim()))
        {
            await _otpRepository.IncrementAttemptsAsync(otp.Id, ct);
            throw new InvalidOperationException("Неверный код.");
        }

        await _otpRepository.MarkUsedAsync(otp.Id, ct);
    }

    public AuthService(
        IOtpRepository otpRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ISmsSender smsSender,
        ITelegramAuthValidator telegramAuthValidator,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _smsSender = smsSender;
        _telegramAuthValidator = telegramAuthValidator;
        _tokenService = tokenService;
    }

    public async Task RequestOtpAsync(string phone, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        if (IsReviewLogin(phone)) return;
        var code = RandomNumberGenerator.GetInt32(1000, 10000).ToString();
        var otp = new OtpCode
        {
            Phone = phone,
            CodeHash = _tokenService.HashToken(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes),
        };
        await _otpRepository.CreateAsync(otp, ct);
        await _smsSender.SendOtpAsync(phone, code, ct);
    }

    public async Task<TokenPairResponse> ConfirmOtpAsync(string phone, string code, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        await VerifyOtpAsync(phone, code, ct);

        var user = await _userRepository.GetByPhoneAsync(phone, ct);
        if (user is null)
        {
            user = new User { Phone = phone };
            user.Id = await _userRepository.CreateAsync(user, ct);
        }

        return await IssueAndPersistTokensAsync(user, ct);
    }

    /// <summary>Вход по номеру и паролю.</summary>
    public async Task<TokenPairResponse> LoginWithPasswordAsync(string phone, string password, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        var user = await _userRepository.GetByPhoneAsync(phone, ct);
        var hash = user is null ? null : await _userRepository.GetPasswordHashAsync(user.Id, ct);
        if (user is null || !PasswordHasher.Verify(password ?? string.Empty, hash))
            throw new UnauthorizedAccessException("Неверный номер или пароль.");

        return await IssueAndPersistTokensAsync(user, ct);
    }

    /// <summary>Регистрация по номеру телефона и паролю (без SMS-кода).</summary>
    public async Task<TokenPairResponse> RegisterAsync(string phone, string password, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        ValidatePassword(password);

        var user = await _userRepository.GetByPhoneAsync(phone, ct);
        if (user is not null)
            throw new InvalidOperationException("Аккаунт с этим номером уже есть. Войдите по паролю.");

        user = new User { Phone = phone };
        user.Id = await _userRepository.CreateAsync(user, ct);

        await _userRepository.SetPasswordHashAsync(user.Id, PasswordHasher.Hash(password), ct);
        return await IssueAndPersistTokensAsync(user, ct);
    }

    /// <summary>Проверяет, что аккаунт с таким номером существует; возвращает нормализованный номер.</summary>
    public async Task<string> EnsureAccountExistsAsync(string phone, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        _ = await _userRepository.GetByPhoneAsync(phone, ct)
            ?? throw new InvalidOperationException("Аккаунт с этим номером не найден. Зарегистрируйтесь.");
        return phone;
    }

    /// <summary>Сброс пароля по коду из SMS.</summary>
    public async Task<TokenPairResponse> ResetPasswordAsync(string phone, string code, string password, CancellationToken ct)
    {
        phone = NormalizePhone(phone);
        ValidatePassword(password);

        var user = await _userRepository.GetByPhoneAsync(phone, ct)
            ?? throw new InvalidOperationException("Аккаунт с этим номером не найден. Зарегистрируйтесь.");

        await VerifyOtpAsync(phone, code, ct);
        await _userRepository.SetPasswordHashAsync(user.Id, PasswordHasher.Hash(password), ct);
        return await IssueAndPersistTokensAsync(user, ct);
    }

    public async Task<TokenPairResponse> LoginWithTelegramAsync(TelegramAuthRequest request, CancellationToken ct)
    {
        var payload = new TelegramAuthPayload(request.Id, request.FirstName, request.LastName, request.Username, request.PhotoUrl, request.AuthDate, request.Hash);
        if (!_telegramAuthValidator.Validate(payload))
            throw new InvalidOperationException("Подпись Telegram-входа недействительна.");

        var pseudoPhone = $"tg:{request.Id}";
        var user = await _userRepository.GetByPhoneAsync(pseudoPhone, ct);
        if (user is null)
        {
            user = new User { Phone = pseudoPhone, FullName = $"{request.FirstName} {request.LastName}".Trim() };
            user.Id = await _userRepository.CreateAsync(user, ct);
        }

        return await IssueAndPersistTokensAsync(user, ct);
    }

    public async Task<TokenPairResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var hash = _tokenService.HashToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct)
            ?? throw new InvalidOperationException("Refresh-токен недействителен.");

        if (stored.RevokedAt is not null || stored.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Refresh-токен истёк или отозван.");

        var user = await _userRepository.GetByIdAsync(stored.UserId, ct)
            ?? throw new InvalidOperationException("Пользователь не найден.");

        await _refreshTokenRepository.RevokeAsync(stored.Id, ct);
        return await IssueAndPersistTokensAsync(user, ct);
    }

    private async Task<TokenPairResponse> IssueAndPersistTokensAsync(User user, CancellationToken ct)
    {
        var pair = _tokenService.IssueTokens(user);
        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(pair.RefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays),
        }, ct);

        return new TokenPairResponse(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt);
    }
}
