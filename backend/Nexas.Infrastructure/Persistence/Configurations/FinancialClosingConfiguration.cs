using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class FinancialClosingConfiguration : IEntityTypeConfiguration<FinancialClosing>
    {
        public void Configure(EntityTypeBuilder<FinancialClosing> builder)
        {
            builder.ToTable("FinancialClosings");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TeacherId).IsRequired();
            builder.Property(c => c.PeriodStart).IsRequired();
            builder.Property(c => c.PeriodEnd).IsRequired();
            
            builder.Property(c => c.GrossRevenue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(c => c.BankFeesTotal).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(c => c.NexasFeesTotal).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(c => c.NetValue).HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(c => c.TotalToReceive).HasColumnType("decimal(10,2)").IsRequired();

            builder.Property(c => c.Status).HasConversion<int>().IsRequired();
            builder.Property(c => c.PaymentDate);
            builder.Property(c => c.AsaasTransferId).HasMaxLength(100);
            builder.Property(c => c.PaymentReceiptUrl).HasMaxLength(2000);

            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UpdatedAt);

            builder.HasOne(c => c.Teacher)
                   .WithMany()
                   .HasForeignKey(c => c.TeacherId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
