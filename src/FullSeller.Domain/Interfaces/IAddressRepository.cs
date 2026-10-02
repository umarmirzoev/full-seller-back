using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(Address address, CancellationToken ct = default);
    Task SetDefaultAsync(Guid userId, Guid addressId, CancellationToken ct = default);
}
