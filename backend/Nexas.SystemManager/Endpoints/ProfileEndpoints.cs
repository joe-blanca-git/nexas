using Microsoft.AspNetCore.Identity;
using Nexas.Application.Common.Interfaces;
using Nexas.SystemManager.Domain.Entities;

namespace Nexas.SystemManager.Endpoints
{
    public record UpdateProfileRequest(string FullName, string? PhoneNumber, string? Avatar);

    public record ProfileResponse(string FullName, string Email, string? PhoneNumber, string? Avatar);

    public static class ProfileEndpoints
    {
        // O frontend já redimensiona/comprime a imagem para um quadrado pequeno (JPEG ~256px)
        // antes de enviar — isso dá bastante folga acima do que ele realmente manda, só para
        // impedir payloads absurdos caso alguém chame o endpoint direto sem passar pela UI.
        private const int MaxAvatarLength = 400_000;

        public static IEndpointRouteBuilder MapProfile(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/profile", async (
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                return Results.Ok(new ProfileResponse(user.FullName ?? string.Empty, user.Email ?? string.Empty, user.PhoneNumber, user.AvatarBase64));
            })
            .RequireAuthorization();

            endpoints.MapPut("/profile", async (
                UpdateProfileRequest request,
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.FullName))
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["FullName"] = new[] { "O nome completo é obrigatório." }
                    });

                if (!string.IsNullOrEmpty(request.Avatar))
                {
                    if (request.Avatar.Length > MaxAvatarLength)
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                            ["Avatar"] = new[] { "A imagem enviada é muito grande." }
                        });

                    if (!request.Avatar.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                            ["Avatar"] = new[] { "Formato de imagem inválido." }
                        });
                }

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                user.FullName = request.FullName.Trim();
                user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
                user.AvatarBase64 = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                    return Results.ValidationProblem(errors);
                }

                return Results.Ok(new ProfileResponse(user.FullName ?? string.Empty, user.Email ?? string.Empty, user.PhoneNumber, user.AvatarBase64));
            })
            .RequireAuthorization();

            return endpoints;
        }
    }
}
