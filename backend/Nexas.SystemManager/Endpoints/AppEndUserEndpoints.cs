using System.Security.Claims;
using System.Security.Cryptography;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Filters;
using Nexas.SystemManager.Infrastructure;
using Nexas.SystemManager.Services;

namespace Nexas.SystemManager.Endpoints
{
    public record AppRegisterRequest(string Email, string Password, string? FullName);

    public record AppLoginRequest(string Email, string Password);

    public record AppGoogleLoginRequest(string IdToken);

    public record AppForgotPasswordRequest(string Email);

    public record AppResetPasswordRequest(string Email, string Token, string NewPassword);

    public record AppEndUserResponse(Guid Id, string Email, string? FullName, DateTime CreatedAt, List<string> Roles);

    public record AppLoginResponse(string AccessToken, int ExpiresIn, AppEndUserResponse User);

    /// <summary>
    /// Endpoints de autenticação para os usuários FINAIS das Applications cadastradas na Nexas
    /// (o cliente da Application, não o dono dela). Autenticados via header "X-Api-Key" (a
    /// chave da Application, ver RequireApiKeyFilter) em vez do esquema Identity.Bearer do
    /// portal — são universos de usuário e de token completamente separados
    /// (ver AppEndUser e AppEndUserTokenService).
    /// </summary>
    public static class AppEndUserEndpoints
    {
        private static readonly PasswordHasher<AppEndUser> PasswordHasher = new();
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        public static IEndpointRouteBuilder MapAppAuth(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/v1/apps/auth")
                .WithTags("Application End-User Auth")
                .AddEndpointFilter<RequireApiKeyFilter>();

            group.MapPost("/register", async (AppRegisterRequest request, HttpContext http, SystemManagerDbContext db) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Email"] = new[] { "E-mail e senha são obrigatórios." }
                    });
                }

                var normalizedEmail = request.Email.Trim().ToUpperInvariant();
                var exists = await db.AppEndUsers.AnyAsync(u => u.ApplicationId == app.Id && u.NormalizedEmail == normalizedEmail);
                if (exists)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Email"] = new[] { "Já existe uma conta com esse e-mail nessa aplicação." }
                    });
                }

                var user = new AppEndUser
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = app.Id,
                    Email = request.Email.Trim(),
                    NormalizedEmail = normalizedEmail,
                    FullName = request.FullName,
                    CreatedAt = DateTime.UtcNow
                };
                user.PasswordHash = PasswordHasher.HashPassword(user, request.Password);

                db.AppEndUsers.Add(user);
                await db.SaveChangesAsync();

                return Results.Created(
                    $"/api/v1/apps/auth/{user.Id}",
                    new AppEndUserResponse(user.Id, user.Email, user.FullName, user.CreatedAt, new List<string>()));
            });

            group.MapPost("/login", async (AppLoginRequest request, HttpContext http, SystemManagerDbContext db, AppEndUserTokenService tokenService) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;
                var normalizedEmail = request.Email?.Trim().ToUpperInvariant() ?? string.Empty;

                var user = await db.AppEndUsers
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.AppRole)
                    .FirstOrDefaultAsync(u => u.ApplicationId == app.Id && u.NormalizedEmail == normalizedEmail);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var verification = PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verification == PasswordVerificationResult.Failed)
                {
                    return Results.Unauthorized();
                }

                var roleNames = user.UserRoles.Select(ur => ur.AppRole.Name).ToList();
                var token = tokenService.GenerateToken(user, TokenLifetime, roleNames);

                return Results.Ok(new AppLoginResponse(
                    token,
                    (int)TokenLifetime.TotalSeconds,
                    new AppEndUserResponse(user.Id, user.Email, user.FullName, user.CreatedAt, roleNames)));
            });

            group.MapPost("/google-login", async (AppGoogleLoginRequest request, HttpContext http, SystemManagerDbContext db, AppEndUserTokenService tokenService) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;

                if (string.IsNullOrWhiteSpace(request.IdToken))
                {
                    return Results.BadRequest(new { error = "O ID Token é obrigatório." });
                }

                if (string.IsNullOrWhiteSpace(app.GoogleClientId))
                {
                    return Results.BadRequest(new { error = "Login com Google não está configurado para esta aplicação." });
                }

                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { app.GoogleClientId }
                    });
                }
                catch (Exception ex) when (ex is InvalidJwtException or FormatException)
                {
                    // FormatException cobre tokens que nem chegam a ter formato de JWT (ex.: string
                    // qualquer enviada por engano) — GoogleJsonWebSignature só lança InvalidJwtException
                    // para JWTs bem-formados mas com assinatura/audience/expiração inválidas.
                    return Results.BadRequest(new { error = "Token do Google inválido." });
                }

                var normalizedEmail = payload.Email.Trim().ToUpperInvariant();
                var user = await db.AppEndUsers
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.AppRole)
                    .FirstOrDefaultAsync(u => u.ApplicationId == app.Id && u.NormalizedEmail == normalizedEmail);

                if (user == null)
                {
                    user = new AppEndUser
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = app.Id,
                        Email = payload.Email,
                        NormalizedEmail = normalizedEmail,
                        FullName = payload.Name,
                        CreatedAt = DateTime.UtcNow
                    };
                    // Conta criada via Google não usa senha própria — gera uma aleatória e
                    // inutilizável só para satisfazer o campo obrigatório PasswordHash.
                    user.PasswordHash = PasswordHasher.HashPassword(user, Guid.NewGuid().ToString() + "Aa1!");

                    db.AppEndUsers.Add(user);
                    await db.SaveChangesAsync();
                }

                var roleNames = user.UserRoles.Select(ur => ur.AppRole.Name).ToList();
                var token = tokenService.GenerateToken(user, TokenLifetime, roleNames);

                return Results.Ok(new AppLoginResponse(
                    token,
                    (int)TokenLifetime.TotalSeconds,
                    new AppEndUserResponse(user.Id, user.Email, user.FullName, user.CreatedAt, roleNames)));
            });

            group.MapPost("/forgot-password", async (AppForgotPasswordRequest request, HttpContext http, SystemManagerDbContext db) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;
                var normalizedEmail = request.Email?.Trim().ToUpperInvariant() ?? string.Empty;

                var user = await db.AppEndUsers.FirstOrDefaultAsync(u => u.ApplicationId == app.Id && u.NormalizedEmail == normalizedEmail);
                if (user != null)
                {
                    user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
                    user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
                    await db.SaveChangesAsync();

                    // TODO: enviar e-mail de verdade com o token quando o Resend for integrado
                    // (mesma lacuna documentada para o /forgotPassword do portal em known-issues.md).
                }

                // Sempre 200, exista ou não o e-mail — evita revelar quais contas existem.
                return Results.Ok();
            });

            group.MapPost("/reset-password", async (AppResetPasswordRequest request, HttpContext http, SystemManagerDbContext db) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;
                var normalizedEmail = request.Email?.Trim().ToUpperInvariant() ?? string.Empty;

                var user = await db.AppEndUsers.FirstOrDefaultAsync(u => u.ApplicationId == app.Id && u.NormalizedEmail == normalizedEmail);
                var tokenValid = user?.PasswordResetToken != null
                    && user.PasswordResetToken == request.Token
                    && user.PasswordResetTokenExpiresAt.HasValue
                    && user.PasswordResetTokenExpiresAt.Value >= DateTime.UtcNow;

                if (!tokenValid)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["Token"] = new[] { "Token inválido ou expirado." }
                    });
                }

                user!.PasswordHash = PasswordHasher.HashPassword(user, request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiresAt = null;
                await db.SaveChangesAsync();

                return Results.Ok();
            });

            group.MapGet("/me", async (HttpContext http, SystemManagerDbContext db, AppEndUserTokenService tokenService) =>
            {
                var app = (SystemApp)http.Items["CurrentApp"]!;

                var authHeader = http.Request.Headers.Authorization.ToString();
                if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Unauthorized();
                }

                var token = authHeader["Bearer ".Length..].Trim();
                var principal = tokenService.ValidateToken(token);
                if (principal == null)
                {
                    return Results.Unauthorized();
                }

                var tokenAppId = principal.FindFirst(AppEndUserTokenService.AppIdClaim)?.Value;
                if (tokenAppId != app.Id.ToString())
                {
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var user = await db.AppEndUsers
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.AppRole)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var roleNames = user.UserRoles.Select(ur => ur.AppRole.Name).ToList();
                return Results.Ok(new AppEndUserResponse(user.Id, user.Email, user.FullName, user.CreatedAt, roleNames));
            });

            return endpoints;
        }
    }
}
