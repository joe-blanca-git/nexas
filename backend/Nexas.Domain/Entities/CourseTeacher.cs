namespace Nexas.Domain.Entities
{
    public class CourseTeacher
    {
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
    }
}
