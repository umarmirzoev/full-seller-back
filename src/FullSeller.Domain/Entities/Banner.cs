namespace FullSeller.Domain.Entities;

/// <summary>Рекламный баннер главной страницы (ТЗ п.3 «Большой рекламный баннер», п.10 «Настройки → Баннеры»).</summary>
public class Banner
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>Куда ведёт баннер при нажатии — например, /catalog?category=... или конкретный товар.</summary>
    public string? LinkUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
