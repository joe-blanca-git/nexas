using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasColumnName("user_id");
            
            builder.Property(u => u.ExternalId)
                .HasColumnName("external_id")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(u => u.ExternalId).IsUnique();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }

    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("course_id");

            builder.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            builder.Property(c => c.Description).HasColumnName("description");
            builder.Property(c => c.DescriptionSub).HasColumnName("description_sub");
            builder.Property(c => c.Level).HasColumnName("level").HasMaxLength(50);
            builder.Property(c => c.PriceSingle).HasColumnName("price_single").HasColumnType("decimal(10,2)");
            builder.Property(c => c.ImgCoverLink).HasColumnName("img_cover_link").HasMaxLength(2000);
            builder.Property(c => c.Active).HasColumnName("active");
            builder.Property(c => c.CreatedAt).HasColumnName("created_at");
            builder.Property(c => c.CreatedBy).HasColumnName("created_by");
            builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            builder.Property(c => c.UpdatedBy).HasColumnName("updated_by");
            builder.Property(c => c.BunnyLibraryId).HasColumnName("bunny_library_id").HasMaxLength(100);
        }
    }

    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Modules");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasColumnName("module_id");

            builder.Property(m => m.CourseId).HasColumnName("course_id");
            builder.Property(m => m.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            builder.Property(m => m.Description).HasColumnName("description");
            builder.Property(m => m.DescriptionSub).HasColumnName("description_sub");
            builder.Property(m => m.ImgCoverLink).HasColumnName("img_cover_link").HasMaxLength(2000);
            builder.Property(m => m.BunnyCollectionId).HasColumnName("bunny_collection_id").HasMaxLength(100);
            builder.Property(m => m.Active).HasColumnName("active");
            builder.Property(m => m.CreatedAt).HasColumnName("created_at");
            builder.Property(m => m.CreatedBy).HasColumnName("created_by");
            builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            builder.Property(m => m.UpdatedBy).HasColumnName("updated_by");

            builder.HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId);
        }
    }

    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.ToTable("Lessons");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).HasColumnName("lesson_id");

            builder.Property(l => l.ModuleId).HasColumnName("module_id");
            builder.Property(l => l.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            builder.Property(l => l.Description).HasColumnName("description");
            builder.Property(l => l.DurationSeconds).HasColumnName("duration_seconds");
            builder.Property(l => l.Active).HasColumnName("active");
            builder.Property(l => l.CreatedAt).HasColumnName("created_at");
            builder.Property(l => l.CreatedBy).HasColumnName("created_by");
            builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");
            builder.Property(l => l.UpdatedBy).HasColumnName("updated_by");
            builder.Property(l => l.BunnyVideoId).HasColumnName("bunny_video_id").HasMaxLength(100);

            builder.HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId);
        }
    }

}
