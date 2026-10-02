using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.PromoCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Промокоды (ТЗ п.10 «Админ-панель → Промокоды»). Проверка кода при оформлении заказа — публична;
/// управление списком (создание/удаление) — только Admin/SuperAdmin.
/// </summary>
[ApiController]
[Route("api/promocodes")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeRepository _promoCodes;

    public PromoCodesController(IPromoCodeRepository promoCodes)
    {
        _promoCodes = promoCodes;
    }

    /// <summary>Проверяет код и считает скидку для суммы заказа. Счётчик использований не увеличивает —
    /// это происходит только при реальном оформлении заказа (см. IncrementUsageAsync в OrderService).</summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ValidatePromoCodeResponse>> Validate([FromBody] ValidatePromoCodeRequest request, CancellationToken ct)
    {
        var promo = await _promoCodes.GetByCodeAsync(request.Code, ct);
        if (promo is null)
            return Ok(new ValidatePromoCodeResponse(false, "Промокод не найден", 0, request.OrderAmount));

        if (promo.ExpiresAt is not null && promo.ExpiresAt < DateTime.UtcNow)
            return Ok(new ValidatePromoCodeResponse(false, "Срок действия промокода истёк", 0, request.OrderAmount));

        if (promo.UsageLimit is not null && promo.TimesUsed >= promo.UsageLimit)
            return Ok(new ValidatePromoCodeResponse(false, "Промокод больше не действует", 0, request.OrderAmount));

        var discount = promo.DiscountType == DiscountType.Percent
            ? Math.Round(request.OrderAmount * promo.DiscountValue / 100m, 2)
            : promo.DiscountValue;
        discount = Math.Min(discount, request.OrderAmount);

        return Ok(new ValidatePromoCodeResponse(true, null, discount, request.OrderAmount - discount));
    }

    [HttpGet]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<IReadOnlyList<PromoCodeDto>>> GetAll(CancellationToken ct)
    {
        var codes = await _promoCodes.GetAllAsync(ct);
        return Ok(codes.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<PromoCodeDto>> Create([FromBody] CreatePromoCodeRequest request, CancellationToken ct)
    {
        var existing = await _promoCodes.GetByCodeAsync(request.Code, ct);
        if (existing is not null) return Conflict("Промокод с таким кодом уже существует.");

        var promo = new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            ExpiresAt = request.ExpiresAt,
            UsageLimit = request.UsageLimit,
            TimesUsed = 0,
        };
        await _promoCodes.CreateAsync(promo, ct);
        return Ok(ToDto(promo));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [AdminOnly]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _promoCodes.DeleteAsync(id, ct);
        return NoContent();
    }

    private static PromoCodeDto ToDto(PromoCode p) => new(p.Id, p.Code, p.DiscountType, p.DiscountValue, p.ExpiresAt, p.UsageLimit, p.TimesUsed);
}
