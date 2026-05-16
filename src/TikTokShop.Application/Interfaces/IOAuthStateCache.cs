namespace TikTokShop.Application.Interfaces;

/// <summary>Data stored per OAuth state token — carries tenant context so the anonymous callback can resolve the tenant.</summary>
public record OAuthStateData(Guid TenantId, string RedirectAfter);

public interface IOAuthStateCache
{
    void Set(string state, OAuthStateData data, TimeSpan expiry);
    bool TryGetAndRemove(string state, out OAuthStateData data);
}
