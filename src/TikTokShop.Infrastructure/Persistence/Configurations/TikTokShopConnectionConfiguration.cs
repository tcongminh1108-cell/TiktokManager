using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokShopConnectionConfiguration : IEntityTypeConfiguration<TikTokShopConnection>
{
    public void Configure(EntityTypeBuilder<TikTokShopConnection> builder)
    {
        builder.Property(c => c.ShopId).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ShopName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ShopCipher).IsRequired().HasColumnType("text");
        builder.Property(c => c.Region).IsRequired().HasMaxLength(20);
        builder.Property(c => c.BaseApiUrl).IsRequired().HasMaxLength(500);
        builder.Property(c => c.AccessToken).IsRequired().HasColumnType("text");
        builder.Property(c => c.RefreshToken).IsRequired().HasColumnType("text");

        // One tenant cannot connect the same TikTok shop twice.
        builder.HasIndex(c => new { c.TenantId, c.ShopId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        // Quick lookup by tenant + status for the token-refresh background job.
        builder.HasIndex(c => new { c.TenantId, c.Status });
    }
}
