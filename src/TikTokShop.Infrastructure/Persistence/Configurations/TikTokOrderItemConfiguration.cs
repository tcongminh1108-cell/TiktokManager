using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokOrderItemConfiguration : IEntityTypeConfiguration<TikTokOrderItem>
{
    public void Configure(EntityTypeBuilder<TikTokOrderItem> builder)
    {
        builder.Property(i => i.LineItemId).IsRequired().HasMaxLength(100);
        builder.Property(i => i.TikTokProductId).IsRequired().HasMaxLength(100);
        builder.Property(i => i.TikTokSkuId).IsRequired().HasMaxLength(100);
        builder.Property(i => i.SkuName).HasMaxLength(500);
        builder.Property(i => i.SalePrice).HasColumnType("numeric(18,4)");
        builder.Property(i => i.LastError).HasMaxLength(1000);
        builder.Property(i => i.ReservationKey).HasMaxLength(300);
        builder.Property(i => i.MovementKey).HasMaxLength(300);

        // LineItemId is globally unique per tenant (a line item belongs to exactly one order)
        builder.HasIndex(i => new { i.TenantId, i.LineItemId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        // Unresolved items dashboard query
        builder.HasIndex(i => new { i.TenantId, i.SyncStatus });

        // Look up items by TikTok SKU within a connection (used during mapping resolution)
        builder.HasIndex(i => new { i.TenantId, i.TikTokSkuId });

        builder.ToTable(t =>
            t.HasCheckConstraint("ck_tik_tok_order_items_quantity_positive", "quantity > 0"));
    }
}
