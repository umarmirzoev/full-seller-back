using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IAddressRepository _addresses;

    public ProfileController(IUserRepository users, IAddressRepository addresses)
    {
        _users = users;
        _addresses = addresses;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile(CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(User.GetUserId(), ct);
        if (user is null) return NotFound();
        return Ok(ToDto(user));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(User.GetUserId(), ct);
        if (user is null) return NotFound();

        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.Language is not null) user.Language = request.Language;
        if (request.IsLegalEntity is not null) user.IsLegalEntity = request.IsLegalEntity.Value;
        if (request.LegalName is not null) user.LegalName = request.LegalName;
        if (request.TaxId is not null) user.TaxId = request.TaxId;

        await _users.UpdateAsync(user, ct);
        return Ok(ToDto(user));
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAddresses(CancellationToken ct)
    {
        var addresses = await _addresses.GetByUserIdAsync(User.GetUserId(), ct);
        return Ok(addresses.Select(a => new AddressDto(a.Id, a.Country, a.City, a.Line, a.IsDefault)).ToList());
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var address = new Address
        {
            UserId = User.GetUserId(),
            Country = request.Country,
            City = request.City,
            Line = request.Line,
        };
        address.Id = await _addresses.CreateAsync(address, ct);
        return Ok(new AddressDto(address.Id, address.Country, address.City, address.Line, address.IsDefault));
    }

    [HttpPut("addresses/{id:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress(Guid id, CancellationToken ct)
    {
        await _addresses.SetDefaultAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    private static ProfileDto ToDto(User u) => new(u.Id, u.Phone, u.FullName, u.IsLegalEntity, u.LegalName, u.TaxId, u.LoyaltyPoints, u.Language, u.PreferredCurrency);
}
