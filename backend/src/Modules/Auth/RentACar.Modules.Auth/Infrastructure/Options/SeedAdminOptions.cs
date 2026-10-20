namespace RentACar.Modules.Auth.Infrastructure.Options;

/// <summary>appsettings: <c>Auth:SeedAdmin</c>. Hiç kullanıcı yoksa açılışta bu yönetici oluşturulur.</summary>
internal sealed class SeedAdminOptions
{
    public const string SectionName = "Auth:SeedAdmin";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = "Yönetici";
}
