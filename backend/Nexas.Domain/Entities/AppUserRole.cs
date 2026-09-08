using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class AppUserRole : BaseEntity
{
    public string UserId { get; private set; } = null!;
    public string RoleId { get; private set; } = null!;

    private AppUserRole() { }

    public static AppUserRole Create(string userId, string roleId)
    {
        return new AppUserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
