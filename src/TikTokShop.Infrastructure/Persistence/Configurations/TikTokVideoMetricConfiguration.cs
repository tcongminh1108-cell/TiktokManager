using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TikTokVideoMetricConfiguration : IEntityTypeConfiguration<TikTokVideoMetric>
{
    public void Configure(EntityTypeBuilder<TikTokVideoMetric> builder)
    {
        // FK configured via TikTokVideoConfiguration (HasMany/WithOne)
        builder.HasIndex(m => new { m.TikTokVideoId, m.CapturedAt });
        builder.HasIndex(m => new { m.TenantId, m.TikTokVideoId, m.CapturedAt });
    }
}
