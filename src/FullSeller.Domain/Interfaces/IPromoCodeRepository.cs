using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IPromoCodeRepository
{
    Task<IReadOnlyList<PromoCode>> GetAllAsync(CancellationToken ct = default);
    Task<PromoCode?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<PromoCode?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(PromoCode promoCode, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    /// <summary>Атомарно увеличивает счётчик использований — вызывается при оформлении заказа с промокодом.</summary>
    Task IncrementUsageAsync(Guid id, CancellationToken ct = default);
}
