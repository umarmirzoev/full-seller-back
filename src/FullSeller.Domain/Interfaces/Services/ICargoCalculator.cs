using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Interfaces.Services;

public record CargoQuote(decimal WeightKg, decimal PricePerKg, decimal TotalCost, string Currency);

public interface ICargoCalculator
{
    Task<CargoQuote> CalculateAsync(DeliveryCountry country, int totalWeightGrams, CancellationToken ct = default);
}
