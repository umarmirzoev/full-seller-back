namespace FullSeller.Domain.Entities;

/// <summary>Refresh-токен партнёрского кабинета — отдельная таблица от <see cref="RefreshToken"/>,
/// т.к. ссылается на Partners(Id), а не на Users(Id).</summary>
public class PartnerRefreshToken
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
