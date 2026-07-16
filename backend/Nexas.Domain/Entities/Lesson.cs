using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities
{
    public class Lesson : BaseEntity
    {
        public int ModuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DurationSeconds { get; set; }
        public bool Active { get; set; } = true;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string? BunnyVideoId { get; set; }

        public LessonStatus Status { get; set; } = LessonStatus.Draft;
        public string? Thumbnail { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Relations
        public virtual Module Module { get; set; } = null!;
        public virtual ICollection<LessonView> LessonViews { get; set; } = new List<LessonView>();

        public static Lesson Create(string name, string? description, int? durationSeconds, int? createdBy)
        {
            return new Lesson
            {
                Name = name,
                Description = description,
                DurationSeconds = durationSeconds,
                CreatedBy = createdBy,
                Active = true,
                Status = LessonStatus.Draft
            };
        }
    }
}
