using Microsoft.AspNetCore.DataProtection;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class TikTokTokenProtector(IDataProtectionProvider dataProtectionProvider) : ITikTokTokenProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("TikTokShop.Tokens");

    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
