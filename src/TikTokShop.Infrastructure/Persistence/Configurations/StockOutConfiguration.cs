using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class StockOutConfiguration : IEntityTypeConfiguration<StockOut>
{
    public void Configure(EntityTypeBuilder<StockOut> builder)
    {
        builder.Property(s => s.CustomerName).HasMaxLength(200);
        builder.Property(s => s.Quantity).IsRequired();
        builder.Property(s => s.UnitPrice).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(s => s.TransactionDate).IsRequired();
        builder.Property(s => s.Note).HasColumnType("text");

        builder.HasIndex(s => new { s.TenantId, s.ProductId });
        builder.HasIndex(s => new { s.TenantId, s.TransactionDate });

        // FK from StockMovement.StockOutId → StockOut.Id
        builder.HasMany<StockMovement>()
               .WithOne()
               .HasForeignKey(sm => sm.StockOutId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
