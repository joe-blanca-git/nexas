using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class CourseRate : BaseEntity
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int Rate { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public static CourseRate Create(int courseId, int userId, int rate)
        {
            return new CourseRate
            {
                CourseId = courseId,
                UserId = userId,
                Rate = rate,
                RatedAt = DateTime.UtcNow
            };
        }

        public void UpdateRate(int rate)
        {
            Rate = rate;
            RatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
