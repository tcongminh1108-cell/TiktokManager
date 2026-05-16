using Microsoft.Extensions.Caching.Memory;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class MemoryOAuthStateCache(IMemoryCache cache) : IOAuthStateCache
{
    public void Set(string state, OAuthStateData data, TimeSpan expiry) =>
        cache.Set(state, data, expiry);

    public bool TryGetAndRemove(string state, out OAuthStateData data)
    {
        if (cache.TryGetValue(state, out OAuthStateData? value) && value is not null)
        {
            cache.Remove(state);
            data = value;
            return true;
        }
        data = default!;
        return false;
    }
}
