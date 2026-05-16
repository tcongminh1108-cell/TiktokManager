using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TikTokShop.Infrastructure.Persistence;

// Used only by `dotnet ef migrations` — not loaded at runtime
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=tiktokshop_dev;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
