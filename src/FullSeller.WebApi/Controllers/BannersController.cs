using FullSeller.Domain.Entities;
using FullSeller.Domain.Exceptions;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Banners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Рекламные баннеры главной страницы (ТЗ п.3 «Большой рекламный баннер», п.10 «Настройки → Баннеры»).
/// Публично отдаются только активные баннеры; полный список и управление — Admin/SuperAdmin.
/// </summary>
[ApiController]
[Route("api/banners")]
public class BannersController : ControllerBase
{
    private readonly IBannerRepository _banners;

    public BannersController(IBannerRepository banners)
    {
        _banners = banners;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BannerDto>>> GetActive(CancellationToken ct)
    {
        var banners = await _banners.GetActiveAsync(ct);
        return Ok(banners.Select(ToDto).ToList());
    }

    /// <summary>Все баннеры, включая скрытые — для списка в админ-панели.</summary>
    [HttpGet("all")]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<IReadOnlyList<BannerDto>>> GetAll(CancellationToken ct)
    {
        var banners = await _banners.GetAllAsync(ct);
        return Ok(banners.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize]
    [AdminOnly]
    public async Task<ActionResult<BannerDto>> Create([FromBody] CreateBannerRequest request, CancellationToken ct)
    {
        var banner = new Banner
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Subtitle = request.Subtitle,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
        };
        await _banners.CreateAsync(banner, ct);
        return Ok(ToDto(banner));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [AdminOnly]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBannerRequest request, CancellationToken ct)
    {
        var banner = await _banners.GetByIdAsync(id, ct) ?? throw new NotFoundException("Баннер", id);
        banner.Title = request.Title;
        banner.Subtitle = request.Subtitle;
        banner.ImageUrl = request.ImageUrl;
        banner.LinkUrl = request.LinkUrl;
        banner.SortOrder = request.SortOrder;
        banner.IsActive = request.IsActive;

        await _banners.UpdateAsync(banner, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [AdminOnly]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _banners.DeleteAsync(id, ct);
        return NoContent();
    }

    private static BannerDto ToDto(Banner b) => new(b.Id, b.Title, b.Subtitle, b.ImageUrl, b.LinkUrl, b.SortOrder, b.IsActive);
}
