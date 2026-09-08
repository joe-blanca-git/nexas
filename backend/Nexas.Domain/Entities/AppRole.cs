using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class AppRole : BaseEntity
{
    public string RoleId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? NormalizedName { get; private set; }

    private AppRole() { }

    public static AppRole Create(string roleId, string name, string? normalizedName = null)
    {
        return new AppRole
        {
            RoleId = roleId,
            Name = name,
            NormalizedName = normalizedName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
