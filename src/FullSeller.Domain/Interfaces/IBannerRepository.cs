using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IBannerRepository
{
    /// <summary>Только активные баннеры, по порядку SortOrder — для главной страницы приложения.</summary>
    Task<IReadOnlyList<Banner>> GetActiveAsync(CancellationToken ct = default);
    /// <summary>Все баннеры (включая скрытые) — для списка в админ-панели.</summary>
    Task<IReadOnlyList<Banner>> GetAllAsync(CancellationToken ct = default);
    Task<Banner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(Banner banner, CancellationToken ct = default);
    Task UpdateAsync(Banner banner, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
