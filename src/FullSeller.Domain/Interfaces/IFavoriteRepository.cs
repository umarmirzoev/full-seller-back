using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default);
}
