using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokFinanceStatementConfiguration : IEntityTypeConfiguration<TikTokFinanceStatement>
{
    public void Configure(EntityTypeBuilder<TikTokFinanceStatement> builder)
    {
        builder.Property(s => s.TikTokStatementId).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Currency).IsRequired().HasMaxLength(10);
        builder.Property(s => s.StatementType).HasMaxLength(50);
        builder.Property(s => s.SaleAmount).HasColumnType("numeric(18,4)");
        builder.Property(s => s.TikTokFee).HasColumnType("numeric(18,4)");
        builder.Property(s => s.ShippingFee).HasColumnType("numeric(18,4)");
        builder.Property(s => s.PromotionAmount).HasColumnType("numeric(18,4)");
        builder.Property(s => s.AdjustmentAmount).HasColumnType("numeric(18,4)");
        builder.Property(s => s.SettlementAmount).HasColumnType("numeric(18,4)");
        builder.Property(s => s.RawData).HasColumnType("text");

        builder.HasOne(s => s.Connection)
               .WithMany()
               .HasForeignKey(s => s.ConnectionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.OrderFinances)
               .WithOne(f => f.Statement)
               .HasForeignKey(f => f.TikTokFinanceStatementId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.TenantId, s.TikTokStatementId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(s => new { s.TenantId, s.ConnectionId, s.StatementTime });
    }
}
