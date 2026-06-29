namespace Nexas.Application.Teachers.Common
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Position { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? InstagramLink { get; set; }
        public string? LinkedinLink { get; set; }
        public string? IdAgivys { get; set; }
    }
}
