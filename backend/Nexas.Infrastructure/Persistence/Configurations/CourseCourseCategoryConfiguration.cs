using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class CourseCourseCategoryConfiguration : IEntityTypeConfiguration<CourseCourseCategory>
    {
        public void Configure(EntityTypeBuilder<CourseCourseCategory> builder)
        {
            builder.ToTable("CourseCourseCategories");

            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Course)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
