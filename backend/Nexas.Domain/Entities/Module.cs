using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Module : BaseEntity
    {
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DescriptionSub { get; set; }
        public string? ImgCoverLink { get; set; }
        public string? BunnyCollectionId { get; set; }
        public bool Active { get; set; } = true;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // Relations
        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public static Module Create(string name, string? description, string? descriptionSub, string? imgCoverLink, string? bunnyCollectionId, int? createdBy)
        {
            return new Module
            {
                Name = name,
                Description = description,
                DescriptionSub = descriptionSub,
                ImgCoverLink = imgCoverLink,
                BunnyCollectionId = bunnyCollectionId,
                CreatedBy = createdBy,
                Active = true
            };
        }
    }
}
