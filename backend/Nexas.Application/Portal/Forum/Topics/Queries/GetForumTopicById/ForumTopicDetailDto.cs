namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;

public class ForumTopicDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? LessonName { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<ForumMessageDto> Messages { get; set; } = new List<ForumMessageDto>();
    public bool IsOwn { get; set; }
}
