using Microsoft.Extensions.Options;
using Noor.Core.Entities;
using Noor.Core.Options;
using Noor.Infrastructure.Services;

namespace Noor.Tests;

public class SecurityTests
{
    private static readonly Pbkdf2PasswordHasher Hasher = new();

    [Fact]
    public void PasswordHasher_RoundTripsPassword()
    {
        var hash = Hasher.Hash("Atelier@2026");

        Assert.StartsWith("noor.v1.", hash);
        Assert.True(Hasher.Verify("Atelier@2026", hash));
        Assert.False(Hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void PasswordHasher_UsesRandomSalt()
    {
        Assert.NotEqual(Hasher.Hash("same-password"), Hasher.Hash("same-password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("noor.v1.120000.invalid.invalid")]
    public void PasswordHasher_RejectsMalformedHashes(string hash)
    {
        Assert.False(Hasher.Verify("anything", hash));
    }

    [Fact]
    public void JwtTokenService_IssuesRoleBearingToken()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Secret = "noor-test-secret-that-is-long-enough-for-hmac-sha256",
            Issuer = "Noor.Tests",
            Audience = "Noor.Tests.Client",
            AccessTokenMinutes = 5
        }));
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@noor.store",
            FullName = "Noor Atelier",
            Role = Roles.Admin
        };

        var (token, expiresAt) = service.CreateAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void RefreshTokens_AreUniqueAndHashedDeterministically()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Secret = "noor-test-secret-that-is-long-enough-for-hmac-sha256"
        }));

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.Equal(service.HashRefreshToken(first), service.HashRefreshToken(first));
        Assert.NotEqual(service.HashRefreshToken(first), service.HashRefreshToken(second));
    }
}
