using RentACar.Modules.Auth.Infrastructure.Security;

namespace RentACar.UnitTests.Auth;

public sealed class PasswordHasherTests
{
    private readonly IdentityPasswordHasher _hasher = new();

    [Fact]
    public void Verifies_the_right_password_only()
    {
        var hash = _hasher.Hash("password123");
        Assert.NotEqual("password123", hash);
        Assert.True(_hasher.Verify(hash, "password123", out var rehash));
        Assert.False(rehash);
        Assert.False(_hasher.Verify(hash, "Password123", out _));
    }

    [Fact]
    public void Same_password_gets_a_different_salted_hash() =>
        Assert.NotEqual(_hasher.Hash("password123"), _hasher.Hash("password123"));

    [Fact]
    public void Missing_user_never_verifies() => Assert.False(_hasher.Verify(null, "password123", out _));
}
