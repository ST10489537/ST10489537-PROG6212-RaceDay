using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var role = session.GetString("Role");

            // Not logged in -> 401 Unauthorized
            if (userId == null || role == null)
            {
                context.Result = new ObjectResult(new { message = "You must be logged in to access this resource." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Logged in but wrong role -> 403 Forbidden
            if (_requiredRole != null &&
                !string.Equals(role, _requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new { message = $"This action requires the {_requiredRole} role." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}