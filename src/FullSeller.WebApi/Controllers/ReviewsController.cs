using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviews;
    private readonly IOrderRepository _orders;

    public ReviewsController(IReviewRepository reviews, IOrderRepository orders)
    {
        _reviews = reviews;
        _orders = orders;
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetByProduct(Guid productId, CancellationToken ct)
    {
        var reviews = await _reviews.GetByProductIdAsync(productId, ct);
        return Ok(reviews.Where(r => r.Status == ReviewStatus.Approved).Select(ToDto).ToList());
    }

    /// <summary>Очередь модерации отзывов — доступно только менеджеру/админу.</summary>
    [HttpGet("moderation")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetModerationQueue(CancellationToken ct)
    {
        if (!User.IsManagerOrAdmin()) return Forbid();
        var reviews = await _reviews.GetByStatusAsync(ReviewStatus.Pending, ct);
        return Ok(reviews.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var hasDelivered = await _orders.UserHasDeliveredOrderWithProductAsync(userId, request.ProductId, ct);
        if (!hasDelivered)
            return BadRequest("Оставить отзыв можно только после получения товара в заказе.");

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            OrderId = request.OrderId,
            Rating = Math.Clamp(request.Rating, 1, 5),
            Text = request.Text,
            PhotoUrls = request.PhotoUrls.ToList(),
            Status = ReviewStatus.Pending,
        };
        var id = await _reviews.CreateAsync(review, ct);
        return CreatedAtAction(nameof(GetByProduct), new { productId = review.ProductId }, new { id });
    }

    [HttpPut("{id:guid}/moderate")]
    [Authorize]
    public async Task<IActionResult> Moderate(Guid id, [FromBody] ModerateReviewRequest request, CancellationToken ct)
    {
        if (!User.IsManagerOrAdmin()) return Forbid();
        await _reviews.SetStatusAsync(id, request.Approve ? ReviewStatus.Approved : ReviewStatus.Rejected, ct);
        return NoContent();
    }

    private static ReviewDto ToDto(Review r) => new(r.Id, r.ProductId, r.UserId, r.Rating, r.Text, r.PhotoUrls, r.Status, r.CreatedAt);
}
