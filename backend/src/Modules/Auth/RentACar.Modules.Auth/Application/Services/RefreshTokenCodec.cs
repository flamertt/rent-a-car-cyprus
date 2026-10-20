using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace RentACar.Modules.Auth.Application.Services;

/// <summary>Opak yenileme belirteci: 256 bit rastgele değer (base64url); veritabanında yalnız SHA-256 özeti.</summary>
internal static class RefreshTokenCodec
{
    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    public static string Hash(string token) => Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
