using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalTriage.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles)
    {
        _roles = roles ?? Array.Empty<string>();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var path = http.Request.Path;

        // ✅ Do not interfere with ASP.NET Core Identity UI
        if (path.StartsWithSegments("/Identity"))
            return;

        var user = http.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            if (path.StartsWithSegments("/api"))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // ✅ keep returnUrl like RequireLogin
            var returnUrl = http.Request.Path + http.Request.QueryString;
            const string loginPath = "/Identity/Account/Login";
            context.Result = new RedirectResult($"{loginPath}?returnUrl={UrlEncoder.Default.Encode(returnUrl)}");
            return;
        }

        // No role requirement -> allow
        if (_roles.Length == 0)
            return;

        // Any role match -> allow
        if (_roles.Any(user.IsInRole))
            return;

        if (path.StartsWithSegments("/api"))
        {
            context.Result = new ForbidResult();
            return;
        }

        context.Result = new RedirectResult("/Home/AccessDenied");
    }
}
