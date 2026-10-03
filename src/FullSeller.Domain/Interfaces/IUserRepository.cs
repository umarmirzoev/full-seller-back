using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Interfaces;

/// <summary>Сводная статистика по пользователям для дашборда администратора.</summary>
public record UserStats(int TotalUsers, int NewUsersToday, int NewUsersThisWeek);

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<Guid> CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>PBKDF2-хеш пароля пользователя (null — пароль ещё не задан).</summary>
    Task<string?> GetPasswordHashAsync(Guid userId, CancellationToken ct = default);
    Task SetPasswordHashAsync(Guid userId, string passwordHash, CancellationToken ct = default);

    /// <summary>Список пользователей для админ-панели: поиск по телефону/имени, фильтр по роли, постранично.</summary>
    Task<PagedResult<User>> GetAllAsync(int page, int pageSize, string? search = null, UserRole? role = null, CancellationToken ct = default);

    Task<UserStats> GetStatsAsync(CancellationToken ct = default);

    /// <summary>Точечное изменение роли пользователя (без загрузки/сохранения всей сущности).</summary>
    Task SetRoleAsync(Guid userId, UserRole role, CancellationToken ct = default);
}
