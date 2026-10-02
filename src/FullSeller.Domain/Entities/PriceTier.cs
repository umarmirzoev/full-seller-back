namespace FullSeller.Domain.Entities;

/// <summary>Оптовая шкала цен: чем больше MinQuantity, тем ниже PricePerUnit.</summary>
public class PriceTier
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int MinQuantity { get; set; }
    public decimal PricePerUnit { get; set; }
}
