using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Modules.Auth.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.UnitTests.Auth;

public sealed class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Email_is_normalized_and_must_look_like_an_address()
    {
        var user = User.Create("  Admin@Gmail.COM ", "Yönetici", "hash", Now);
        Assert.Equal("admin@gmail.com", user.Email);
        Assert.True(user.IsActive);

        var ex = Assert.Throws<DomainException>(() => User.Create("admin", "Yönetici", "hash", Now));
        Assert.Equal("auth.invalid_email", ex.Code);
    }

    [Fact]
    public void Account_locks_after_max_failed_attempts_and_unlocks_after_lockout()
    {
        var user = User.Create("admin@gmail.com", "Yönetici", "hash", Now);
        for (var i = 0; i < LoginRules.MaxFailedAttempts - 1; i++)
        {
            user.RecordFailedLogin(Now);
        }

        Assert.False(user.IsLockedOut(Now));

        user.RecordFailedLogin(Now);
        Assert.True(user.IsLockedOut(Now));
        Assert.True(user.IsLockedOut(Now + LoginRules.LockoutDuration - TimeSpan.FromSeconds(1)));
        Assert.False(user.IsLockedOut(Now + LoginRules.LockoutDuration));
    }

    [Fact]
    public void Successful_login_resets_failed_attempts()
    {
        var user = User.Create("admin@gmail.com", "Yönetici", "hash", Now);
        user.RecordFailedLogin(Now);
        user.RecordFailedLogin(Now);
        user.RecordLogin(Now);

        Assert.Equal(0, user.FailedLoginCount);
        Assert.Equal(Now, user.LastLoginAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kisa123")]
    public void Short_passwords_are_rejected(string? password)
    {
        var ex = Assert.Throws<DomainException>(() => LoginRules.EnsurePasswordIsAcceptable(password));
        Assert.Equal("auth.weak_password", ex.Code);
    }

    [Fact]
    public void Default_admin_password_satisfies_policy() => LoginRules.EnsurePasswordIsAcceptable("password123");
}
