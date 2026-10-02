using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Profile;

public record ProfileDto(Guid Id, string Phone, string? FullName, bool IsLegalEntity, string? LegalName, string? TaxId, int LoyaltyPoints, string Language, string PreferredCurrency);
public record UpdateProfileRequest(string? FullName, string? Language, bool? IsLegalEntity, string? LegalName, string? TaxId);

public record AddressDto(Guid Id, DeliveryCountry Country, string City, string Line, bool IsDefault);
public record CreateAddressRequest(DeliveryCountry Country, string City, string Line);
