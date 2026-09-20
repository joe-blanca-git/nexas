using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Nexas.SystemManager.Services
{
    /// <summary>
    /// Substitui o BearerTokenProtector padrão do ASP.NET Core Identity (que gera tokens opacos
    /// e criptografados via Data Protection) por um JWT de verdade, usando a mesma Issuer/Audience/Key
    /// da seção "Jwt" já usada pelas demais APIs Nexas. Preserva os claims do Identity (ClaimTypes.*,
    /// já em formato longo) sem remapeamento de saída, para casar com o RoleClaimType das outras APIs
    /// e com o que o frontend (AuthUtil) já espera decodificar do token.
    /// </summary>
    public class JwtTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string? _issuer;
        private readonly string? _audience;

        public JwtTicketDataFormat(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Configuração \"Jwt:Key\" ausente.");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _issuer = jwtSection["Issuer"];
            _audience = jwtSection["Audience"];
        }

        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();

            var expires = data.Properties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: data.Principal.Claims,
                expires: expires,
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            return handler.WriteToken(token);
        }

        public AuthenticationTicket? Unprotect(string? protectedText) => Unprotect(protectedText, null);

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            if (string.IsNullOrEmpty(protectedText))
                return null;

            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            try
            {
                var principal = handler.ValidateToken(protectedText, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                }, out var validatedToken);

                // O BearerTokenHandler do ASP.NET checa ticket.Properties.ExpiresUtc (não a claim
                // "exp" do JWT) para decidir se o token expirou — sem isso, ele rejeita todo token
                // como se estivesse sempre expirado, mesmo com uma assinatura/validade corretas.
                var properties = new AuthenticationProperties { ExpiresUtc = validatedToken.ValidTo };

                return new AuthenticationTicket(principal, properties, IdentityConstants.BearerScheme);
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }
    }
}
