using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nexas.SystemManager.Domain.Entities;

namespace Nexas.SystemManager.Services
{
    /// <summary>
    /// Gera e valida os tokens dos usuários finais das Applications (AppEndUser) — um universo
    /// de tokens completamente separado dos tokens do portal (Identity.Bearer/JwtTicketDataFormat).
    /// Só o próprio Nexas.SystemManager decodifica esses tokens (via GET /api/v1/apps/auth/me);
    /// a Application cliente nunca precisa validar o JWT diretamente, então essa chave nunca
    /// precisa ser compartilhada com terceiros. Reaproveita a mesma seção "Jwt" de configuração
    /// do portal por simplicidade — a claim "token_type" abaixo é o que impede um token de
    /// usuário final ser aceito como se fosse um token de admin do portal (ou vice-versa).
    /// </summary>
    public class AppEndUserTokenService
    {
        private const string TokenTypeClaim = "token_type";
        private const string TokenTypeValue = "app_end_user";
        public const string AppIdClaim = "app_id";

        private readonly SymmetricSecurityKey _signingKey;
        private readonly string? _issuer;
        private readonly string? _audience;

        public AppEndUserTokenService(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Configuração \"Jwt:Key\" ausente.");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _issuer = jwtSection["Issuer"];
            _audience = jwtSection["Audience"];
        }

        public string GenerateToken(AppEndUser user, TimeSpan lifetime, IEnumerable<string>? roles = null)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.OutboundClaimTypeMap.Clear();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(AppIdClaim, user.ApplicationId.ToString()),
                new(TokenTypeClaim, TokenTypeValue)
            };

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            }

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(lifetime),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            return handler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey
                }, out _);

                var tokenType = principal.FindFirst(TokenTypeClaim)?.Value;
                return tokenType == TokenTypeValue ? principal : null;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }
    }
}
