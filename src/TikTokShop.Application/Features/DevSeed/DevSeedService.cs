using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Features.StockMovements;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.DevSeed;

public sealed class DevSeedService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IStockMovementService stockMovementService) : IDevSeedService
{
    private const int SupplierTarget = 10;
    private const int ProductTarget = 50;
    private const int StockInTarget = 200;   // 4 per product
    private const int StockOutTarget = 150;  // 3 per product
    private const int ReservationTarget = 20;

    public async Task<DevSeedResult> SeedAsync(CancellationToken ct = default)
    {
        var tenantId = currentUser.TenantId;

        var suppliersCreated = await SeedSuppliersAsync(tenantId, ct);
        var productsCreated = await SeedProductsAsync(tenantId, ct);
        var (stockInsCreated, stockOutsCreated) = await SeedMovementsAsync(tenantId, ct);
        var reservationsCreated = await SeedReservationsAsync(tenantId, ct);

        return new DevSeedResult(suppliersCreated, productsCreated, stockInsCreated, stockOutsCreated, reservationsCreated);
    }

    private async Task<int> SeedSuppliersAsync(Guid tenantId, CancellationToken ct)
    {
        var existing = await db.Suppliers.CountAsync(ct);
        if (existing >= SupplierTarget)
            return 0;

        var toCreate = SupplierTarget - existing;
        var suppliers = Enumerable.Range(existing + 1, toCreate).Select(i => new Supplier
        {
            TenantId = tenantId,
            Code = $"SUP-{i:D3}",
            Name = $"Seed Supplier {i:D2}",
            Phone = $"09{i:D8}",
            Email = $"supplier{i:D2}@seed.local",
            Address = $"{i * 10} Seed Street, District {i}"
        }).ToList();

        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync(ct);
        return toCreate;
    }

    private async Task<int> SeedProductsAsync(Guid tenantId, CancellationToken ct)
    {
        var existing = await db.Products.CountAsync(ct);
        if (existing >= ProductTarget)
            return 0;

        var toCreate = ProductTarget - existing;
        var products = Enumerable.Range(existing + 1, toCreate).Select(i => new Product
        {
            TenantId = tenantId,
            Code = $"SEED-{i:D4}",
            Name = $"Seed Product {i:D2}",
            Description = $"Auto-seeded product #{i}",
            SellingPrice = 10_000m * i,
            Unit = i % 3 == 0 ? "kg" : i % 3 == 1 ? "pcs" : "box",
            IsActive = true
        }).ToList();

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
        return toCreate;
    }

    private async Task<(int stockInsCreated, int stockOutsCreated)> SeedMovementsAsync(Guid tenantId, CancellationToken ct)
    {
        var stockInsExisting = await db.StockIns.CountAsync(ct);
        var stockOutsExisting = await db.StockOuts.CountAsync(ct);

        if (stockInsExisting >= StockInTarget && stockOutsExisting >= StockOutTarget)
            return (0, 0);

        var suppliers = await db.Suppliers.Take(SupplierTarget).ToListAsync(ct);
        var products = await db.Products.Take(ProductTarget).ToListAsync(ct);

        if (suppliers.Count == 0 || products.Count == 0)
            return (0, 0);

        var stockInsCreated = 0;
        var stockOutsCreated = 0;

        if (stockInsExisting < StockInTarget)
        {
            // 4 stock-ins per product, quantity=50 each, unit cost varies by product index.
            var stockIns = new List<StockIn>();
            var movements = new List<(Guid ProductId, int Quantity, decimal UnitPrice, Guid StockInId, DateTimeOffset Date)>();

            foreach (var (product, pi) in products.Select((p, i) => (p, i)))
            {
                var supplier = suppliers[pi % suppliers.Count];
                var unitCost = 5_000m * (pi + 1);

                for (var j = 0; j < 4; j++)
                {
                    var date = DateTimeOffset.UtcNow.AddDays(-(ProductTarget * 4 - pi * 4 - j));
                    var si = new StockIn
                    {
                        TenantId = tenantId,
                        ProductId = product.Id,
                        SupplierId = supplier.Id,
                        Quantity = 50,
                        UnitPrice = unitCost,
                        TotalAmount = 50 * unitCost,
                        TransactionDate = date,
                        Note = $"Seed stock-in {j + 1} for {product.Code}"
                    };
                    stockIns.Add(si);
                    movements.Add((product.Id, 50, unitCost, si.Id, date));
                }
            }

            db.StockIns.AddRange(stockIns);

            foreach (var (productId, qty, unitPrice, siId, date) in movements)
            {
                await stockMovementService.RecordAsync(
                    productId, StockMovementType.In, StockMovementSource.Manual,
                    qty, unitPrice, date, $"stockin:{siId}",
                    new StockMovementReference { StockInId = siId }, ct: ct);
            }

            await db.SaveChangesAsync(ct);
            stockInsCreated = stockIns.Count;
        }

        if (stockOutsExisting < StockOutTarget)
        {
            // 3 stock-outs per product, quantity=30 each (leaves 200-90=110 per product).
            var stockOuts = new List<StockOut>();
            var outMovements = new List<(Guid ProductId, int Quantity, decimal UnitPrice, Guid StockOutId, DateTimeOffset Date)>();

            foreach (var (product, pi) in products.Select((p, i) => (p, i)))
            {
                var unitPrice = 10_000m * (pi + 1);

                for (var j = 0; j < 3; j++)
                {
                    var date = DateTimeOffset.UtcNow.AddDays(-(ProductTarget * 3 - pi * 3 - j) / 2);
                    var so = new StockOut
                    {
                        TenantId = tenantId,
                        ProductId = product.Id,
                        CustomerName = $"Seed Customer {pi + 1}",
                        Quantity = 30,
                        UnitPrice = unitPrice,
                        TotalAmount = 30 * unitPrice,
                        TransactionDate = date,
                        Note = $"Seed stock-out {j + 1} for {product.Code}"
                    };
                    stockOuts.Add(so);
                    outMovements.Add((product.Id, 30, unitPrice, so.Id, date));
                }
            }

            db.StockOuts.AddRange(stockOuts);

            foreach (var (productId, qty, unitPrice, soId, date) in outMovements)
            {
                await stockMovementService.RecordAsync(
                    productId, StockMovementType.Out, StockMovementSource.Manual,
                    qty, unitPrice, date, $"stockout:{soId}",
                    new StockMovementReference { StockOutId = soId }, ct: ct);
            }

            await db.SaveChangesAsync(ct);
            stockOutsCreated = stockOuts.Count;
        }

        return (stockInsCreated, stockOutsCreated);
    }

    private async Task<int> SeedReservationsAsync(Guid tenantId, CancellationToken ct)
    {
        var products = await db.Products.Take(ReservationTarget).ToListAsync(ct);
        if (products.Count == 0)
            return 0;

        var created = 0;
        var now = DateTimeOffset.UtcNow;

        foreach (var (product, i) in products.Select((p, i) => (p, i)))
        {
            var key = $"seed:reservation:{i}";

            var exists = await db.InventoryReservations
                .IgnoreQueryFilters()
                .AnyAsync(r => r.TenantId == tenantId && r.IdempotencyKey == key, ct);

            if (exists)
                continue;

            db.InventoryReservations.Add(new InventoryReservation
            {
                TenantId = tenantId,
                ProductId = product.Id,
                Quantity = 10,
                Status = InventoryReservationStatus.Active,
                ReservedAt = now,
                ExpiresAt = now.AddDays(7),
                IdempotencyKey = key
            });
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        return created;
    }
}
