using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class CourseCategory : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool Active { get; private set; } = true;
        
        public virtual ICollection<CourseCourseCategory> CourseCategories { get; set; } = new List<CourseCourseCategory>();

        private CourseCategory() { }

        public static CourseCategory Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
                
            return new CourseCategory
            {
                Name = name,
                Description = description,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string? description, bool active)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Name = name;
            Description = description;
            Active = active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            Active = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
