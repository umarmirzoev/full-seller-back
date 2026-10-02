using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Repositories;
using Npgsql;

namespace FullSeller.Infrastructure.Data;

/// <summary>
/// Открывает одно SQL-соединение и одну транзакцию и раздаёт репозитории, которые её разделяют —
/// используется там, где нужна атомарность поверх нескольких таблиц (в первую очередь — оформление заказа).
/// Для обычных операций чтения (просмотр каталога и т.п.) репозитории инжектируются в контроллеры
/// напрямую через ISqlConnectionFactory и работают в автономном режиме (см. Program.cs).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ISqlConnectionFactory _factory;
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;

    public UnitOfWork(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _connection = await _factory.CreateOpenConnectionAsync(ct);
        _transaction = (NpgsqlTransaction)await _connection.BeginTransactionAsync(ct);

        Users = new UserRepository(_connection, _transaction);
        Addresses = new AddressRepository(_connection, _transaction);
        OtpCodes = new OtpRepository(_connection, _transaction);
        RefreshTokens = new RefreshTokenRepository(_connection, _transaction);
        Catalog = new CatalogRepository(_connection, _transaction);
        Carts = new CartRepository(_connection, _transaction);
        Orders = new OrderRepository(_connection, _transaction);
        Reviews = new ReviewRepository(_connection, _transaction);
        Favorites = new FavoriteRepository(_connection, _transaction);
        Chat = new ChatRepository(_connection, _transaction);
        Notifications = new NotificationRepository(_connection, _transaction);
        CargoRates = new CargoRateRepository(_connection, _transaction);
        Settings = new AppSettingsRepository(_connection, _transaction);
        Banners = new BannerRepository(_connection, _transaction);
        PromoCodes = new PromoCodeRepository(_connection, _transaction);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) await _transaction.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) await _transaction.RollbackAsync(ct);
    }

    public IUserRepository Users { get; private set; } = default!;
    public IAddressRepository Addresses { get; private set; } = default!;
    public IOtpRepository OtpCodes { get; private set; } = default!;
    public IRefreshTokenRepository RefreshTokens { get; private set; } = default!;
    public ICatalogRepository Catalog { get; private set; } = default!;
    public ICartRepository Carts { get; private set; } = default!;
    public IOrderRepository Orders { get; private set; } = default!;
    public IReviewRepository Reviews { get; private set; } = default!;
    public IFavoriteRepository Favorites { get; private set; } = default!;
    public IChatRepository Chat { get; private set; } = default!;
    public INotificationRepository Notifications { get; private set; } = default!;
    public ICargoRateRepository CargoRates { get; private set; } = default!;
    public IAppSettingsRepository Settings { get; private set; } = default!;
    public IBannerRepository Banners { get; private set; } = default!;
    public IPromoCodeRepository PromoCodes { get; private set; } = default!;

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
