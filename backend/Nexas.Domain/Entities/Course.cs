using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        
        // Relations
        public virtual ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();

        public static Course Create(string title, string description)
        {
            return new Course { Title = title, Description = description };
        }
    }
}
