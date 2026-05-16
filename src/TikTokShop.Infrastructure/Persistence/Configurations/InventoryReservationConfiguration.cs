using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.ReservedAt).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.IdempotencyKey).IsRequired().HasMaxLength(300);

        // Unique idempotency key per tenant.
        builder.HasIndex(r => new { r.TenantId, r.IdempotencyKey }).IsUnique();

        // Primary lookup: active reservations for a product.
        builder.HasIndex(r => new { r.TenantId, r.ProductId, r.Status });

        // Partial index for the expiry background job — only Active reservations need scanning.
        builder.HasIndex(r => new { r.TenantId, r.ExpiresAt })
               .HasFilter("status = 1");

        // Quantity must always be positive.
        builder.ToTable(t => t.HasCheckConstraint("ck_inventory_reservations_quantity_positive", "quantity > 0"));
    }
}
