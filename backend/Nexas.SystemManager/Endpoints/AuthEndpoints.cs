using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Infrastructure;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nexas.SystemManager.Endpoints
{
    public record GoogleLoginRequest(string IdToken);

    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapCustomRegister(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/register", async (RegisterUserRequest request, UserManager<NexasUser> userManager) =>
            {
                var validationErrors = new Dictionary<string, string[]>();
                if (string.IsNullOrWhiteSpace(request.Nome)) validationErrors["Nome"] = new[] { "O nome é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Email)) validationErrors["Email"] = new[] { "O e-mail é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Telefone)) validationErrors["Telefone"] = new[] { "O telefone é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Senha)) validationErrors["Senha"] = new[] { "A senha é obrigatória." };

                if (validationErrors.Count > 0)
                {
                    return Results.ValidationProblem(validationErrors);
                }

                var user = new NexasUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.Nome,
                    PhoneNumber = request.Telefone
                };

                var result = await userManager.CreateAsync(user, request.Senha);
                if (!result.Succeeded)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    return Results.ValidationProblem(errors);
                }

                var isFirstUser = userManager.Users.Count() == 1;
                var rolesToAssign = isFirstUser ? new[] { "Admin", "Dev" } : new[] { "Owner" };
                await userManager.AddToRolesAsync(user, rolesToAssign);

                return Results.Ok();
            })
            .AllowAnonymous()
            .Add(endpointBuilder =>
            {
                if (endpointBuilder is RouteEndpointBuilder routeEndpointBuilder)
                {
                    routeEndpointBuilder.Order = -1;
                }
            });

            endpoints.MapPost("/google-login", async (
                GoogleLoginRequest request,
                UserManager<NexasUser> userManager,
                SignInManager<NexasUser> signInManager,
                SystemManagerDbContext dbContext,
                IConfiguration configuration,
                HttpContext context) =>
            {
                if (string.IsNullOrWhiteSpace(request.IdToken))
                    return Results.BadRequest(new { error = "O ID Token é obrigatório." });

                GoogleJsonWebSignature.Payload payload;
                try
                {
                    var clientId = configuration["GoogleClientId"];
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
                }
                catch (InvalidJwtException)
                {
                    return Results.BadRequest(new { error = "Token do Google inválido." });
                }

                var email = payload.Email;
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // Create user silently
                    user = new NexasUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = payload.Name,
                        PhoneNumber = "" // Google might not provide phone number easily
                    };
                    
                    var password = Guid.NewGuid().ToString() + "Aa1!"; // Random password
                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded) return Results.BadRequest(new { error = "Erro ao criar conta vinculada ao Google." });

                    var isFirstUser = userManager.Users.Count() == 1;
                    var rolesToAssign = isFirstUser ? new[] { "Admin", "Dev" } : new[] { "Owner" };
                    await userManager.AddToRolesAsync(user, rolesToAssign);
                }

                // Register login history
                dbContext.LoginHistories.Add(new UserLoginHistory
                {
                    UserId = user.Id,
                    Method = "Google",
                    LoginDate = DateTime.UtcNow
                });
                await dbContext.SaveChangesAsync();

                // Generate Opaque Bearer Token
                var principal = await signInManager.CreateUserPrincipalAsync(user);
                var bearerOptions = context.RequestServices.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>().Get(IdentityConstants.BearerScheme);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), IdentityConstants.BearerScheme);
                var token = bearerOptions.BearerTokenProtector.Protect(ticket);

                // Return both accessToken and token for frontend compatibility
                return Results.Ok(new
                {
                    tokenType = "Bearer",
                    accessToken = token,
                    token = token,
                    expiresIn = 3600 // We don't have the exact logic, 1h is standard
                });
            })
            .AllowAnonymous();

            return endpoints;
        }
    }
}
