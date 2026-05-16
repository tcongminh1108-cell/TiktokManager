using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokOrderFinanceConfiguration : IEntityTypeConfiguration<TikTokOrderFinance>
{
    public void Configure(EntityTypeBuilder<TikTokOrderFinance> builder)
    {
        builder.Property(f => f.TikTokOrderId).IsRequired().HasMaxLength(100);
        builder.Property(f => f.Currency).HasMaxLength(10);
        builder.Property(f => f.SaleAmount).HasColumnType("numeric(18,4)");
        builder.Property(f => f.TikTokFee).HasColumnType("numeric(18,4)");
        builder.Property(f => f.ShippingFee).HasColumnType("numeric(18,4)");
        builder.Property(f => f.PromotionAmount).HasColumnType("numeric(18,4)");
        builder.Property(f => f.AdjustmentAmount).HasColumnType("numeric(18,4)");
        builder.Property(f => f.NetRevenue).HasColumnType("numeric(18,4)");
        builder.Property(f => f.RawData).HasColumnType("text");

        builder.HasOne(f => f.Connection)
               .WithMany()
               .HasForeignKey(f => f.ConnectionId)
               .OnDelete(DeleteBehavior.Restrict);

        // Statement FK is configured via TikTokFinanceStatementConfiguration (HasMany/WithOne)

        builder.HasIndex(f => new { f.TenantId, f.TikTokOrderId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(f => new { f.TenantId, f.TikTokFinanceStatementId });
        builder.HasIndex(f => new { f.TenantId, f.ConnectionId });
    }
}
