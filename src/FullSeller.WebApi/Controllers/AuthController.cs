using FullSeller.WebApi.Contracts.Auth;
using FullSeller.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequestDto dto, CancellationToken ct)
    {
        await _authService.RequestOtpAsync(dto.Phone, ct);
        return NoContent();
    }

    [HttpPost("otp/confirm")]
    public async Task<ActionResult<TokenPairResponse>> ConfirmOtp([FromBody] OtpConfirmDto dto, CancellationToken ct)
    {
        var tokens = await _authService.ConfirmOtpAsync(dto.Phone, dto.Code, ct);
        return Ok(tokens);
    }

    [HttpPost("telegram")]
    public async Task<ActionResult<TokenPairResponse>> LoginWithTelegram([FromBody] TelegramAuthRequest dto, CancellationToken ct)
    {
        var tokens = await _authService.LoginWithTelegramAsync(dto, ct);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var tokens = await _authService.RefreshAsync(dto.RefreshToken, ct);
        return Ok(tokens);
    }
}
