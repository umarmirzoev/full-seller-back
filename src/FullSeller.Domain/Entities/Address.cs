using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DeliveryCountry Country { get; set; }
    public string City { get; set; } = default!;
    public string Line { get; set; } = default!;
    public bool IsDefault { get; set; }
}
