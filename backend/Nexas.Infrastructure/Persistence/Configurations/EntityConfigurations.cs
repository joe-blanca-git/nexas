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
            builder.Property(u => u.Id).HasColumnName("UserId");
            
            builder.Property(u => u.ExternalId)
                .HasColumnName("ExternalId")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(u => u.ExternalId).IsUnique();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();
            
            builder.Property(u => u.UpdatedAt)
                .HasColumnName("UpdatedAt");
        }
    }

    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("CourseId");

            builder.Property(c => c.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(c => c.Description).HasColumnName("Description");
            builder.Property(c => c.DescriptionSub).HasColumnName("DescriptionSub");
            builder.Property(c => c.Level).HasColumnName("Level").HasMaxLength(50);
            builder.Property(c => c.PriceSingle).HasColumnName("PriceSingle").HasColumnType("decimal(10,2)");
            builder.Property(c => c.ImgCoverLink).HasColumnName("ImgCoverLink").HasMaxLength(2000);
            builder.Property(c => c.Active).HasColumnName("Active");
            builder.Property(c => c.CreatedAt).HasColumnName("CreatedAt");
            builder.Property(c => c.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(c => c.UpdatedAt).HasColumnName("UpdatedAt");
            builder.Property(c => c.UpdatedBy).HasColumnName("UpdatedBy");
            builder.Property(c => c.BunnyLibraryId).HasColumnName("BunnyLibraryId").HasMaxLength(100);
        }
    }

    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Modules");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasColumnName("ModuleId");

            builder.Property(m => m.CourseId).HasColumnName("CourseId");
            builder.Property(m => m.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(m => m.Description).HasColumnName("Description");
            builder.Property(m => m.DescriptionSub).HasColumnName("DescriptionSub");
            builder.Property(m => m.ImgCoverLink).HasColumnName("ImgCoverLink").HasMaxLength(2000);
            builder.Property(m => m.BunnyCollectionId).HasColumnName("BunnyCollectionId").HasMaxLength(100);
            builder.Property(m => m.Active).HasColumnName("Active");
            builder.Property(m => m.CreatedAt).HasColumnName("CreatedAt");
            builder.Property(m => m.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(m => m.UpdatedAt).HasColumnName("UpdatedAt");
            builder.Property(m => m.UpdatedBy).HasColumnName("UpdatedBy");

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
            builder.Property(l => l.Id).HasColumnName("LessonId");

            builder.Property(l => l.ModuleId).HasColumnName("ModuleId");
            builder.Property(l => l.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(l => l.Description).HasColumnName("Description");
            builder.Property(l => l.DurationSeconds).HasColumnName("DurationSeconds");
            builder.Property(l => l.Active).HasColumnName("Active");
            builder.Property(l => l.CreatedAt).HasColumnName("CreatedAt");
            builder.Property(l => l.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(l => l.UpdatedAt).HasColumnName("UpdatedAt");
            builder.Property(l => l.UpdatedBy).HasColumnName("UpdatedBy");
            builder.Property(l => l.BunnyVideoId).HasColumnName("BunnyVideoId").HasMaxLength(100);

            builder.HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId);
        }
    }

}
