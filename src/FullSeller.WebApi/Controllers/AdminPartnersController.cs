using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Partner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

/// <summary>
/// Админ-панель → партнёры (производители/оптовые поставщики): создание учётной записи
/// (логин и пароль выдаются вручную администратором — отдельно от клиентского OTP-входа, см. PartnerController)
/// и просмотр списка. Доступ — только Admin и SuperAdmin, как и остальная админ-панель.
/// </summary>
[ApiController]
[Route("api/admin/partners")]
[Authorize]
[AdminOnly]
public class AdminPartnersController : ControllerBase
{
    private readonly IPartnerRepository _partners;
    private static readonly PasswordHasher<Partner> PasswordHasher = new();

    public AdminPartnersController(IPartnerRepository partners)
    {
        _partners = partners;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PartnerProfileDto>>> GetAll(CancellationToken ct)
    {
        var partners = await _partners.GetAllAsync(ct);
        return Ok(partners.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<PartnerProfileDto>> Create([FromBody] CreatePartnerRequest request, CancellationToken ct)
    {
        var partner = new Partner
        {
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            Phone = request.Phone,
            Login = request.Login,
            IsActive = true,
        };
        partner.PasswordHash = PasswordHasher.HashPassword(partner, request.Password);

        await _partners.CreateAsync(partner, ct);
        return Ok(ToDto(partner));
    }

    private static PartnerProfileDto ToDto(Partner p) => new(p.Id, p.CompanyName, p.ContactName, p.Phone, p.Login, p.IsActive, p.CreatedAt);
}
