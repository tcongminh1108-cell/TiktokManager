using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class ProductTikTokMappingConfiguration : IEntityTypeConfiguration<ProductTikTokMapping>
{
    public void Configure(EntityTypeBuilder<ProductTikTokMapping> builder)
    {
        builder.Property(m => m.TikTokProductId).IsRequired().HasMaxLength(100);
        builder.Property(m => m.TikTokSkuId).IsRequired().HasMaxLength(100);
        builder.Property(m => m.TikTokSkuName).IsRequired().HasMaxLength(300);

        // One TikTok SKU can only be mapped once per connection (per tenant enforced by global filter).
        builder.HasIndex(m => new { m.TenantId, m.ConnectionId, m.TikTokProductId, m.TikTokSkuId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        // Fast lookup by SKU id when processing orders (order item → find mapped product).
        builder.HasIndex(m => new { m.TenantId, m.ConnectionId, m.TikTokSkuId });

        // Lookup by product when pushing inventory.
        builder.HasIndex(m => new { m.TenantId, m.ProductId });
    }
}
