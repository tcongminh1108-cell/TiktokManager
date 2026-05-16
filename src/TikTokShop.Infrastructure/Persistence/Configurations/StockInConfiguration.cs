using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class StockInConfiguration : IEntityTypeConfiguration<StockIn>
{
    public void Configure(EntityTypeBuilder<StockIn> builder)
    {
        builder.Property(s => s.Quantity).IsRequired();
        builder.Property(s => s.UnitPrice).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.TransactionDate).IsRequired();
        builder.Property(s => s.Note).HasColumnType("text");

        builder.HasIndex(s => new { s.TenantId, s.ProductId });
        builder.HasIndex(s => new { s.TenantId, s.SupplierId });
        builder.HasIndex(s => new { s.TenantId, s.TransactionDate });

        // FK from StockMovement.StockInId → StockIn.Id
        builder.HasMany<StockMovement>()
               .WithOne()
               .HasForeignKey(sm => sm.StockInId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
