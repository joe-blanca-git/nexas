using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class FinancialTaxConfiguration : IEntityTypeConfiguration<FinancialTax>
    {
        public void Configure(EntityTypeBuilder<FinancialTax> builder)
        {
            builder.ToTable("FinancialTaxes");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type).HasConversion<int>().IsRequired();
            builder.Property(t => t.Percentage).HasColumnType("decimal(18,4)").IsRequired(); // Store fixed values or percentages
            builder.Property(t => t.EffectiveFrom).IsRequired();
            builder.Property(t => t.IsActive).IsRequired();
            
            builder.Property(t => t.CreatedAt).IsRequired();
            builder.Property(t => t.UpdatedAt);
        }
    }
}
