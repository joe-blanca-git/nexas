using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class ForumMessageConfiguration : IEntityTypeConfiguration<ForumMessage>
{
    public void Configure(EntityTypeBuilder<ForumMessage> builder)
    {
        builder.ToTable("ForumMessages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnType("longtext");

        builder.HasOne(e => e.Topic)
            .WithMany(t => t.Messages)
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TopicId);
        builder.HasIndex(e => e.AuthorId);
    }
}
