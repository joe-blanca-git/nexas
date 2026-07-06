using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class ForumMessage : BaseEntity
{
    public int TopicId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int AuthorId { get; private set; }

    // Navigation
    public virtual ForumTopic Topic { get; private set; } = null!;
    public virtual User Author { get; private set; } = null!;

    private ForumMessage() { } // EF Core

    public static ForumMessage Create(int topicId, string content, int authorId)
    {
        return new ForumMessage
        {
            TopicId = topicId,
            Content = content,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
