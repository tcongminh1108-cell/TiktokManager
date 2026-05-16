using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using TikTokShop.Infrastructure.Persistence;

namespace TikTokShop.IntegrationTests.Infrastructure;

/// <summary>
/// Shared WebApplicationFactory backed by a real PostgreSQL Testcontainer.
/// Starts the container, runs EF Core migrations, and removes all background services
/// (TikTok integrations, outbox, etc.) so tests run without external dependencies.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("tiktokshop_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    // ── IAsyncLifetime ──────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    // ── WebApplicationFactory ───────────────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override connection string with the Testcontainer's
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                // Set a deterministic JWT secret so tokens are predictable across requests
                ["Jwt:SecretKey"] = "integration-test-secret-key-minimum-64-characters-long-for-hmac512!!",
                ["Jwt:Issuer"] = "TikTokShop",
                ["Jwt:Audience"] = "TikTokShop",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL background services — they try to connect to TikTok/external systems
            services.RemoveAll<IHostedService>();
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private async Task MigrateAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>Creates an <see cref="HttpClient"/> that sends requests to the in-memory test server.</summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
}
