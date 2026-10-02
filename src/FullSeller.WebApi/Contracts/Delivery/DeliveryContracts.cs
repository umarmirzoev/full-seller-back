using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Delivery;

public record DeliveryCalculateRequest(DeliveryCountry Country, int TotalWeightGrams);
public record DeliveryCalculateResponse(decimal WeightKg, decimal PricePerKg, decimal TotalCost, string Currency);

// ---- Админ-панель: тарифы карго (ТЗ п.7 «Калькулятор карго», п.10 «Настройки → Тарифы доставки») ----

public record CargoRateDto(DeliveryCountry Country, decimal PricePerKg, string Currency);
public record UpsertCargoRateRequest(DeliveryCountry Country, decimal PricePerKg, string Currency);
