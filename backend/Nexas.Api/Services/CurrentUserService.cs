using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nexas.Application.Common.Interfaces;
using System.Security.Claims;

namespace Nexas.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public string? FullName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || user.Identity?.IsAuthenticated == false)
                    return null;

                _logger.LogInformation("CurrentUserService.FullName called. Claims available:");
                foreach (var claim in user.Claims)
                {
                    _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
                }

                var name = user.FindFirst("name")?.Value ??
                           user.FindFirst(ClaimTypes.Name)?.Value ??
                           user.FindFirst("given_name")?.Value ??
                           user.FindFirst("preferred_username")?.Value ??
                           user.FindFirst("unique_name")?.Value ??
                           user.FindFirst("nickname")?.Value;

                if (string.IsNullOrWhiteSpace(name))
                {
                    // Fallback para o prefixo do email se o token não tiver a claim de nome
                    var email = Email;
                    if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                    {
                        name = email.Split('@')[0];
                    }
                }

                return name;
            }
        }
    }
}
