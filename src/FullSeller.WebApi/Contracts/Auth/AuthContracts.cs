namespace FullSeller.WebApi.Contracts.Auth;

public record OtpRequestDto(string Phone);
public record OtpConfirmDto(string Phone, string Code);
public record TelegramAuthRequest(long Id, string? FirstName, string? LastName, string? Username, string? PhotoUrl, long AuthDate, string Hash);
public record RefreshRequest(string RefreshToken);
public record PasswordLoginRequest(string Phone, string Password);
public record RegisterRequest(string Phone, string Password);
public record ResetPasswordRequest(string Phone, string Code, string Password);

public record TokenPairResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
