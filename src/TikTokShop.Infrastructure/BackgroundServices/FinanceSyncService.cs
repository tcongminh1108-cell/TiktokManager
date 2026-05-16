using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class FinanceSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<FinanceSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(40), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAllConnectionsAsync(stoppingToken);
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SyncAllConnectionsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var apiClient = scope.ServiceProvider.GetRequiredService<ITikTokApiClient>();
            var tokenProtector = scope.ServiceProvider.GetRequiredService<ITikTokTokenProtector>();

            var connections = await db.TikTokShopConnections
                .IgnoreQueryFilters()
                .Where(c => !c.IsDeleted && c.Status == TikTokShopConnectionStatus.Active)
                .ToListAsync(ct);

            foreach (var connection in connections)
            {
                try
                {
                    await SyncConnectionAsync(connection, db, apiClient, tokenProtector, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "FinanceSyncService: failed to sync connection {ConnectionId}.", connection.Id);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "FinanceSyncService: unhandled error in SyncAllConnectionsAsync.");
        }
    }

    private async Task SyncConnectionAsync(
        TikTokShopConnection connection,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        ITikTokTokenProtector tokenProtector,
        CancellationToken ct)
    {
        var ctx = new TikTokApiContext(
            tokenProtector.Unprotect(connection.AccessToken),
            tokenProtector.Unprotect(connection.ShopCipher),
            connection.BaseApiUrl);

        // Incremental sync: start from the latest statement time or 30 days ago
        var lastTime = await db.TikTokFinanceStatements
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == connection.TenantId && s.ConnectionId == connection.Id && !s.IsDeleted)
            .MaxAsync(s => (DateTimeOffset?)s.StatementTime, ct) ?? DateTimeOffset.UtcNow.AddDays(-30);

        var fromTs = lastTime.ToUnixTimeSeconds();
        var toTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string? pageToken = null;
        do
        {
            var raw = await apiClient.GetFinanceStatementsRawAsync(ctx, fromTs, toTs, pageToken, 20, ct);
            if (raw is null) break;

            using var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("data");

            if (!data.TryGetProperty("statements", out var statementsEl)
                || statementsEl.ValueKind != JsonValueKind.Array)
                break;

            foreach (var statEl in statementsEl.EnumerateArray())
                await UpsertStatementAsync(statEl, connection, db, apiClient, ctx, ct);

            pageToken = data.TryGetProperty("next_page_token", out var npt) ? npt.GetString() : null;
        }
        while (!string.IsNullOrEmpty(pageToken));
    }

    private async Task UpsertStatementAsync(
        JsonElement statEl,
        TikTokShopConnection connection,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        TikTokApiContext ctx,
        CancellationToken ct)
    {
        var tikTokStatementId = statEl.TryGetProperty("statement_id", out var sid)
            ? sid.GetString() : null;
        if (string.IsNullOrEmpty(tikTokStatementId)) return;

        var existing = await db.TikTokFinanceStatements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s =>
                s.TenantId == connection.TenantId &&
                s.TikTokStatementId == tikTokStatementId, ct);

        var statementTimeUnix = statEl.TryGetProperty("statement_time", out var st) ? st.GetInt64() : 0L;
        var statementTime = DateTimeOffset.FromUnixTimeSeconds(statementTimeUnix);

        if (existing is null)
        {
            existing = new TikTokFinanceStatement
            {
                TenantId = connection.TenantId,
                ConnectionId = connection.Id,
                TikTokStatementId = tikTokStatementId,
                StatementTime = statementTime,
                Currency = statEl.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "USD" : "USD",
                SaleAmount = ReadDecimal(statEl, "sale_amount"),
                TikTokFee = ReadDecimal(statEl, "tiktok_fee"),
                ShippingFee = ReadDecimal(statEl, "shipping_fee"),
                PromotionAmount = ReadDecimal(statEl, "promotion_amount"),
                AdjustmentAmount = ReadDecimal(statEl, "adjustment_amount"),
                SettlementAmount = ReadDecimal(statEl, "settlement_amount"),
                StatementType = statEl.TryGetProperty("statement_type", out var styp) ? styp.GetString() : null,
                RawData = statEl.GetRawText()
            };
            db.TikTokFinanceStatements.Add(existing);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            existing.SaleAmount = ReadDecimal(statEl, "sale_amount", existing.SaleAmount);
            existing.TikTokFee = ReadDecimal(statEl, "tiktok_fee", existing.TikTokFee);
            existing.ShippingFee = ReadDecimal(statEl, "shipping_fee", existing.ShippingFee);
            existing.PromotionAmount = ReadDecimal(statEl, "promotion_amount", existing.PromotionAmount);
            existing.AdjustmentAmount = ReadDecimal(statEl, "adjustment_amount", existing.AdjustmentAmount);
            existing.SettlementAmount = ReadDecimal(statEl, "settlement_amount", existing.SettlementAmount);
            await db.SaveChangesAsync(ct);
        }

        await SyncStatementOrdersAsync(existing, connection, db, apiClient, ctx, ct);
    }

    private async Task SyncStatementOrdersAsync(
        TikTokFinanceStatement statement,
        TikTokShopConnection connection,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        TikTokApiContext ctx,
        CancellationToken ct)
    {
        string? pageToken = null;
        do
        {
            var raw = await apiClient.GetFinanceStatementOrdersRawAsync(
                ctx, statement.TikTokStatementId, pageToken, ct);
            if (raw is null) break;

            using var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("data");

            if (!data.TryGetProperty("orders", out var ordersEl)
                || ordersEl.ValueKind != JsonValueKind.Array)
                break;

            foreach (var orderEl in ordersEl.EnumerateArray())
            {
                var tikTokOrderId = orderEl.TryGetProperty("order_id", out var oid) ? oid.GetString() : null;
                if (string.IsNullOrEmpty(tikTokOrderId)) continue;

                var existing = await db.TikTokOrderFinances
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(f =>
                        f.TenantId == connection.TenantId &&
                        f.TikTokOrderId == tikTokOrderId, ct);

                if (existing is null)
                {
                    db.TikTokOrderFinances.Add(new TikTokOrderFinance
                    {
                        TenantId = connection.TenantId,
                        ConnectionId = connection.Id,
                        TikTokFinanceStatementId = statement.Id,
                        TikTokOrderId = tikTokOrderId,
                        SaleAmount = ReadDecimal(orderEl, "sale_amount"),
                        TikTokFee = ReadDecimal(orderEl, "tiktok_fee"),
                        ShippingFee = ReadDecimal(orderEl, "shipping_fee"),
                        PromotionAmount = ReadDecimal(orderEl, "promotion_amount"),
                        AdjustmentAmount = ReadDecimal(orderEl, "adjustment_amount"),
                        NetRevenue = ReadDecimal(orderEl, "net_revenue"),
                        Currency = orderEl.TryGetProperty("currency", out var cur) ? cur.GetString() : null,
                        RawData = orderEl.GetRawText()
                    });
                }
                else
                {
                    existing.TikTokFinanceStatementId ??= statement.Id;
                    existing.NetRevenue = ReadDecimal(orderEl, "net_revenue", existing.NetRevenue);
                    existing.SaleAmount = ReadDecimal(orderEl, "sale_amount", existing.SaleAmount);
                    existing.TikTokFee = ReadDecimal(orderEl, "tiktok_fee", existing.TikTokFee);
                }
            }

            await db.SaveChangesAsync(ct);
            pageToken = data.TryGetProperty("next_page_token", out var npt) ? npt.GetString() : null;
        }
        while (!string.IsNullOrEmpty(pageToken));
    }

    private static decimal ReadDecimal(JsonElement el, string prop, decimal fallback = 0m)
    {
        if (el.TryGetProperty(prop, out var v))
        {
            if (v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d)) return d;
            if (v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString(), out var ds)) return ds;
        }
        return fallback;
    }
}
