using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class ForumCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool Active { get; private set; } = true;

    // Navigation
    public virtual ICollection<ForumTopic> Topics { get; private set; } = new List<ForumTopic>();

    private ForumCategory() { } // EF Core

    public static ForumCategory Create(string name, string? description)
    {
        return new ForumCategory
        {
            Name = name,
            Description = description,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
