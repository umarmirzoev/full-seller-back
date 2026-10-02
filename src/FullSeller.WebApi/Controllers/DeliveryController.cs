using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly ICargoCalculator _cargoCalculator;
    private readonly ICargoRateRepository _cargoRates;

    public DeliveryController(ICargoCalculator cargoCalculator, ICargoRateRepository cargoRates)
    {
        _cargoCalculator = cargoCalculator;
        _cargoRates = cargoRates;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<DeliveryCalculateResponse>> Calculate([FromBody] DeliveryCalculateRequest request, CancellationToken ct)
    {
        var quote = await _cargoCalculator.CalculateAsync(request.Country, request.TotalWeightGrams, ct);
        return Ok(new DeliveryCalculateResponse(quote.WeightKg, quote.PricePerKg, quote.TotalCost, quote.Currency));
    }

    /// <summary>Все тарифы карго по странам — для экрана «Настройки → Тарифы доставки» в админ-панели.</summary>
    [HttpGet("rates")]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<IReadOnlyList<CargoRateDto>>> GetRates(CancellationToken ct)
    {
        var rates = await _cargoRates.GetAllAsync(ct);
        return Ok(rates.Select(r => new CargoRateDto(r.Country, r.PricePerKg, r.Currency)).ToList());
    }

    /// <summary>Обновляет цену за кг для страны (создаёт тариф, если для неё его ещё нет).</summary>
    [HttpPut("rates")]
    [Authorize]
    [AdminOnly]
    public async Task<IActionResult> UpsertRate([FromBody] UpsertCargoRateRequest request, CancellationToken ct)
    {
        await _cargoRates.UpsertAsync(new CargoRate { Country = request.Country, PricePerKg = request.PricePerKg, Currency = request.Currency }, ct);
        return NoContent();
    }
}
