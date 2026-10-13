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
    private readonly OrderNotifier _notifier;

    public OrderService(IUnitOfWork unitOfWork, ICargoCalculator cargoCalculator, INotificationRepository notificationRepository, OrderNotifier notifier)
    {
        _notifier = notifier;
        _unitOfWork = unitOfWork;
        _cargoCalculator = cargoCalculator;
        _notificationRepository = notificationRepository;
    }

    public async Task<Order> CreateOrderFromCartAsync(
        Guid userId, Guid addressId, DeliveryCountry country, PaymentMethod paymentMethod,
        string? contactName, string? comment, OrderNotifyChannel? notifyChannel, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, ct)
                ?? throw new NotFoundException(nameof(User), userId);
            var address = await _unitOfWork.Addresses.GetByIdAsync(addressId, ct);
            if (address is null || address.UserId != userId)
                throw new ValidationException("Адрес доставки не найден. Выберите или добавьте адрес.");

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
            var lines = new List<string>();

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

                var variantLabel = string.Join(" / ", new[] { variant.Size, variant.Color }.Where(x => !string.IsNullOrWhiteSpace(x)));
                lines.Add($"{lines.Count + 1}. {product.Name}{(variantLabel.Length > 0 ? $" ({variantLabel})" : "")} — {item.Quantity} шт × {unitPrice:0} ₽ = {unitPrice * item.Quantity:0} ₽");

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
                ContactName = string.IsNullOrWhiteSpace(contactName) ? user.FullName : contactName.Trim(),
                ContactPhone = user.Phone,
                AddressText = $"{CountryLabel(address.Country)}, {address.City}, {address.Line}",
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                NotifyChannel = notifyChannel,
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

            // Автоотправка владельцу в выбранный клиентом мессенджер (не блокирует ответ клиенту, ошибки только в лог).
            _notifier.Enqueue(notifyChannel ?? OrderNotifyChannel.Telegram, BuildNotificationText(order, lines));
            return order;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    private static string CountryLabel(DeliveryCountry c) => c switch
    {
        DeliveryCountry.Russia => "Россия",
        DeliveryCountry.Tajikistan => "Таджикистан",
        DeliveryCountry.Uzbekistan => "Узбекистан",
        DeliveryCountry.Kazakhstan => "Казахстан",
        DeliveryCountry.Kyrgyzstan => "Кыргызстан",
        _ => "Другая страна"
    };

    private static string BuildNotificationText(Order order, List<string> lines)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"🛒 Новый заказ {order.OrderNumber}");
        sb.AppendLine();
        sb.AppendLine($"👤 {(string.IsNullOrWhiteSpace(order.ContactName) ? "—" : order.ContactName)}");
        sb.AppendLine($"📞 {order.ContactPhone}");
        sb.AppendLine($"📍 {order.AddressText}");
        sb.AppendLine($"💳 Оплата: {(order.PaymentMethod == PaymentMethod.Invoice ? "по счёту" : "картой")}");
        sb.AppendLine();
        sb.AppendLine("Товары:");
        foreach (var l in lines) sb.AppendLine(l);
        sb.AppendLine();
        sb.AppendLine($"🚚 Доставка: {order.DeliveryCost:0} ₽");
        sb.AppendLine($"💰 Итого: {order.TotalAmount:0} ₽");
        if (!string.IsNullOrWhiteSpace(order.Comment)) { sb.AppendLine(); sb.AppendLine($"💬 {order.Comment}"); }
        return sb.ToString().TrimEnd();
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
