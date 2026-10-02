using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Text { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
