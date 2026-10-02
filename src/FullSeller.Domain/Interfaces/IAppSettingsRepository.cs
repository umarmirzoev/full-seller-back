using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IAppSettingsRepository
{
    /// <summary>Всегда возвращает единственную строку настроек (создаёт со значениями по умолчанию, если таблица почему-то пуста).</summary>
    Task<AppSettings> GetAsync(CancellationToken ct = default);
    Task UpdateAsync(AppSettings settings, CancellationToken ct = default);
}
