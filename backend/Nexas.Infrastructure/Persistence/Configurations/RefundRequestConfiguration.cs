using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
    {
        public void Configure(EntityTypeBuilder<RefundRequest> builder)
        {
            builder.ToTable("RefundRequests");
            
            builder.HasKey(r => r.Id);

            builder.Property(r => r.RequestCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(r => r.Reason)
                .HasMaxLength(1000);

            builder.Property(r => r.SupportTicketCode)
                .HasMaxLength(100);

            builder.HasOne(r => r.Purchase)
                .WithMany()
                .HasForeignKey(r => r.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Property(t => t.CreatedAt).IsRequired();
            builder.Property(t => t.UpdatedAt);
        }
    }
}
