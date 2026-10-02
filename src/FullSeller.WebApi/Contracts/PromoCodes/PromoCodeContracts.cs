using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.PromoCodes;

public record PromoCodeDto(Guid Id, string Code, DiscountType DiscountType, decimal DiscountValue, DateTime? ExpiresAt, int? UsageLimit, int TimesUsed);

public record CreatePromoCodeRequest(string Code, DiscountType DiscountType, decimal DiscountValue, DateTime? ExpiresAt, int? UsageLimit);

public record ValidatePromoCodeRequest(string Code, decimal OrderAmount);

/// <summary>DiscountAmount — сколько денег вычесть из OrderAmount; уже посчитано сервером (Percent/Fixed), клиенту нужно только число.</summary>
public record ValidatePromoCodeResponse(bool IsValid, string? Error, decimal DiscountAmount, decimal FinalAmount);
