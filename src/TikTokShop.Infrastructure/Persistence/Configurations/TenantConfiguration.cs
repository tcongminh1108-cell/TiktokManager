using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(50);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(50);
        builder.Property(t => t.Status).HasConversion<int>().IsRequired();

        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasMany(t => t.Users)
               .WithOne(u => u.Tenant)
               .HasForeignKey(u => u.TenantId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
