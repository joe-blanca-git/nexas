using System.Security.Claims;
using Nexas.Api.Services;
using Nexas.Domain.Constants;

namespace Nexas.Api.Middlewares
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUserProvider currentUserProvider)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var externalId = context.User.FindFirst(AuthConstants.ExternalIdClaim)?.Value;

                if (!string.IsNullOrEmpty(externalId))
                {
                    currentUserProvider.SetExternalId(externalId);
                }
            }

            await _next(context);
        }
    }

    public static class UserContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserContextMiddleware>();
        }
    }
}
