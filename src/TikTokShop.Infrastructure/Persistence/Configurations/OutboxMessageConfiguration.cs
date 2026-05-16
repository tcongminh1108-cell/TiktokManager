using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.Property(m => m.Type).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("text");
        builder.Property(m => m.LastError).HasMaxLength(2000);

        // Primary polling index: pending messages ordered by creation time
        builder.HasIndex(m => new { m.Status, m.NextAttemptAt, m.CreatedAt });

        // Tenant-scoped lookup for admin monitoring
        builder.HasIndex(m => new { m.TenantId, m.Status, m.CreatedAt });
    }
}
