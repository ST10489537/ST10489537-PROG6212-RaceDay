using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Web.Helpers;

namespace RaceDay.Web.Filters;

// Put on a controller or action to protect it.
//   [SessionAuthorize]                    -> any logged-in user
//   [SessionAuthorize(Roles.Organiser)]   -> Organisers only
// Not logged in -> redirected to Login. Wrong role -> Access Denied page.
public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string? _role;

    public SessionAuthorizeAttribute(string? role = null)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32(SessionKeys.UserId) == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (_role != null &&
            !string.Equals(session.GetString(SessionKeys.Role), _role,
                StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}