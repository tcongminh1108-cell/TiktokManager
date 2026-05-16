using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

// Per-shop token-bucket rate limiter (50 requests/second per shop_cipher).
// TikTok enforces this at the shop level, not app level.
public interface ITikTokRateLimiter : IAsyncDisposable
{
    ValueTask AcquireAsync(string shopCipher, CancellationToken ct = default);
}

public sealed class TikTokRateLimiter : ITikTokRateLimiter
{
    private const int TOKENS_PER_SECOND = 50;

    // Lazily create one limiter per shop; dispose when the singleton is torn down.
    private readonly ConcurrentDictionary<string, TokenBucketRateLimiter> _limiters = new();

    public ValueTask AcquireAsync(string shopCipher, CancellationToken ct = default)
    {
        var limiter = _limiters.GetOrAdd(shopCipher, static _ =>
            new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = TOKENS_PER_SECOND,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 200,           // max queued requests per shop
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = TOKENS_PER_SECOND,
                AutoReplenishment = true
            }));

        return AcquireLeaseAsync(limiter, ct);
    }

    private static async ValueTask AcquireLeaseAsync(TokenBucketRateLimiter limiter, CancellationToken ct)
    {
        using var lease = await limiter.AcquireAsync(permitCount: 1, ct);
        if (!lease.IsAcquired)
            throw new InvalidOperationException("TikTok rate limiter queue is full — too many concurrent requests.");
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var limiter in _limiters.Values)
            await limiter.DisposeAsync();
        _limiters.Clear();
    }
}
