using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Partner?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<IReadOnlyList<Partner>> GetAllAsync(CancellationToken ct = default);
    Task<Guid> CreateAsync(Partner partner, CancellationToken ct = default);

    /// <summary>Товары, добавленные конкретным партнёром через его личный кабинет (api/partner/products).</summary>
    Task<PagedResult<Product>> GetProductsByPartnerAsync(Guid partnerId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Создаёт товар в общем каталоге и помечает его как принадлежащий данному партнёру (Products.PartnerId).</summary>
    Task<Guid> CreateProductForPartnerAsync(Guid partnerId, Product product, CancellationToken ct = default);

    Task<Guid> CreateRefreshTokenAsync(PartnerRefreshToken token, CancellationToken ct = default);
    Task<PartnerRefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(Guid id, CancellationToken ct = default);
}
