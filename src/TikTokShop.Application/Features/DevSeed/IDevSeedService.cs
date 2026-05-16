namespace TikTokShop.Application.Features.DevSeed;

public interface IDevSeedService
{
    Task<DevSeedResult> SeedAsync(CancellationToken ct = default);
}

public record DevSeedResult(
    int SuppliersCreated,
    int ProductsCreated,
    int StockInsCreated,
    int StockOutsCreated,
    int ReservationsCreated
);
