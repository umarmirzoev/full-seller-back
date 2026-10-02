using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Auth;
using FullSeller.WebApi.Contracts.Catalog;
using FullSeller.WebApi.Contracts.Partner;
using FullSeller.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Кабинет партнёра (производителя/оптового поставщика): отдельный вход по логину и паролю
/// (учётку создаёт администратор — см. AdminPartnersController, это не клиентский OTP-флоу),
/// просмотр своего профиля и добавление собственных товаров в общий каталог.
/// </summary>
[ApiController]
[Route("api/partner")]
public class PartnerController : ControllerBase
{
    private readonly PartnerService _partnerService;

    public PartnerController(PartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenPairResponse>> Login([FromBody] PartnerLoginRequest dto, CancellationToken ct)
    {
        var tokens = await _partnerService.LoginAsync(dto.Login, dto.Password, ct);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var tokens = await _partnerService.RefreshAsync(dto.RefreshToken, ct);
        return Ok(tokens);
    }

    [HttpGet("me")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<PartnerProfileDto>> GetMe(CancellationToken ct)
    {
        var profile = await _partnerService.GetProfileAsync(User.GetPartnerId(), ct);
        return Ok(profile);
    }

    /// <summary>Товары, добавленные этим партнёром (постранично).</summary>
    [HttpGet("products")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<AdminProductListResponse>> GetProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _partnerService.GetProductsAsync(User.GetPartnerId(), page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Добавляет новый товар в общий каталог от имени партнёра (переиспользует DTO админ-панели).</summary>
    [HttpPost("products")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<AdminProductDto>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _partnerService.CreateProductAsync(User.GetPartnerId(), request, ct);
        return Ok(product);
    }

    /// <summary>Редактирует собственный товар партнёра; после правки товар снова уходит на модерацию.</summary>
    [HttpPut("products/{id:guid}")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<AdminProductDto>> UpdateProduct(Guid id, [FromBody] PartnerUpdateProductRequest request, CancellationToken ct)
    {
        var product = await _partnerService.UpdateProductAsync(User.GetPartnerId(), id, request, ct);
        return Ok(product);
    }

    /// <summary>Варианты (размер/цвет/сток) собственного товара — для предзаполнения формы редактирования.</summary>
    [HttpGet("products/{id:guid}/variants")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<IReadOnlyList<VariantDto>>> GetProductVariants(Guid id, CancellationToken ct)
    {
        var variants = await _partnerService.GetProductVariantsAsync(User.GetPartnerId(), id, ct);
        return Ok(variants);
    }

    /// <summary>Оптовая сетка цен ("мешки") собственного товара — для предзаполнения формы редактирования.</summary>
    [HttpGet("products/{id:guid}/price-tiers")]
    [Authorize]
    [PartnerOnly]
    public async Task<ActionResult<IReadOnlyList<PriceTierRequest>>> GetProductPriceTiers(Guid id, CancellationToken ct)
    {
        var tiers = await _partnerService.GetProductPriceTiersAsync(User.GetPartnerId(), id, ct);
        return Ok(tiers);
    }

    /// <summary>Удаляет собственный товар партнёра.</summary>
    [HttpDelete("products/{id:guid}")]
    [Authorize]
    [PartnerOnly]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        await _partnerService.DeleteProductAsync(User.GetPartnerId(), id, ct);
        return NoContent();
    }
}
