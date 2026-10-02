using System.Security.Claims;

namespace FullSeller.WebApi.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException("Токен не содержит идентификатор пользователя.");
    }

    public static bool IsManagerOrAdmin(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirstValue("role");
        return role is "Manager" or "Admin" or "SuperAdmin";
    }

    /// <summary>Admin или SuperAdmin — доступ к панели администратора (не Manager).</summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirstValue("role");
        return role is "Admin" or "SuperAdmin";
    }

    /// <summary>Только SuperAdmin — например, для назначения роли Admin другим пользователям.</summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal principal)
        => principal.FindFirstValue("role") == "SuperAdmin";

    /// <summary>Идентификатор партнёра из токена, выданного PartnerService (claim "sub").</summary>
    public static Guid GetPartnerId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException("Токен не содержит идентификатор партнёра.");
    }

    /// <summary>Партнёрский кабинет (отдельный логин+пароль, не связан с UserRole/Users) — см. PartnerOnlyAttribute.</summary>
    public static bool IsPartner(this ClaimsPrincipal principal)
        => principal.FindFirstValue("role") == "Partner";
}
