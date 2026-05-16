using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnType("text");
        builder.Property(p => p.SellingPrice).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(p => p.Unit).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        builder.HasIndex(p => new { p.TenantId, p.Code })
               .IsUnique()
               .HasFilter("is_deleted = false");

        builder.HasIndex(p => new { p.TenantId, p.IsActive });
        builder.HasIndex(p => p.TenantId);
    }
}
