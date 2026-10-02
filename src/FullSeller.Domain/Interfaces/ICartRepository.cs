using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Guid> AddItemAsync(Guid cartId, Guid productVariantId, int quantity, CancellationToken ct = default);
    Task UpdateQuantityAsync(Guid cartItemId, int quantity, CancellationToken ct = default);
    Task RemoveItemAsync(Guid cartItemId, CancellationToken ct = default);
    Task ClearAsync(Guid cartId, CancellationToken ct = default);
}
