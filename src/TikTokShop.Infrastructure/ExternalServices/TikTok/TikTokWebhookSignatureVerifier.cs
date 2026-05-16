using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

// Verifies the HMAC-SHA256 signature on incoming TikTok webhook requests.
// Formula (per TikTok docs §12.5.6): HMAC-SHA256(app_secret, request_url + raw_body)
public sealed class TikTokWebhookSignatureVerifier(IOptions<TikTokSettings> options) : ITikTokWebhookSignatureVerifier
{
    private readonly string _appSecret = options.Value.AppSecret;

    public bool Verify(string requestUrl, string rawBody, string authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
            return false;

        var canonicalString = requestUrl + rawBody;
        var expected = ComputeHmacSha256(_appSecret, canonicalString);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(authorizationHeader),
            Encoding.UTF8.GetBytes(expected));
    }

    private static string ComputeHmacSha256(string secret, string message)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var msgBytes = Encoding.UTF8.GetBytes(message);
        var hash = HMACSHA256.HashData(keyBytes, msgBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
