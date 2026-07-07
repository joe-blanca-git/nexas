using Nexas.Domain.Enums;

namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopics;

public class ForumTopicSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int RepliesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
