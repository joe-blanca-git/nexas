using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.ToTable("TicketAttachments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(300);
        builder.Property(e => e.StoredFileName).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Bucket).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Path).IsRequired().HasMaxLength(500);
        builder.Property(e => e.MimeType).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Size).IsRequired();

        builder.HasOne(e => e.Message)
            .WithMany(m => m.Attachments)
            .HasForeignKey(e => e.TicketMessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
