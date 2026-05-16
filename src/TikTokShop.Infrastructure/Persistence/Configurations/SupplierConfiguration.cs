using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Address).HasColumnType("text");
        builder.Property(s => s.Note).HasColumnType("text");

        builder.HasIndex(s => new { s.TenantId, s.Code })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(s => s.TenantId);
    }
}
