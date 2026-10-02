using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class PromoCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
    public int TimesUsed { get; set; }
}
