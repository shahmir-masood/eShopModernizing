using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedMVC.Middleware
{
    /// <summary>
    /// When Azure Active Directory is not configured, this middleware injects a
    /// default authenticated identity so that [Authorize] protected actions remain
    /// reachable, replicating the behaviour of the legacy OWIN AuthenticationMiddleware.
    /// </summary>
    public class AutoAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Developer")
                };
                var identity = new ClaimsIdentity(claims, "AutoAuthentication");
                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }
    }
}
