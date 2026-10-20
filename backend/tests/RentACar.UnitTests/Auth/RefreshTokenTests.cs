using RentACar.Modules.Auth.Application.Services;
using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Modules.Auth.Domain.Rules;

namespace RentACar.UnitTests.Auth;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Token_is_active_until_expiry()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "h", Now, TimeSpan.FromDays(7));
        Assert.True(token.IsActive(Now.AddDays(7).AddSeconds(-1)));
        Assert.False(token.IsActive(Now.AddDays(7)));
    }

    [Fact]
    public void Rotation_closes_the_token_and_links_the_replacement()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "h", Now, TimeSpan.FromDays(7));
        var replacement = Guid.NewGuid();
        token.RotateTo(replacement, Now);

        Assert.False(token.IsActive(Now));
        Assert.Equal(replacement, token.ReplacedById);
        Assert.True(token.WasRotatedWithin(LoginRules.RefreshReuseGrace, Now + LoginRules.RefreshReuseGrace));
        Assert.False(token.WasRotatedWithin(LoginRules.RefreshReuseGrace, Now + LoginRules.RefreshReuseGrace + TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Revoked_token_is_not_a_rotation()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), Guid.NewGuid(), "h", Now, TimeSpan.FromDays(7));
        token.Revoke(Now);
        Assert.False(token.WasRotatedWithin(LoginRules.RefreshReuseGrace, Now));
    }

    [Fact]
    public void Codec_generates_unique_tokens_and_stable_hashes()
    {
        var a = RefreshTokenCodec.Generate();
        var b = RefreshTokenCodec.Generate();
        Assert.NotEqual(a, b);
        Assert.Equal(RefreshTokenCodec.Hash(a), RefreshTokenCodec.Hash(a));
        Assert.NotEqual(RefreshTokenCodec.Hash(a), RefreshTokenCodec.Hash(b));
        Assert.True(RefreshTokenCodec.Hash(a).Length <= 64);
    }
}
