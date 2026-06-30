using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class CourseCourseCategory : BaseEntity
    {
        public int CourseId { get; private set; }
        public int CategoryId { get; private set; }

        public virtual Course Course { get; private set; } = null!;
        public virtual CourseCategory Category { get; private set; } = null!;

        private CourseCourseCategory() { }

        public static CourseCourseCategory Create(int courseId, int categoryId)
        {
            if (courseId <= 0) throw new ArgumentException("Invalid courseId", nameof(courseId));
            if (categoryId <= 0) throw new ArgumentException("Invalid categoryId", nameof(categoryId));

            return new CourseCourseCategory
            {
                CourseId = courseId,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
