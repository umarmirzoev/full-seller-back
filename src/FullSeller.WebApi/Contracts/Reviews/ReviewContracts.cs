using FullSeller.Domain.Enums;

namespace FullSeller.WebApi.Contracts.Reviews;

public record ReviewDto(Guid Id, Guid ProductId, Guid UserId, int Rating, string? Text, IReadOnlyList<string> PhotoUrls, ReviewStatus Status, DateTime CreatedAt);
public record CreateReviewRequest(Guid ProductId, Guid OrderId, int Rating, string? Text, IReadOnlyList<string> PhotoUrls);
public record ModerateReviewRequest(bool Approve);
