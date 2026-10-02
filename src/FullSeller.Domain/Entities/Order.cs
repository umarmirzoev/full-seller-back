using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DeliveryCountry DeliveryCountry { get; set; }
    public Guid AddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DeliveryCost { get; set; }
    public string? CargoTrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}
