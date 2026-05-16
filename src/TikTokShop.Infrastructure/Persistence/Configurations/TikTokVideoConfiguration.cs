using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokVideoConfiguration : IEntityTypeConfiguration<TikTokVideo>
{
    public void Configure(EntityTypeBuilder<TikTokVideo> builder)
    {
        builder.Property(v => v.TikTokVideoId).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Title).HasMaxLength(500);
        builder.Property(v => v.ThumbnailUrl).HasMaxLength(2000);
        builder.Property(v => v.VideoUrl).HasMaxLength(2000);
        builder.Property(v => v.VideoStatus).HasMaxLength(50);

        builder.HasOne(v => v.Connection)
               .WithMany()
               .HasForeignKey(v => v.ConnectionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Metrics)
               .WithOne(m => m.Video)
               .HasForeignKey(m => m.TikTokVideoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.TenantId, v.ConnectionId, v.TikTokVideoId })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(v => new { v.TenantId, v.ConnectionId, v.ViewCount });
    }
}
