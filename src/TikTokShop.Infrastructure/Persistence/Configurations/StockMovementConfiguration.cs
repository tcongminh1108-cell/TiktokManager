using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.Property(sm => sm.Quantity).IsRequired();
        builder.Property(sm => sm.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(sm => sm.IdempotencyKey).IsRequired().HasMaxLength(200);
        builder.Property(sm => sm.Note).HasColumnType("text");

        // OccurredAt is the business timestamp; always stored in UTC.
        builder.Property(sm => sm.OccurredAt).IsRequired();

        // Unique idempotency key per tenant.
        builder.HasIndex(sm => new { sm.TenantId, sm.IdempotencyKey }).IsUnique();

        // Primary aggregate index: stock-on-hand and history queries.
        builder.HasIndex(sm => new { sm.TenantId, sm.ProductId, sm.OccurredAt });

        // Source-based queries (e.g., TikTok vs Manual analytics).
        builder.HasIndex(sm => new { sm.TenantId, sm.Source, sm.OccurredAt });

        // FK reference indexes (actual FK constraints added with their respective entity migrations).
        builder.HasIndex(sm => sm.TikTokOrderItemId).HasFilter("tik_tok_order_item_id IS NOT NULL");

        // Quantity must always be positive; direction is conveyed by Type.
        builder.ToTable(t => t.HasCheckConstraint("ck_stock_movements_quantity_positive", "quantity > 0"));

        // IsDeleted is forced false — movements are never soft-deleted.
        builder.Property(sm => sm.IsDeleted).HasDefaultValue(false);
    }
}
