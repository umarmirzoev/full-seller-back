using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Interfaces;

public interface ICargoRateRepository
{
    Task<CargoRate?> GetByCountryAsync(DeliveryCountry country, CancellationToken ct = default);
    /// <summary>Все тарифы карго — для списка в админ-панели («Настройки → Тарифы доставки»).</summary>
    Task<IReadOnlyList<CargoRate>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Обновляет цену за кг (и валюту) для страны — создаёт тариф, если для страны его ещё нет.</summary>
    Task UpsertAsync(CargoRate rate, CancellationToken ct = default);
}
