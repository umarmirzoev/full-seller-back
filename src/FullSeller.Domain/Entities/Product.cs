namespace FullSeller.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? Description { get; set; }
    public string? Composition { get; set; }
    public string BaseSku { get; set; } = default!;
    public int WeightGrams { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Денормализованная минимальная оптовая цена (из PriceTiers) — для быстрой фильтрации/сортировки каталога.</summary>
    public decimal MinPrice { get; set; }
    /// <summary>Старая (зачёркнутая) цена — если задана и выше MinPrice, на клиенте показывается бейдж «Скидка».</summary>
    public decimal? OldPrice { get; set; }
    /// <summary>Метка «Новинка» — управляется вручную из админ-панели (не выводится автоматически по дате).</summary>
    public bool IsNew { get; set; }
    /// <summary>Метка «Хит» — управляется вручную из админ-панели.</summary>
    public bool IsHit { get; set; }
    /// <summary>Партнёр (производитель), добавивший товар через свой кабинет — null для товаров, добавленных из обычной админ-панели.</summary>
    public Guid? PartnerId { get; set; }
    /// <summary>Целевая аудитория (муж/жен/унисекс и т.п.) — задаётся партнёром при добавлении товара.</summary>
    public string? AudienceTag { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
