using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;
using TikTokShop.Infrastructure.ExternalServices.TikTok;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class TikTokTokenRefreshService(
    IServiceScopeFactory scopeFactory,
    ILogger<TikTokTokenRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for startup to settle before first run.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RefreshExpiringTokensAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task RefreshExpiringTokensAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var tikTokClient = scope.ServiceProvider.GetRequiredService<ITikTokApiClient>();
            var tokenProtector = scope.ServiceProvider.GetRequiredService<ITikTokTokenProtector>();

            var threshold = DateTimeOffset.UtcNow.AddHours(2);

            // IgnoreQueryFilters: background service has no HTTP context (no TenantId in ICurrentUser).
            var connections = await db.TikTokShopConnections
                .IgnoreQueryFilters()
                .Where(c => !c.IsDeleted &&
                            c.Status == TikTokShopConnectionStatus.Active &&
                            c.TokenExpiresAt < threshold)
                .ToListAsync(ct);

            if (connections.Count == 0)
                return;

            logger.LogInformation("Refreshing {Count} TikTok connection token(s).", connections.Count);

            foreach (var connection in connections)
            {
                try
                {
                    var plainRefresh = tokenProtector.Unprotect(connection.RefreshToken);
                    var tokens = await tikTokClient.RefreshTokenAsync(plainRefresh, ct);

                    connection.AccessToken = tokenProtector.Protect(tokens.AccessToken);
                    connection.RefreshToken = tokenProtector.Protect(tokens.RefreshToken);
                    connection.TokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokens.AccessTokenExpiresIn);
                    connection.Status = TikTokShopConnectionStatus.Active;

                    logger.LogInformation(
                        "Refreshed token for connection {ConnectionId} (Shop: {ShopName})",
                        connection.Id, connection.ShopName);
                }
                catch (Exception ex)
                {
                    connection.Status = TikTokShopConnectionStatus.Expired;
                    logger.LogError(ex,
                        "Failed to refresh token for connection {ConnectionId} (Shop: {ShopName}). Marked as Expired.",
                        connection.Id, connection.ShopName);
                }
            }

            await db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in TikTokTokenRefreshService.");
        }
    }
}
