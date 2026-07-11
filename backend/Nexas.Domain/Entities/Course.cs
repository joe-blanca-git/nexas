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
        public int WorkloadHours { get; set; }

        // Relations
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<CourseDomain> Domains { get; set; } = new List<CourseDomain>();
        public virtual ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public virtual ICollection<CourseCourseCategory> CourseCategories { get; set; } = new List<CourseCourseCategory>();
        public virtual ICollection<CourseRate> CourseRates { get; set; } = new List<CourseRate>();
        public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        public static Course Create(string name, string? description, string? descriptionSub, string? level, decimal? priceSingle, string? imgCoverLink, string? bunnyLibraryId, int workloadHours, int? createdBy)
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
                WorkloadHours = workloadHours,
                CreatedBy = createdBy,
                Active = true
            };
        }
    }
}
