using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.UnitTests.Auth;

/// <summary>Kimlik doğrulama hattı: admin uçları korumalı, herkese açıklar açık, belirteç üretimi ile doğrulaması uyumlu.</summary>
public sealed class ApiSecurityTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private List<RouteEndpoint> Endpoints =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToList();

    [Fact]
    public void Every_admin_endpoint_requires_an_authenticated_user()
    {
        var admin = Endpoints.Where(e => e.RoutePattern.RawText!.StartsWith("/api/admin/", StringComparison.Ordinal)).ToList();
        Assert.NotEmpty(admin);
        var open = admin
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null || !e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any())
            .Select(e => e.DisplayName)
            .ToList();
        Assert.Empty(open);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/refresh")]
    [InlineData("/api/auth/logout")]
    [InlineData("/api/clock")]
    [InlineData("/api/fleet/categories")]
    [InlineData("/health")]
    public void Public_endpoints_allow_anonymous(string route)
    {
        var matches = Endpoints.Where(e => e.RoutePattern.RawText == route).ToList();
        Assert.NotEmpty(matches);
        // Açıkça işaretli olmalı: işaretsiz uç nokta fallback politikasıyla giriş ister.
        Assert.All(matches, e => Assert.NotNull(e.Metadata.GetMetadata<IAllowAnonymous>()));
    }

    [Fact]
    public async Task Admin_endpoint_without_token_is_401()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/customers", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_401()
    {
        var forged = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "mert-peker-rent-a-car",
            Audience = "mert-peker-rent-a-car-panel",
            Expires = DateTime.UtcNow.AddMinutes(5),
            Claims = new Dictionary<string, object> { [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString() },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("someone-elses-key-that-is-also-32-bytes-long")), SecurityAlgorithms.HmacSha256),
        });
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", forged);

        var response = await client.GetAsync("/api/admin/customers", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Issued_token_passes_bearer_validation_and_expired_one_does_not()
    {
        var user = User.Create("admin@gmail.com", "Yönetici", "hash", DateTimeOffset.UtcNow);
        var issuer = factory.Services.GetRequiredService<IAccessTokenIssuer>();
        var parameters = factory.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        var handler = new JsonWebTokenHandler();

        var valid = await handler.ValidateTokenAsync(issuer.Issue(user, DateTimeOffset.UtcNow).Token, parameters);
        Assert.True(valid.IsValid, valid.Exception?.Message);
        Assert.Equal(user.Id.ToString(), valid.Claims[JwtRegisteredClaimNames.Sub]);

        var expired = await handler.ValidateTokenAsync(issuer.Issue(user, DateTimeOffset.UtcNow.AddHours(-2)).Token, parameters);
        Assert.False(expired.IsValid);
    }

    [Fact]
    public async Task Issued_token_passes_the_admin_authorization_gate()
    {
        var user = User.Create("admin@gmail.com", "Yönetici", "hash", DateTimeOffset.UtcNow);
        var token = factory.Services.GetRequiredService<IAccessTokenIssuer>().Issue(user, DateTimeOffset.UtcNow);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        // Bağlama hatası (page=abc) yetkiden SONRA, işleyiciden (veritabanı) ÖNCE oluşur:
        // 400 gelmesi belirtecin kabul edildiğini gösterir; belirteçsiz aynı istek 401 alır.
        var response = await client.GetAsync("/api/admin/customers?page=abc", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_without_cookie_is_401_with_code()
    {
        var response = await factory.CreateClient().PostAsync("/api/auth/refresh", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.no_session", await CodeOf(response));
    }

    [Fact]
    public async Task Login_with_empty_credentials_is_401_with_code()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email = "", password = "" }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.invalid_credentials", await CodeOf(response));
    }

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
