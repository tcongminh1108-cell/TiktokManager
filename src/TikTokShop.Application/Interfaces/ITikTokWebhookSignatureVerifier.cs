namespace TikTokShop.Application.Interfaces;

// Abstracts TikTok HMAC-SHA256 signature verification without importing ASP.NET Core types.
public interface ITikTokWebhookSignatureVerifier
{
    bool Verify(string requestUrl, string rawBody, string authorizationHeader);
}
