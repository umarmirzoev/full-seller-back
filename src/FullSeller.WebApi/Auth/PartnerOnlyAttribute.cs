using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FullSeller.WebApi.Auth;

/// <summary>
/// Ограничивает доступ ролью Partner (отдельный логин+пароль, выдаётся вручную администратором —
/// см. AdminPartnersController; не связано с UserRole/Users). Использовать вместе с [Authorize],
/// как AdminOnlyAttribute используется для админ-панели.
/// </summary>
public class PartnerOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!user.IsPartner())
        {
            context.Result = new ForbidResult();
        }
    }
}
