using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.Property(e => e.EventId).IsRequired().HasMaxLength(200);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).IsRequired().HasColumnType("text");
        builder.Property(e => e.Source).IsRequired().HasMaxLength(20).HasDefaultValue("webhook");
        builder.Property(e => e.LastError).HasColumnType("text");

        // EventId must be globally unique per tenant — used as idempotency key.
        builder.HasIndex(e => new { e.TenantId, e.EventId }).IsUnique();

        // Background processor queries by status + recency.
        builder.HasIndex(e => new { e.TenantId, e.Status, e.ReceivedAt });

        // History view per connection.
        builder.HasIndex(e => new { e.TenantId, e.ConnectionId, e.ReceivedAt });

        // IsDeleted is always false for this append-only entity (enforced at DB level).
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
