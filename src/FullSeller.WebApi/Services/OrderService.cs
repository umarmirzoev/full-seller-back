using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Exceptions;
using FullSeller.Domain.Interfaces;
using FullSeller.Domain.Interfaces.Services;

namespace FullSeller.WebApi.Services;

public class OrderService
{
    private const decimal MinimumOrderAmount = 2000m;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICargoCalculator _cargoCalculator;
    private readonly INotificationRepository _notificationRepository;

    public OrderService(IUnitOfWork unitOfWork, ICargoCalculator cargoCalculator, INotificationRepository notificationRepository)
    {
        _unitOfWork = unitOfWork;
        _cargoCalculator = cargoCalculator;
        _notificationRepository = notificationRepository;
    }

    public async Task<Order> CreateOrderFromCartAsync(
        Guid userId, Guid addressId, DeliveryCountry country, PaymentMethod paymentMethod, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var cart = await _unitOfWork.Carts.GetOrCreateByUserIdAsync(userId, ct);
            if (cart.Items.Count == 0)
                throw new ValidationException("Корзина пуста.");

            var orderItems = new List<OrderItem>();
            var quantityByProduct = new Dictionary<Guid, int>();
            var variantByItem = new Dictionary<Guid, (ProductVariant Variant, Product Product)>();

            foreach (var item in cart.Items)
            {
                var variant = await _unitOfWork.Catalog.GetVariantByIdAsync(item.ProductVariantId, ct)
                    ?? throw new NotFoundException(nameof(ProductVariant), item.ProductVariantId);
                var product = await _unitOfWork.Catalog.GetProductByIdAsync(variant.ProductId, ct)
                    ?? throw new NotFoundException(nameof(Product), variant.ProductId);

                if (variant.StockQuantity < item.Quantity)
                    throw new ValidationException($"Недостаточно остатка «{product.Name}» ({variant.Size}/{variant.Color}).");

                variantByItem[item.Id] = (variant, product);
                quantityByProduct[product.Id] = quantityByProduct.GetValueOrDefault(product.Id) + item.Quantity;
            }

            var priceTierCache = new Dictionary<Guid, IReadOnlyList<Domain.Entities.PriceTier>>();
            decimal totalAmount = 0;
            var totalWeightGrams = 0;

            foreach (var item in cart.Items)
            {
                var (variant, product) = variantByItem[item.Id];
                if (!priceTierCache.TryGetValue(product.Id, out var tiers))
                {
                    tiers = await _unitOfWork.Catalog.GetPriceTiersAsync(product.Id, ct);
                    priceTierCache[product.Id] = tiers;
                }

                var productTotalQty = quantityByProduct[product.Id];
                var unitPrice = ResolveUnitPrice(tiers, productTotalQty, product.MinPrice);

                orderItems.Add(new OrderItem
                {
                    ProductVariantId = variant.Id,
                    Quantity = item.Quantity,
                    UnitPriceAtOrderTime = unitPrice,
                });

                totalAmount += unitPrice * item.Quantity;
                totalWeightGrams += product.WeightGrams * item.Quantity;
            }

            if (totalAmount < MinimumOrderAmount)
                throw new ValidationException($"Минимальная сумма заказа — {MinimumOrderAmount:0} ₽.");

            var cargoQuote = await _cargoCalculator.CalculateAsync(country, totalWeightGrams, ct);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                Status = OrderStatus.New,
                DeliveryCountry = country,
                AddressId = addressId,
                PaymentMethod = paymentMethod,
                TotalAmount = totalAmount + cargoQuote.TotalCost,
                DeliveryCost = cargoQuote.TotalCost,
                Items = orderItems,
            };

            await _unitOfWork.Orders.CreateAsync(order, ct);

            foreach (var item in cart.Items)
            {
                await _unitOfWork.Catalog.DecrementStockAsync(item.ProductVariantId, item.Quantity, ct);
            }
            await _unitOfWork.Carts.ClearAsync(cart.Id, ct);

            await _unitOfWork.Notifications.CreateAsync(new Notification
            {
                UserId = userId,
                Type = NotificationType.OrderStatusChanged,
                Title = "Заказ принят в работу",
                Body = $"Заказ {order.OrderNumber} на сумму {order.TotalAmount:0} ₽ принят в обработку.",
            }, ct);

            await _unitOfWork.CommitAsync(ct);
            return order;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<Order> RepeatOrderAsync(Guid userId, Guid sourceOrderId, CancellationToken ct)
    {
        var source = await _unitOfWork.Orders.GetByIdAsync(sourceOrderId, ct)
            ?? throw new NotFoundException(nameof(Order), sourceOrderId);
        if (source.UserId != userId)
            throw new UnauthorizedDomainException("Заказ принадлежит другому пользователю.");

        var cart = await _unitOfWork.Carts.GetOrCreateByUserIdAsync(userId, ct);
        foreach (var item in source.Items)
        {
            var variant = await _unitOfWork.Catalog.GetVariantByIdAsync(item.ProductVariantId, ct);
            if (variant is null || variant.StockQuantity < item.Quantity) continue; // нет в наличии — пропускаем
            await _unitOfWork.Carts.AddItemAsync(cart.Id, item.ProductVariantId, item.Quantity, ct);
        }
        return source;
    }

    private static decimal ResolveUnitPrice(IReadOnlyList<Domain.Entities.PriceTier> tiers, int quantity, decimal fallback)
    {
        if (tiers.Count == 0) return fallback;
        var applicable = tiers.Where(t => t.MinQuantity <= quantity).OrderByDescending(t => t.MinQuantity).FirstOrDefault();
        return applicable?.PricePerUnit ?? tiers.OrderBy(t => t.MinQuantity).First().PricePerUnit;
    }

    private static string GenerateOrderNumber() => $"FS-{Random.Shared.Next(10000, 99999)}";
}
