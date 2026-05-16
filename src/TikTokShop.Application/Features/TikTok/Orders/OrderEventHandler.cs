using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Orders;

public sealed class OrderEventHandler(
    IApplicationDbContext db,
    ITikTokApiClient apiClient,
    ITikTokTokenProtector tokenProtector,
    IOutboxService outbox,
    ILogger<OrderEventHandler> logger) : IOrderEventHandler
{
    public async Task HandleAsync(WebhookEvent webhookEvent, CancellationToken ct = default)
    {
        var orderId = ExtractOrderId(webhookEvent.Payload);
        var tenantId = webhookEvent.TenantId;
        var connectionId = webhookEvent.ConnectionId
            ?? throw new InvalidOperationException(
                $"WebhookEvent '{webhookEvent.EventId}' has no ConnectionId.");

        // Load connection bypassing tenant filter — background service has no HTTP context
        var connection = await db.TikTokShopConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == connectionId && !c.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Connection {connectionId} not found.");

        var ctx = new TikTokApiContext(
            tokenProtector.Unprotect(connection.AccessToken),
            tokenProtector.Unprotect(connection.ShopCipher),
            connection.BaseApiUrl);

        var raw = await apiClient.GetOrderDetailRawAsync(ctx, orderId, ct);
        if (string.IsNullOrEmpty(raw))
        {
            logger.LogWarning("TikTok returned empty response for order {OrderId}.", orderId);
            return;
        }

        var detail = ParseOrderDetail(orderId, raw);
        if (detail is null)
        {
            logger.LogWarning("Could not parse TikTok order detail for order {OrderId}.", orderId);
            return;
        }

        var order = await UpsertOrderAsync(detail, connectionId, tenantId, raw, ct);
        var shopCipher = ctx.ShopCipher;

        // AwaitingShipment: each item needs a row-level lock → own transaction per item
        if (detail.StatusCode == (int)TikTokOrderStatus.AwaitingShipment)
        {
            foreach (var li in detail.LineItems)
                await ProcessAwaitingShipmentItemAsync(order, li, shopCipher, tenantId, ct);
        }
        else
        {
            foreach (var li in detail.LineItems)
                await ProcessItemByStatusAsync(order, li, shopCipher, detail.StatusCode, tenantId, ct);

            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "Processed TikTok order {OrderId} (status={Status}) for tenant {TenantId}.",
            orderId, detail.StatusCode, tenantId);
    }

    // ─── Upsert helpers ───────────────────────────────────────────────────────────

    private async Task<TikTokOrder> UpsertOrderAsync(
        ParsedOrderDetail detail, Guid connectionId, Guid tenantId, string raw, CancellationToken ct)
    {
        var order = await db.TikTokOrders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.OrderId == detail.OrderId, ct);

        if (order is null)
        {
            order = new TikTokOrder
            {
                TenantId = tenantId,
                ConnectionId = connectionId,
                OrderId = detail.OrderId,
                StatusCode = detail.StatusCode,
                BuyerUsername = detail.BuyerUsername,
                TikTokCreatedAt = detail.TikTokCreatedAt,
                TikTokUpdatedAt = detail.TikTokUpdatedAt,
                RawData = raw
            };
            db.TikTokOrders.Add(order);
        }
        else
        {
            order.StatusCode = detail.StatusCode;
            order.TikTokUpdatedAt = detail.TikTokUpdatedAt;
            order.RawData = raw;
        }

        return order;
    }

    private async Task<TikTokOrderItem> UpsertOrderItemAsync(
        TikTokOrder order, ParsedLineItem li, Guid tenantId, CancellationToken ct)
    {
        var item = await db.TikTokOrderItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.LineItemId == li.LineItemId, ct);

        if (item is null)
        {
            item = new TikTokOrderItem
            {
                TenantId = tenantId,
                TikTokOrderId = order.Id,
                LineItemId = li.LineItemId,
                TikTokProductId = li.TikTokProductId,
                TikTokSkuId = li.TikTokSkuId,
                SkuName = li.SkuName,
                Quantity = li.Quantity,
                SalePrice = li.SalePrice,
                SyncStatus = TikTokOrderSyncStatus.MappingPending
            };
            db.TikTokOrderItems.Add(item);
        }
        else
        {
            // Refresh quantity/price in case TikTok data was corrected
            item.Quantity = li.Quantity;
            item.SalePrice = li.SalePrice;
        }

        return item;
    }

    // ─── AwaitingShipment (111) — reservation with row-level lock ────────────────

    private async Task ProcessAwaitingShipmentItemAsync(
        TikTokOrder order, ParsedLineItem li, string shopCipher, Guid tenantId, CancellationToken ct)
    {
        var item = await UpsertOrderItemAsync(order, li, tenantId, ct);

        // Skip if already progressed beyond the reservation stage
        if (item.SyncStatus is TikTokOrderSyncStatus.Reserved
            or TikTokOrderSyncStatus.StockApplied
            or TikTokOrderSyncStatus.StockReversed
            or TikTokOrderSyncStatus.Released)
        {
            await db.SaveChangesAsync(ct);
            return;
        }

        var mapping = await db.ProductTikTokMappings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId
                && m.ConnectionId == order.ConnectionId
                && m.TikTokSkuId == li.TikTokSkuId && !m.IsDeleted, ct);

        if (mapping is null)
        {
            item.SyncStatus = TikTokOrderSyncStatus.MappingPending;
            item.LastError = $"No product mapping for TikTok SKU '{li.TikTokSkuId}'.";
            await db.SaveChangesAsync(ct);
            return;
        }

        item.ProductId = mapping.ProductId;
        item.MappingId = mapping.Id;

        var reservationKey = $"reservation:{shopCipher}:{order.OrderId}:{li.LineItemId}";

        var existing = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.IdempotencyKey == reservationKey, ct);

        if (existing is null)
        {
            await using var tx = await db.BeginTransactionAsync(ct);
            await db.LockProductRowAsync(mapping.ProductId, ct);

            var now = DateTimeOffset.UtcNow;
            db.InventoryReservations.Add(new InventoryReservation
            {
                TenantId = tenantId,
                ProductId = mapping.ProductId,
                Quantity = li.Quantity,
                Status = InventoryReservationStatus.Active,
                TikTokOrderItemId = item.Id,
                ReservedAt = now,
                ExpiresAt = now.AddDays(7),
                IdempotencyKey = reservationKey
            });

            item.ReservationKey = reservationKey;
            item.SyncStatus = TikTokOrderSyncStatus.Reserved;
            item.LastError = null;

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            logger.LogInformation(
                "Reserved {Qty}x product {ProductId} for order {OrderId}/{LineItemId}.",
                li.Quantity, mapping.ProductId, order.OrderId, li.LineItemId);
        }
        else
        {
            // Reservation already exists — just sync item tracking fields
            item.ReservationKey = reservationKey;
            item.SyncStatus = TikTokOrderSyncStatus.Reserved;
            item.LastError = null;
            await db.SaveChangesAsync(ct);
        }
    }

    // ─── All other statuses — batch, no per-item transaction ─────────────────────

    private async Task ProcessItemByStatusAsync(
        TikTokOrder order, ParsedLineItem li, string shopCipher, int statusCode, Guid tenantId, CancellationToken ct)
    {
        var item = await UpsertOrderItemAsync(order, li, tenantId, ct);

        var mapping = await db.ProductTikTokMappings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.TenantId == tenantId
                && m.ConnectionId == order.ConnectionId
                && m.TikTokSkuId == li.TikTokSkuId && !m.IsDeleted, ct);

        if (mapping is null)
        {
            item.SyncStatus = TikTokOrderSyncStatus.MappingPending;
            item.LastError = $"No product mapping for TikTok SKU '{li.TikTokSkuId}'.";
            return;
        }

        item.ProductId = mapping.ProductId;
        item.MappingId = mapping.Id;

        var reservationKey = $"reservation:{shopCipher}:{order.OrderId}:{li.LineItemId}";
        var movementKey = $"tiktok-out:{shopCipher}:{order.OrderId}:{li.LineItemId}";
        var cancelReverseKey = $"tiktok-cancel-reverse:{shopCipher}:{order.OrderId}:{li.LineItemId}";

        try
        {
            switch (statusCode)
            {
                case (int)TikTokOrderStatus.Unpaid:
                case (int)TikTokOrderStatus.AwaitingCollection:
                    item.SyncStatus = TikTokOrderSyncStatus.Synced;
                    item.LastError = null;
                    break;

                case (int)TikTokOrderStatus.InTransit:
                case (int)TikTokOrderStatus.Delivered:
                case (int)TikTokOrderStatus.Completed:
                    await HandleShippedAsync(item, mapping.ProductId, li, reservationKey, movementKey, tenantId, ct);
                    break;

                case (int)TikTokOrderStatus.Cancelled:
                    await HandleCancelledAsync(item, mapping.ProductId, li, reservationKey, cancelReverseKey, tenantId, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            item.SyncStatus = TikTokOrderSyncStatus.Failed;
            item.LastError = ex.Message;
            logger.LogError(ex,
                "Failed to process order {OrderId} item {LineItemId} (status={Status}).",
                order.OrderId, li.LineItemId, statusCode);
        }
    }

    private async Task HandleShippedAsync(
        TikTokOrderItem item, Guid productId, ParsedLineItem li,
        string reservationKey, string movementKey, Guid tenantId, CancellationToken ct)
    {
        if (item.SyncStatus == TikTokOrderSyncStatus.StockApplied) return;

        // Commit existing reservation if still Active
        var reservation = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.IdempotencyKey == reservationKey, ct);

        if (reservation?.Status == InventoryReservationStatus.Active)
        {
            reservation.Status = InventoryReservationStatus.Committed;
            reservation.ResolvedAt = DateTimeOffset.UtcNow;
        }

        // Idempotency: skip if movement already exists
        var movementExists = await db.StockMovements
            .IgnoreQueryFilters()
            .AnyAsync(sm => sm.TenantId == tenantId && sm.IdempotencyKey == movementKey, ct);

        if (!movementExists)
        {
            db.StockMovements.Add(new StockMovement
            {
                TenantId = tenantId,
                ProductId = productId,
                Type = StockMovementType.Out,
                Source = StockMovementSource.TikTokOrder,
                Quantity = li.Quantity,
                UnitCost = li.SalePrice,
                OccurredAt = DateTimeOffset.UtcNow,
                IdempotencyKey = movementKey,
                TikTokOrderItemId = item.Id
            });
        }

        outbox.EnqueuePushInventory(productId, tenantId);
        item.MovementKey = movementKey;
        item.ReservationKey = reservationKey;
        item.SyncStatus = TikTokOrderSyncStatus.StockApplied;
        item.LastError = null;
    }

    private async Task HandleCancelledAsync(
        TikTokOrderItem item, Guid productId, ParsedLineItem li,
        string reservationKey, string cancelReverseKey, Guid tenantId, CancellationToken ct)
    {
        if (item.SyncStatus is TikTokOrderSyncStatus.Released
            or TikTokOrderSyncStatus.StockReversed) return;

        var reservation = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.IdempotencyKey == reservationKey, ct);

        if (reservation?.Status == InventoryReservationStatus.Active)
        {
            // Cancelled before shipment → release reservation; no stock movement needed
            reservation.Status = InventoryReservationStatus.Released;
            reservation.ResolvedAt = DateTimeOffset.UtcNow;
            item.SyncStatus = TikTokOrderSyncStatus.Released;
            item.LastError = null;
        }
        else if (reservation?.Status == InventoryReservationStatus.Committed)
        {
            // Cancelled after stock was committed → compensating In movement
            var reverseExists = await db.StockMovements
                .IgnoreQueryFilters()
                .AnyAsync(sm => sm.TenantId == tenantId && sm.IdempotencyKey == cancelReverseKey, ct);

            if (!reverseExists)
            {
                db.StockMovements.Add(new StockMovement
                {
                    TenantId = tenantId,
                    ProductId = productId,
                    Type = StockMovementType.In,
                    Source = StockMovementSource.TikTokReturn,
                    Quantity = li.Quantity,
                    UnitCost = 0,
                    OccurredAt = DateTimeOffset.UtcNow,
                    IdempotencyKey = cancelReverseKey,
                    TikTokOrderItemId = item.Id,
                    Note = "Compensating movement: order cancelled after stock committed."
                });
                outbox.EnqueuePushInventory(productId, tenantId);
            }

            item.SyncStatus = TikTokOrderSyncStatus.StockReversed;
            item.LastError = null;
        }
        else
        {
            // No reservation → order was cancelled before AwaitingShipment or mapping was pending
            item.SyncStatus = TikTokOrderSyncStatus.Synced;
            item.LastError = null;
        }
    }

    // ─── Payload & response parsing ───────────────────────────────────────────────

    private static string ExtractOrderId(string payload)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        if (root.TryGetProperty("order_id", out var id) && id.GetString() is string directId)
            return directId;

        if (root.TryGetProperty("data", out var data)
            && data.TryGetProperty("order_id", out id) && id.GetString() is string nestedId)
            return nestedId;

        throw new InvalidOperationException(
            $"Cannot extract order_id from payload: {payload[..Math.Min(200, payload.Length)]}");
    }

    private static ParsedOrderDetail? ParseOrderDetail(string expectedOrderId, string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var data)) return null;
            if (!data.TryGetProperty("orders", out var ordersEl)) return null;

            JsonElement? orderEl = null;
            foreach (var o in ordersEl.EnumerateArray())
            {
                if (o.TryGetProperty("order_id", out var oid) && oid.GetString() == expectedOrderId)
                {
                    orderEl = o;
                    break;
                }
            }

            if (orderEl is null) return null;
            var order = orderEl.Value;

            var statusCode = order.TryGetProperty("order_status", out var sc) ? sc.GetInt32() : 0;
            var createTime = order.TryGetProperty("create_time", out var ct2) ? ct2.GetInt64() : 0;
            var updateTime = order.TryGetProperty("update_time", out var ut) ? ut.GetInt64() : 0;
            var buyer = order.TryGetProperty("buyer_uid", out var b) ? b.GetString() : null;

            var lineItems = new List<ParsedLineItem>();
            if (order.TryGetProperty("line_items", out var itemsEl))
            {
                foreach (var li in itemsEl.EnumerateArray())
                {
                    var lineItemId = GetString(li, "id");
                    var productId = GetString(li, "product_id");
                    var skuId = GetString(li, "sku_id");
                    var skuName = GetStringOrNull(li, "sku_name");
                    var qty = li.TryGetProperty("quantity", out var q) ? q.GetInt32() : 0;
                    var salePrice = ParsePrice(li);

                    lineItems.Add(new ParsedLineItem(lineItemId, productId, skuId, skuName, qty, salePrice));
                }
            }

            return new ParsedOrderDetail(
                expectedOrderId, statusCode, buyer,
                DateTimeOffset.FromUnixTimeSeconds(createTime),
                DateTimeOffset.FromUnixTimeSeconds(updateTime),
                lineItems);
        }
        catch
        {
            return null;
        }
    }

    private static decimal ParsePrice(JsonElement li)
    {
        if (!li.TryGetProperty("sale_price", out var sp)) return 0;
        if (sp.ValueKind == JsonValueKind.Object && sp.TryGetProperty("amount", out var amt))
        {
            decimal.TryParse(amt.GetString(), out var v);
            return v;
        }
        if (sp.ValueKind == JsonValueKind.String)
        {
            decimal.TryParse(sp.GetString(), out var v);
            return v;
        }
        if (sp.ValueKind == JsonValueKind.Number)
            return sp.GetDecimal();
        return 0;
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) ? v.GetString() ?? string.Empty : string.Empty;

    private static string? GetStringOrNull(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) ? v.GetString() : null;

    // Private nested records — internal parsing models, not part of public API
    private sealed record ParsedOrderDetail(
        string OrderId,
        int StatusCode,
        string? BuyerUsername,
        DateTimeOffset TikTokCreatedAt,
        DateTimeOffset TikTokUpdatedAt,
        IReadOnlyList<ParsedLineItem> LineItems);

    private sealed record ParsedLineItem(
        string LineItemId,
        string TikTokProductId,
        string TikTokSkuId,
        string? SkuName,
        int Quantity,
        decimal SalePrice);
}
