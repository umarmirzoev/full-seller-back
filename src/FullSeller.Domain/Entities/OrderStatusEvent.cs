using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class OrderStatusEvent
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
