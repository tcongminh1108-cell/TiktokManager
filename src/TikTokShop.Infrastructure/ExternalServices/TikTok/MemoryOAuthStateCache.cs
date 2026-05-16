using Microsoft.Extensions.Caching.Memory;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class MemoryOAuthStateCache(IMemoryCache cache) : IOAuthStateCache
{
    public void Set(string state, string redirectAfter, TimeSpan expiry) =>
        cache.Set(state, redirectAfter, expiry);

    public bool TryGetAndRemove(string state, out string redirectAfter)
    {
        if (cache.TryGetValue(state, out string? value) && value is not null)
        {
            cache.Remove(state);
            redirectAfter = value;
            return true;
        }
        redirectAfter = string.Empty;
        return false;
    }
}
