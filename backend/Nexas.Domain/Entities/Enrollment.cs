using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
