using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class ForumCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public bool Active { get; private set; } = true;

    // Navigation
    public virtual ICollection<ForumTopic> Topics { get; private set; } = new List<ForumTopic>();

    private ForumCategory() { } // EF Core

    public static ForumCategory Create(string name, string? description, string? icon = null)
    {
        return new ForumCategory
        {
            Name = name,
            Description = description,
            Icon = icon,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, string? icon = null)
    {
        Name = name;
        Description = description;
        Icon = icon;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
