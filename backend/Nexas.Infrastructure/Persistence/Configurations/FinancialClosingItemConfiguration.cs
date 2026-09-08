using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class FinancialClosingItemConfiguration : IEntityTypeConfiguration<FinancialClosingItem>
    {
        public void Configure(EntityTypeBuilder<FinancialClosingItem> builder)
        {
            builder.ToTable("FinancialClosingItems");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.FinancialClosingId).IsRequired();
            builder.Property(i => i.PurchaseId).IsRequired();
            
            builder.Property(i => i.AppliedTeacherPercentage).HasColumnType("decimal(5,4)").IsRequired();
            builder.Property(i => i.GrossValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(i => i.BankFeeValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(i => i.NexasFeeValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(i => i.CalculatedValue).HasColumnType("decimal(10,2)").IsRequired();

            builder.Property(i => i.CreatedAt).IsRequired();
            builder.Property(i => i.UpdatedAt);

            builder.HasOne(i => i.FinancialClosing)
                   .WithMany(c => c.Items)
                   .HasForeignKey(i => i.FinancialClosingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Purchase)
                   .WithMany()
                   .HasForeignKey(i => i.PurchaseId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
