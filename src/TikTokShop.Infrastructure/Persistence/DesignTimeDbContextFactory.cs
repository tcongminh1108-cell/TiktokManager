using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TikTokShop.Infrastructure.Persistence;

// Used only by `dotnet ef migrations` — not loaded at runtime
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = FindApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string FindApiProjectPath()
    {
        var current = Directory.GetCurrentDirectory();

        // Handles running from: solution root, Infrastructure project dir, or any subdirectory
        string[] candidates =
        [
            Path.Combine(current, "src", "TikTokShop.Api"),     // solution root
            Path.Combine(current, "..", "TikTokShop.Api"),       // from Infrastructure/
            Path.Combine(current, "..", "..", "TikTokShop.Api"), // from nested subfolder
            current,                                              // fallback
        ];

        foreach (var path in candidates)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
                return fullPath;
        }

        return current;
    }
}
