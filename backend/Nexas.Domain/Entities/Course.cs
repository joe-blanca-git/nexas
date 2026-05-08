using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DescriptionSub { get; set; }
        public string? Level { get; set; }
        public decimal? PriceSingle { get; set; }
        public string? ImgCoverLink { get; set; }
        public bool Active { get; set; } = true;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string? BunnyLibraryId { get; set; }

        // Relations
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public static Course Create(string name, string? description, string? descriptionSub, string? level, decimal? priceSingle, string? imgCoverLink, string? bunnyLibraryId, int? createdBy)
        {
            return new Course 
            { 
                Name = name, 
                Description = description,
                DescriptionSub = descriptionSub,
                Level = level,
                PriceSingle = priceSingle,
                ImgCoverLink = imgCoverLink,
                BunnyLibraryId = bunnyLibraryId,
                CreatedBy = createdBy,
                Active = true
            };
        }
    }
}
