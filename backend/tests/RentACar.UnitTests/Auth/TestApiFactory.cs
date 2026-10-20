using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace RentACar.UnitTests.Auth;

/// <summary>
/// Veritabanına dokunmadan ayağa kalkan API: migration/tohum yok, arka plan işleri (simülasyon, taramalar) kaldırılmış.
/// Yalnız kimlik doğrulama/yetki hattını sınamak için.
/// </summary>
public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "test-signing-key-that-is-long-enough-for-hs256";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", "Host=127.0.0.1;Port=1;Database=unused");
        builder.UseSetting("Database:AutoMigrate", "false");
        builder.UseSetting("Auth:SigningKey", SigningKey);
        builder.ConfigureTestServices(services => services.RemoveAll<IHostedService>());
    }
}
