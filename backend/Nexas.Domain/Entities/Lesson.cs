using Nexas.Domain.Common;

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

        // Relations
        public virtual Module Module { get; set; } = null!;

        public static Lesson Create(string name, string? description, int? durationSeconds, string? bunnyVideoId, int? createdBy)
        {
            return new Lesson
            {
                Name = name,
                Description = description,
                DurationSeconds = durationSeconds,
                BunnyVideoId = bunnyVideoId,
                CreatedBy = createdBy,
                Active = true
            };
        }
    }
}
