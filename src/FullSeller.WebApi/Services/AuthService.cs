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

    private const int OtpLifetimeMinutes = 5;
    private const int MaxOtpAttempts = 5;
    private const int RefreshTokenLifetimeDays = 30;

    public AuthService(
        IOtpRepository otpRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ISmsSender smsSender,
        ITelegramAuthValidator telegramAuthValidator,
        ITokenService tokenService)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _smsSender = smsSender;
        _telegramAuthValidator = telegramAuthValidator;
        _tokenService = tokenService;
    }

    public async Task RequestOtpAsync(string phone, CancellationToken ct)
    {
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
        var otp = await _otpRepository.GetActiveByPhoneAsync(phone, ct)
            ?? throw new InvalidOperationException("Код не найден или истёк. Запросите новый.");

        if (otp.Attempts >= MaxOtpAttempts)
            throw new InvalidOperationException("Превышено число попыток. Запросите новый код.");

        if (otp.CodeHash != _tokenService.HashToken(code))
        {
            await _otpRepository.IncrementAttemptsAsync(otp.Id, ct);
            throw new InvalidOperationException("Неверный код.");
        }

        await _otpRepository.MarkUsedAsync(otp.Id, ct);

        var user = await _userRepository.GetByPhoneAsync(phone, ct);
        if (user is null)
        {
            user = new User { Phone = phone };
            user.Id = await _userRepository.CreateAsync(user, ct);
        }

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
