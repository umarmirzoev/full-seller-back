namespace FullSeller.Domain.Enums;

/// <summary>Куда автоматически отправить уведомление о новом заказе (выбирает клиент).</summary>
public enum OrderNotifyChannel
{
    WhatsApp = 0,
    Telegram = 1
}
