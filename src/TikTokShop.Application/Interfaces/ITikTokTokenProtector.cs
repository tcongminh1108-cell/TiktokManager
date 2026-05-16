namespace TikTokShop.Application.Interfaces;

public interface ITikTokTokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
