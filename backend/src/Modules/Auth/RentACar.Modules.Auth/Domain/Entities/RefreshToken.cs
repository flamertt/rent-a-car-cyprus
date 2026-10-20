namespace RentACar.Modules.Auth.Domain.Entities;

/// <summary>
/// Yenileme belirteci (opak, yalnız SHA-256 özeti saklanır). Her yenilemede döndürülür: eski belirteç kapanır,
/// yerine aynı aileden (<see cref="FamilyId"/>, tek bir girişin zinciri) yenisi verilir.
/// </summary>
internal sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Döndürmede yerine verilen belirteç; çıkışta/iptalde boş kalır.</summary>
    public Guid? ReplacedById { get; private set; }

    public static RefreshToken Issue(Guid userId, Guid familyId, string tokenHash, DateTimeOffset now, TimeSpan lifetime) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        FamilyId = familyId,
        TokenHash = tokenHash,
        CreatedAt = now,
        ExpiresAt = now + lifetime,
    };

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    /// <summary>Döndürüldü ve bu, <paramref name="grace"/> içinde oldu (eşzamanlı yenileme; hırsızlık sayılmaz).</summary>
    public bool WasRotatedWithin(TimeSpan grace, DateTimeOffset now) =>
        ReplacedById is not null && RevokedAt is { } revokedAt && now - revokedAt <= grace;

    public void RotateTo(Guid replacementId, DateTimeOffset now)
    {
        RevokedAt ??= now;
        ReplacedById ??= replacementId;
    }

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
