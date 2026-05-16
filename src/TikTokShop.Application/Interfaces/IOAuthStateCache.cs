namespace TikTokShop.Application.Interfaces;

public interface IOAuthStateCache
{
    void Set(string state, string redirectAfter, TimeSpan expiry);
    bool TryGetAndRemove(string state, out string redirectAfter);
}
