namespace Nexas.Application.Teachers.Queries.GetTeacherDashboard;

public class DashboardForumDto
{
    public int ForumId { get; set; }
    public int TopicId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string LessonName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}
