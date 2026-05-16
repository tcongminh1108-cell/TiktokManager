using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Returns;

public sealed class ReturnEventHandler(
    IApplicationDbContext db,
    ITikTokApiClient apiClient,
    ITikTokTokenProtector tokenProtector,
    IOutboxService outbox,
    ILogger<ReturnEventHandler> logger) : IReturnEventHandler
{
    public async Task HandleAsync(WebhookEvent webhookEvent, CancellationToken ct = default)
    {
        var returnId = ExtractReturnId(webhookEvent.Payload);
        var tenantId = webhookEvent.TenantId;
        var connectionId = webhookEvent.ConnectionId
            ?? throw new InvalidOperationException(
                $"WebhookEvent '{webhookEvent.EventId}' has no ConnectionId.");

        var connection = await db.TikTokShopConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == connectionId && !c.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Connection {connectionId} not found.");

        var ctx = new TikTokApiContext(
            tokenProtector.Unprotect(connection.AccessToken),
            tokenProtector.Unprotect(connection.ShopCipher),
            connection.BaseApiUrl);

        var raw = await apiClient.GetReturnDetailRawAsync(ctx, returnId, ct);
        if (string.IsNullOrEmpty(raw))
        {
            logger.LogWarning("TikTok returned empty response for return {ReturnId}.", returnId);
            return;
        }

        var detail = ParseReturnDetail(returnId, raw);
        if (detail is null)
        {
            logger.LogWarning("Could not parse TikTok return detail for return {ReturnId}.", returnId);
            return;
        }

        var tikTokReturn = await UpsertReturnAsync(detail, connectionId, tenantId, raw, ct);

        // Only ReturnReceived status triggers stock movement; other statuses just update the record
        if (detail.Status == TikTokReturnStatus.ReturnReceived)
        {
            foreach (var line in detail.Lines)
                await ProcessReturnReceivedLineAsync(tikTokReturn, line, ctx.ShopCipher, tenantId, ct);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Processed TikTok return {ReturnId} (status={Status}) for tenant {TenantId}.",
            returnId, detail.Status, tenantId);
    }

    // ─── Upsert helpers ───────────────────────────────────────────────────────────

    private async Task<TikTokReturn> UpsertReturnAsync(
        ParsedReturnDetail detail, Guid connectionId, Guid tenantId, string raw, CancellationToken ct)
    {
        var existing = await db.TikTokReturns
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.TikTokReturnId == detail.ReturnId, ct);

        if (existing is null)
        {
            existing = new TikTokReturn
            {
                TenantId = tenantId,
                ConnectionId = connectionId,
                TikTokReturnId = detail.ReturnId,
                TikTokOrderId = detail.OrderId,
                ReturnStatus = detail.Status,
                ReturnReason = detail.ReturnReason,
                RequestedAt = detail.RequestedAt,
                ApprovedAt = detail.ApprovedAt,
                ReceivedAt = detail.ReceivedAt,
                RefundedAt = detail.RefundedAt,
                RefundAmount = detail.RefundAmount,
                RawData = raw
            };
            db.TikTokReturns.Add(existing);
        }
        else
        {
            existing.ReturnStatus = detail.Status;
            existing.ApprovedAt = detail.ApprovedAt;
            existing.ReceivedAt = detail.ReceivedAt;
            existing.RefundedAt = detail.RefundedAt;
            existing.RefundAmount = detail.RefundAmount;
            existing.RawData = raw;
        }

        return existing;
    }

    private async Task<TikTokReturnLine> UpsertReturnLineAsync(
        TikTokReturn tikTokReturn, ParsedReturnLine line, Guid tenantId, CancellationToken ct)
    {
        var existing = await db.TikTokReturnLines
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.LineItemId == line.LineItemId, ct);

        if (existing is null)
        {
            existing = new TikTokReturnLine
            {
                TenantId = tenantId,
                TikTokReturnId = tikTokReturn.Id,
                LineItemId = line.LineItemId,
                OriginalOrderItemId = line.OriginalOrderItemId,
                TikTokSkuId = line.TikTokSkuId,
                SkuName = line.SkuName,
                Quantity = line.Quantity,
                RefundAmount = line.RefundAmount,
                SyncStatus = TikTokOrderSyncStatus.MappingPending
            };
            db.TikTokReturnLines.Add(existing);
        }

        return existing;
    }

    // ─── ReturnReceived processing ────────────────────────────────────────────────

    private async Task ProcessReturnReceivedLineAsync(
        TikTokReturn tikTokReturn, ParsedReturnLine line,
        string shopCipher, Guid tenantId, CancellationToken ct)
    {
        var returnLine = await UpsertReturnLineAsync(tikTokReturn, line, tenantId, ct);

        if (returnLine.SyncStatus == TikTokOrderSyncStatus.StockApplied) return;

        var mapping = await db.ProductTikTokMappings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId
                && m.ConnectionId == tikTokReturn.ConnectionId
                && m.TikTokSkuId == line.TikTokSkuId && !m.IsDeleted, ct);

        if (mapping is null)
        {
            // Try resolving via original order item if available
            if (!string.IsNullOrEmpty(line.OriginalOrderItemId))
            {
                var originalItem = await db.TikTokOrderItems
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(i => i.TenantId == tenantId
                        && i.LineItemId == line.OriginalOrderItemId, ct);

                if (originalItem?.ProductId.HasValue == true)
                {
                    returnLine.ProductId = originalItem.ProductId;
                    returnLine.MappingId = originalItem.MappingId;
                    await CreateReturnInMovementAsync(
                        returnLine, originalItem.ProductId!.Value, line, shopCipher,
                        tikTokReturn.TikTokReturnId, tenantId, ct);
                    return;
                }
            }

            returnLine.SyncStatus = TikTokOrderSyncStatus.MappingPending;
            returnLine.LastError = $"No product mapping for TikTok SKU '{line.TikTokSkuId}'.";
            return;
        }

        returnLine.ProductId = mapping.ProductId;
        returnLine.MappingId = mapping.Id;
        await CreateReturnInMovementAsync(
            returnLine, mapping.ProductId, line, shopCipher,
            tikTokReturn.TikTokReturnId, tenantId, ct);
    }

    private async Task CreateReturnInMovementAsync(
        TikTokReturnLine returnLine, Guid productId, ParsedReturnLine line,
        string shopCipher, string tikTokReturnId, Guid tenantId, CancellationToken ct)
    {
        var movementKey = $"tiktok-return-in:{shopCipher}:{tikTokReturnId}:{line.LineItemId}";

        var alreadyExists = await db.StockMovements
            .IgnoreQueryFilters()
            .AnyAsync(sm => sm.TenantId == tenantId && sm.IdempotencyKey == movementKey, ct);

        if (!alreadyExists)
        {
            db.StockMovements.Add(new StockMovement
            {
                TenantId = tenantId,
                ProductId = productId,
                Type = StockMovementType.ReturnIn,
                Source = StockMovementSource.TikTokReturn,
                Quantity = line.Quantity,
                UnitCost = line.RefundAmount,
                OccurredAt = DateTimeOffset.UtcNow,
                IdempotencyKey = movementKey,
                TikTokReturnLineId = returnLine.Id
            });

            // Enqueue inventory push to TikTok
            outbox.EnqueuePushInventory(productId, tenantId);
        }

        returnLine.MovementKey = movementKey;
        returnLine.SyncStatus = TikTokOrderSyncStatus.StockApplied;
        returnLine.LastError = null;

        logger.LogInformation(
            "ReturnIn movement created for product {ProductId}, return line {LineItemId}.",
            productId, line.LineItemId);
    }

    // ─── Payload parsing ──────────────────────────────────────────────────────────

    private static string ExtractReturnId(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("return_id", out var id) && id.GetString() is string direct)
            return direct;

        if (root.TryGetProperty("data", out var data)
            && data.TryGetProperty("return_id", out id) && id.GetString() is string nested)
            return nested;

        throw new InvalidOperationException(
            $"Cannot extract return_id from payload: {payload[..Math.Min(200, payload.Length)]}");
    }

    private static ParsedReturnDetail? ParseReturnDetail(string expectedReturnId, string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var data)) return null;

            // TikTok may return a "returns" array or a single "return" object
            JsonElement returnEl;
            if (data.TryGetProperty("returns", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                JsonElement? found = null;
                foreach (var r in arr.EnumerateArray())
                {
                    if (r.TryGetProperty("return_order_id", out var rid) && rid.GetString() == expectedReturnId)
                    { found = r; break; }
                }
                if (found is null) return null;
                returnEl = found.Value;
            }
            else if (data.TryGetProperty("return", out returnEl))
            {
                // single object format
            }
            else return null;

            var orderId = GetString(returnEl, "order_id");
            var reason = GetStringOrNull(returnEl, "return_reason");
            var refundAmount = ParseDecimal(returnEl, "refund_amount");
            var status = MapReturnStatus(GetString(returnEl, "status"));

            var requestedAt = DateTimeOffset.FromUnixTimeSeconds(
                returnEl.TryGetProperty("create_time", out var ct) ? ct.GetInt64() : 0);
            var approvedAt = returnEl.TryGetProperty("approve_time", out var at) && at.ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(at.GetInt64())
                : (DateTimeOffset?)null;
            var receivedAt = returnEl.TryGetProperty("receive_time", out var rt) && rt.ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(rt.GetInt64())
                : (DateTimeOffset?)null;
            var refundedAt = returnEl.TryGetProperty("refund_time", out var rft) && rft.ValueKind == JsonValueKind.Number
                ? DateTimeOffset.FromUnixTimeSeconds(rft.GetInt64())
                : (DateTimeOffset?)null;

            var lines = new List<ParsedReturnLine>();
            if (returnEl.TryGetProperty("line_items", out var itemsEl))
            {
                foreach (var li in itemsEl.EnumerateArray())
                {
                    lines.Add(new ParsedReturnLine(
                        LineItemId: GetString(li, "id"),
                        OriginalOrderItemId: GetStringOrNull(li, "order_line_item_id"),
                        TikTokSkuId: GetString(li, "sku_id"),
                        SkuName: GetStringOrNull(li, "sku_name"),
                        Quantity: li.TryGetProperty("quantity", out var q) ? q.GetInt32() : 0,
                        RefundAmount: ParseDecimal(li, "refund_amount")));
                }
            }

            return new ParsedReturnDetail(
                expectedReturnId, orderId, status, reason,
                refundAmount, requestedAt, approvedAt, receivedAt, refundedAt, lines);
        }
        catch { return null; }
    }

    private static TikTokReturnStatus MapReturnStatus(string status) =>
        status.ToUpperInvariant() switch
        {
            "PROCESSING" or "REQUESTED" or "RETURN_REQUESTED" => TikTokReturnStatus.Requested,
            "APPROVED" or "RETURN_APPROVED" => TikTokReturnStatus.Approved,
            "REJECTED" or "RETURN_REJECTED" => TikTokReturnStatus.Rejected,
            "GOODS_RECEIVED" or "RETURN_RECEIVED" or "RETURNED" => TikTokReturnStatus.ReturnReceived,
            "REFUNDED" or "COMPLETED" => TikTokReturnStatus.Refunded,
            "CLOSED" or "CANCELLED" => TikTokReturnStatus.Closed,
            _ => TikTokReturnStatus.Requested
        };

    private static decimal ParseDecimal(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var val)) return 0;
        if (val.ValueKind == JsonValueKind.String) { decimal.TryParse(val.GetString(), out var v); return v; }
        if (val.ValueKind == JsonValueKind.Number) return val.GetDecimal();
        return 0;
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) ? v.GetString() ?? string.Empty : string.Empty;

    private static string? GetStringOrNull(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) ? v.GetString() : null;

    private sealed record ParsedReturnDetail(
        string ReturnId,
        string OrderId,
        TikTokReturnStatus Status,
        string? ReturnReason,
        decimal RefundAmount,
        DateTimeOffset RequestedAt,
        DateTimeOffset? ApprovedAt,
        DateTimeOffset? ReceivedAt,
        DateTimeOffset? RefundedAt,
        IReadOnlyList<ParsedReturnLine> Lines);

    private sealed record ParsedReturnLine(
        string LineItemId,
        string? OriginalOrderItemId,
        string TikTokSkuId,
        string? SkuName,
        int Quantity,
        decimal RefundAmount);
}
