using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Настройки магазина: курс валюты, минимальная сумма заказа, контакты и соцсети
/// (ТЗ п.8 «Валюта ₽ / $», п.10 «Админ-панель → Настройки»).
/// Чтение — публично (нужно клиенту для пересчёта ₽/$ и футера), изменение — только Admin/SuperAdmin.
/// </summary>
[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly IAppSettingsRepository _settings;

    public SettingsController(IAppSettingsRepository settings)
    {
        _settings = settings;
    }

    [HttpGet]
    public async Task<ActionResult<AppSettingsDto>> Get(CancellationToken ct)
    {
        var s = await _settings.GetAsync(ct);
        return Ok(ToDto(s));
    }

    [HttpPut]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<AppSettingsDto>> Update([FromBody] UpdateSettingsRequest request, CancellationToken ct)
    {
        var current = await _settings.GetAsync(ct);
        current.UsdToRubRate = request.UsdToRubRate;
        current.RateUpdatedAt = DateTime.UtcNow;
        current.MinOrderAmount = request.MinOrderAmount;
        current.ContactPhone = request.ContactPhone;
        current.ContactAddress = request.ContactAddress;
        current.WhatsAppUrl = request.WhatsAppUrl;
        current.TelegramUrl = request.TelegramUrl;
        current.UpdatedAt = DateTime.UtcNow;

        await _settings.UpdateAsync(current, ct);
        return Ok(ToDto(current));
    }

    private static AppSettingsDto ToDto(Domain.Entities.AppSettings s) => new(
        s.UsdToRubRate, s.RateUpdatedAt, s.MinOrderAmount, s.ContactPhone, s.ContactAddress, s.WhatsAppUrl, s.TelegramUrl);
}
