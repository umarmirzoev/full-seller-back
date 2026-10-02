using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = default!;
    public string? FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsLegalEntity { get; set; }
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public int LoyaltyPoints { get; set; }
    public string Language { get; set; } = "ru";
    public string PreferredCurrency { get; set; } = "RUB";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
