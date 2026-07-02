using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Nexas.Api.Services;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirst("nameid")?.Value ??
               user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               user.FindFirst("sub")?.Value ??
               user.FindFirst("id")?.Value ??
               user.FindFirst("external_id")?.Value;
    }
}
