using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(500);
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(50);

        // IsActive is a computed property — not stored in DB
        builder.Ignore(rt => rt.IsActive);

        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });
        builder.HasIndex(rt => rt.TenantId);

        // Self-referencing FK for token rotation chain
        builder.HasOne(rt => rt.ReplacedBy)
               .WithMany()
               .HasForeignKey(rt => rt.ReplacedByTokenId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
