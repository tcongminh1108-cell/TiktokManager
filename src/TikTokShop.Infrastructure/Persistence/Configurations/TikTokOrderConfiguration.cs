using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokOrderConfiguration : IEntityTypeConfiguration<TikTokOrder>
{
    public void Configure(EntityTypeBuilder<TikTokOrder> builder)
    {
        builder.Property(o => o.OrderId).IsRequired().HasMaxLength(100);
        builder.Property(o => o.StatusCode).IsRequired();
        builder.Property(o => o.BuyerUsername).HasMaxLength(200);
        builder.Property(o => o.RawData).HasColumnType("text");

        builder.HasOne(o => o.Connection)
               .WithMany()
               .HasForeignKey(o => o.ConnectionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.TikTokOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // One order per tenant+shop (natural business key)
        builder.HasIndex(o => new { o.TenantId, o.OrderId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        // Common filter: list orders by connection and status
        builder.HasIndex(o => new { o.TenantId, o.ConnectionId, o.StatusCode });

        // Sort by last activity
        builder.HasIndex(o => new { o.TenantId, o.TikTokUpdatedAt });
    }
}
