namespace FullSeller.Domain.Entities;

/// <summary>
/// Единая строка настроек магазина (всегда одна запись в таблице AppSettings).
/// Покрывает пункты ТЗ «8. Валюта ₽ / $» и «10. Админ-панель → Настройки»:
/// курс доллара (с датой обновления), минимальная сумма заказа, контакты и соцсети.
/// </summary>
public class AppSettings
{
    public Guid Id { get; set; }
    /// <summary>Сколько рублей в 1 долларе — используется клиентом для пересчёта цен при переключении ₽ / $.</summary>
    public decimal UsdToRubRate { get; set; }
    public DateTime RateUpdatedAt { get; set; }
    public decimal MinOrderAmount { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactAddress { get; set; }
    public string? WhatsAppUrl { get; set; }
    public string? TelegramUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
