using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Teacher : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Position { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public string? InstagramLink { get; set; }
        public string? LinkedinLink { get; set; }
        public string? IdAgivys { get; set; }
        
        public bool Active { get; set; } = true;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();

        public static Teacher Create(string name, string? role, string? position, string? avatar, string? bio, string? instagramLink, string? linkedinLink, string? idAgivys, int? createdBy)
        {
            return new Teacher
            {
                Name = name,
                Role = role,
                Position = position,
                Avatar = avatar,
                Bio = bio,
                InstagramLink = instagramLink,
                LinkedinLink = linkedinLink,
                IdAgivys = idAgivys,
                CreatedBy = createdBy,
                Active = true
            };
        }
    }
}
