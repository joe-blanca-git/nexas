using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Infrastructure;

namespace Nexas.SystemManager.Endpoints
{
    public record PrivacyPreferencesResponse(bool ReceiveMarketingEmails, bool ReceiveProductNotifications);

    public record UpdatePrivacyPreferencesRequest(bool ReceiveMarketingEmails, bool ReceiveProductNotifications);

    public record ExportedLoginHistoryItem(string Method, DateTime LoginDate);

    public record UserDataExport(
        string Id,
        string FullName,
        string Email,
        string? PhoneNumber,
        bool EmailConfirmed,
        IEnumerable<string> Roles,
        bool ReceiveMarketingEmails,
        bool ReceiveProductNotifications,
        string? AvatarBase64,
        IEnumerable<ExportedLoginHistoryItem> LoginHistory);

    public static class PrivacyEndpoints
    {
        public static IEndpointRouteBuilder MapPrivacy(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/privacy", async (
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                return Results.Ok(new PrivacyPreferencesResponse(user.ReceiveMarketingEmails, user.ReceiveProductNotifications));
            })
            .RequireAuthorization();

            endpoints.MapPut("/privacy", async (
                UpdatePrivacyPreferencesRequest request,
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                user.ReceiveMarketingEmails = request.ReceiveMarketingEmails;
                user.ReceiveProductNotifications = request.ReceiveProductNotifications;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                    return Results.ValidationProblem(errors);
                }

                return Results.Ok(new PrivacyPreferencesResponse(user.ReceiveMarketingEmails, user.ReceiveProductNotifications));
            })
            .RequireAuthorization();

            endpoints.MapGet("/export-data", async (
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager,
                SystemManagerDbContext dbContext) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                var roles = await userManager.GetRolesAsync(user);
                var history = await dbContext.LoginHistories
                    .Where(h => h.UserId == user.Id)
                    .OrderByDescending(h => h.LoginDate)
                    .Select(h => new ExportedLoginHistoryItem(h.Method, h.LoginDate))
                    .ToListAsync();

                var export = new UserDataExport(
                    user.Id,
                    user.FullName ?? string.Empty,
                    user.Email ?? string.Empty,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    roles,
                    user.ReceiveMarketingEmails,
                    user.ReceiveProductNotifications,
                    user.AvatarBase64,
                    history);

                return Results.Ok(export);
            })
            .RequireAuthorization();

            endpoints.MapDelete("/account", async (
                string confirmEmail,
                ICurrentUserService currentUser,
                UserManager<NexasUser> userManager) =>
            {
                if (string.IsNullOrEmpty(currentUser.ExternalId))
                    return Results.Unauthorized();

                var user = await userManager.FindByIdAsync(currentUser.ExternalId);
                if (user == null) return Results.Unauthorized();

                if (!string.Equals(confirmEmail?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["ConfirmEmail"] = new[] { "O e-mail informado não confere com a sua conta." }
                    });

                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
                    return Results.ValidationProblem(errors);
                }

                return Results.NoContent();
            })
            .RequireAuthorization();

            return endpoints;
        }
    }
}
