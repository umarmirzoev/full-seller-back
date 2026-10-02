using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;
using FullSeller.Infrastructure.Data;
using FullSeller.Infrastructure.Repositories;
using FullSeller.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FullSeller.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFullSellerInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<MigrationRunner>();

        // Репозитории — автономный режим (каждый вызов открывает своё соединение).
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICargoRateRepository, CargoRateRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        services.AddScoped<IBannerRepository, BannerRepository>();
        services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();

        // UnitOfWork — для сценариев, где нужна одна транзакция на несколько репозиториев (заказы).
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Внешние сервисы.
        services.AddSingleton<ISmsSender, ConsoleSmsSender>();
        services.AddSingleton<ITelegramAuthValidator, TelegramAuthValidator>();
        services.AddSingleton<IPushNotifier, FirebasePushNotifier>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<ICargoCalculator, CargoCalculator>();

        return services;
    }
}
