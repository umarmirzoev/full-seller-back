using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;

namespace FullSeller.Domain.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetByStatusAsync(ReviewStatus status, CancellationToken ct = default);
    Task<Guid> CreateAsync(Review review, CancellationToken ct = default);
    Task SetStatusAsync(Guid reviewId, ReviewStatus status, CancellationToken ct = default);
}
