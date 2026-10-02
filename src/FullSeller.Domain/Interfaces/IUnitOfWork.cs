namespace FullSeller.Domain.Interfaces;

/// <summary>Оборачивает несколько репозиториев в одну SQL-транзакцию (System.Data.IDbTransaction).
/// Репозитории, полученные через Get*Repository(), используют то же соединение/транзакцию.</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);

    IUserRepository Users { get; }
    IAddressRepository Addresses { get; }
    IOtpRepository OtpCodes { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ICatalogRepository Catalog { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    IReviewRepository Reviews { get; }
    IFavoriteRepository Favorites { get; }
    IChatRepository Chat { get; }
    INotificationRepository Notifications { get; }
    ICargoRateRepository CargoRates { get; }
    IAppSettingsRepository Settings { get; }
    IBannerRepository Banners { get; }
    IPromoCodeRepository PromoCodes { get; }
}
