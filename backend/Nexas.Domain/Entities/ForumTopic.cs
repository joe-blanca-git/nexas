using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class ForumTopic : BaseEntity
{
    public int CategoryId { get; private set; }
    public int? LessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int AuthorId { get; private set; }
    public ForumTopicStatus Status { get; private set; }

    // Navigation
    public virtual ForumCategory Category { get; private set; } = null!;
    public virtual Lesson? Lesson { get; private set; }
    public virtual User Author { get; private set; } = null!;
    public virtual ICollection<ForumMessage> Messages { get; private set; } = new List<ForumMessage>();

    private ForumTopic() { } // EF Core

    public static ForumTopic Create(int categoryId, int? lessonId, string title, string subject, string content, int authorId)
    {
        return new ForumTopic
        {
            CategoryId = categoryId,
            LessonId = lessonId,
            Title = title,
            Subject = subject,
            Content = content,
            AuthorId = authorId,
            Status = ForumTopicStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Resolve()
    {
        Status = ForumTopicStatus.Resolved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = ForumTopicStatus.Open;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string content)
    {
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
