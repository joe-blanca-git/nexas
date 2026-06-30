using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class BlogPost : BaseEntity
{
    public int AuthorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Tags { get; private set; }
    public string? HeaderImageUrl { get; private set; }

    private BlogPost() { }

    public static BlogPost Create(int authorId, string title, string subject, string content, string? tags, string? headerImageUrl)
    {
        return new BlogPost
        {
            AuthorId = authorId,
            Title = title,
            Subject = subject,
            Content = content,
            Tags = tags,
            HeaderImageUrl = headerImageUrl
        };
    }

    public void Update(string title, string subject, string content, string? tags, string? headerImageUrl)
    {
        Title = title;
        Subject = subject;
        Content = content;
        Tags = tags;
        HeaderImageUrl = headerImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
