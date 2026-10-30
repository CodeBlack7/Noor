using Noor.Core.Entities;

namespace Noor.Core.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}
