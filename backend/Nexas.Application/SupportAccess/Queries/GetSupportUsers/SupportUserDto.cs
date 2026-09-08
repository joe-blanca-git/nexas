namespace Nexas.Application.SupportAccess.Queries.GetSupportUsers;

public class SupportUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EnrolledCoursesCount { get; set; }
}
