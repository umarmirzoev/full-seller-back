using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<Guid> CreateAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAsync(Guid id, CancellationToken ct = default);
}
