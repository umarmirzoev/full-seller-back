namespace FullSeller.WebApi.Contracts.Banners;

public record BannerDto(Guid Id, string Title, string? Subtitle, string? ImageUrl, string? LinkUrl, int SortOrder, bool IsActive);

public record CreateBannerRequest(string Title, string? Subtitle, string? ImageUrl, string? LinkUrl, int SortOrder, bool IsActive);
public record UpdateBannerRequest(string Title, string? Subtitle, string? ImageUrl, string? LinkUrl, int SortOrder, bool IsActive);
