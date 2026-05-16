using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokReturnConfiguration : IEntityTypeConfiguration<TikTokReturn>
{
    public void Configure(EntityTypeBuilder<TikTokReturn> builder)
    {
        builder.Property(r => r.TikTokReturnId).IsRequired().HasMaxLength(100);
        builder.Property(r => r.TikTokOrderId).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ReturnReason).HasMaxLength(500);
        builder.Property(r => r.RefundAmount).HasColumnType("numeric(18,4)");
        builder.Property(r => r.RawData).HasColumnType("text");

        builder.HasOne(r => r.Connection)
               .WithMany()
               .HasForeignKey(r => r.ConnectionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Lines)
               .WithOne(l => l.Return)
               .HasForeignKey(l => l.TikTokReturnId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.TenantId, r.TikTokReturnId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(r => new { r.TenantId, r.ConnectionId, r.ReturnStatus });
        builder.HasIndex(r => new { r.TenantId, r.TikTokOrderId });
    }
}
