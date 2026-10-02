using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FullSeller.WebApi.Auth;

/// <summary>
/// Ограничивает доступ к контроллеру/действию ролями Admin и SuperAdmin (Manager — нет).
/// Использовать вместе с [Authorize] на том же контроллере: [Authorize] отвечает за
/// «пользователь вообще залогинен», этот атрибут — за «залогинен именно как админ».
/// </summary>
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!user.IsAdmin())
        {
            context.Result = new ForbidResult();
        }
    }
}
