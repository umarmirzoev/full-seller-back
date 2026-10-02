using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class CargoRate
{
    public Guid Id { get; set; }
    public DeliveryCountry Country { get; set; }
    public decimal PricePerKg { get; set; }
    public string Currency { get; set; } = "RUB";
}
