using Microsoft.AspNetCore.Http;
using Nexas.Application.Common.Interfaces;
using System.Security.Claims;

namespace Nexas.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? ExternalId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                
                if (user == null || user.Identity?.IsAuthenticated == false)
                    return null;

                // Com MapInboundClaims=false, o JWT chega com nome original "nameid".
                // ClaimTypes.NameIdentifier é o namespace longo (mapeado pelo handler por padrão).
                // Testamos os dois para garantir compatibilidade.
                return user.FindFirst("nameid")?.Value ??
                       user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                       user.FindFirst("sub")?.Value ??
                       user.FindFirst("id")?.Value ??
                       user.FindFirst("external_id")?.Value;
            }
        }

        public string? Email
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || user.Identity?.IsAuthenticated == false)
                    return null;

                // "email" é o nome original do claim no JWT (com MapInboundClaims=false).
                return user.FindFirst("email")?.Value ??
                       user.FindFirst(ClaimTypes.Email)?.Value;
            }
        }
    }
}
