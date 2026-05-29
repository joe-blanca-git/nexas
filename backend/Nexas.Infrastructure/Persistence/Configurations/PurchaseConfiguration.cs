using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;
public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("PurchaseId");

        builder.Property(p => p.UserId)
            .HasColumnName("UserId");

        builder.Property(p => p.CourseId)
            .HasColumnName("CourseId");

        builder.Property(p => p.Amount)
            .HasColumnName("Amount")
            .HasPrecision(10, 2);

        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.PaymentMethod)
            .HasColumnName("PaymentMethod")
            .HasMaxLength(50);

        builder.Property(p => p.AsaasPaymentId)
            .HasColumnName("AsaasPaymentId")
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Ignore(p => p.UpdatedAt);

        // Relacionamentos
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Course)
            .WithMany()
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}