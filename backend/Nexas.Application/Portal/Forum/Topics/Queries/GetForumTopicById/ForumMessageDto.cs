namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;

public class ForumMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
