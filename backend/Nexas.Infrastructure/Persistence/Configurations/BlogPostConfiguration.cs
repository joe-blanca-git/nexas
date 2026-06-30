using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("Id");

        builder.Property(b => b.AuthorId)
            .HasColumnName("AuthorId")
            .IsRequired();

        builder.Property(b => b.Title)
            .HasColumnName("Title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Subject)
            .HasColumnName("Subject")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Content)
            .HasColumnName("Content")
            .IsRequired()
            .HasColumnType("longtext");

        builder.Property(b => b.Tags)
            .HasColumnName("Tags")
            .HasMaxLength(500);

        builder.Property(b => b.HeaderImageUrl)
            .HasColumnName("HeaderImageUrl")
            .HasMaxLength(500);
            
        builder.HasOne<User>().WithMany().HasForeignKey(b => b.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}
