using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalTriage.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireLoginAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var path = http.Request.Path;

        // ✅ Allow ASP.NET Core Identity UI routes
        if (path.StartsWithSegments("/Identity"))
            return;

        // ✅ Allow static files (optional safety)
        if (path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js")
            || path.StartsWithSegments("/lib")
            || path.StartsWithSegments("/images")
            || path.StartsWithSegments("/favicon.ico"))
            return;

        // If already authenticated, continue
        var user = http.User;
        if (user?.Identity?.IsAuthenticated == true)
            return;

        // API: return 401
        if (path.StartsWithSegments("/api"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // MVC: redirect to Login with returnUrl
        var returnUrl = http.Request.Path + http.Request.QueryString;
        const string loginPath = "/Identity/Account/Login";
        context.Result = new RedirectResult($"{loginPath}?returnUrl={UrlEncoder.Default.Encode(returnUrl)}");
    }
}
