using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokReturnLineConfiguration : IEntityTypeConfiguration<TikTokReturnLine>
{
    public void Configure(EntityTypeBuilder<TikTokReturnLine> builder)
    {
        builder.Property(l => l.LineItemId).IsRequired().HasMaxLength(100);
        builder.Property(l => l.OriginalOrderItemId).HasMaxLength(100);
        builder.Property(l => l.TikTokSkuId).IsRequired().HasMaxLength(100);
        builder.Property(l => l.SkuName).HasMaxLength(500);
        builder.Property(l => l.RefundAmount).HasColumnType("numeric(18,4)");
        builder.Property(l => l.LastError).HasMaxLength(1000);
        builder.Property(l => l.MovementKey).HasMaxLength(300);

        builder.HasIndex(l => new { l.TenantId, l.LineItemId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(l => new { l.TenantId, l.SyncStatus });

        builder.ToTable(t =>
            t.HasCheckConstraint("ck_tik_tok_return_lines_quantity_positive", "quantity > 0"));
    }
}
