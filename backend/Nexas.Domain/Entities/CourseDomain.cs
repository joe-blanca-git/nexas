using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class CourseDomain : BaseEntity
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual Course Course { get; set; } = null!;

        public static CourseDomain Create(int courseId, string title, string? description)
        {
            return new CourseDomain
            {
                CourseId = courseId,
                Title = title,
                Description = description
            };
        }
    }
}
