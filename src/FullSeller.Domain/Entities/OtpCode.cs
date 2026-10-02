namespace FullSeller.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = default!;
    public string CodeHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
