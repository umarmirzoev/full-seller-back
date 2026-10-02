using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;

namespace FullSeller.Infrastructure.Services;

public class CargoCalculator : ICargoCalculator
{
    private readonly ICargoRateRepository _cargoRates;

    public CargoCalculator(ICargoRateRepository cargoRates)
    {
        _cargoRates = cargoRates;
    }

    public async Task<CargoQuote> CalculateAsync(DeliveryCountry country, int totalWeightGrams, CancellationToken ct = default)
    {
        var rate = await _cargoRates.GetByCountryAsync(country, ct)
            ?? throw new InvalidOperationException($"Тариф карго для страны {country} не настроен.");

        var weightKg = Math.Ceiling(totalWeightGrams / 1000m * 10) / 10; // округление вверх до 0.1 кг
        var total = Math.Round(weightKg * rate.PricePerKg, 2);
        return new CargoQuote(weightKg, rate.PricePerKg, total, rate.Currency);
    }
}
