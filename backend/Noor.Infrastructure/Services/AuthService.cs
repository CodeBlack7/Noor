using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Core.Options;
using Noor.Core.Security;
using Noor.Infrastructure.Data;

namespace Noor.Infrastructure.Services;

public class AuthService
{
    private readonly NoorDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwt;

    public AuthService(NoorDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService,
        IOptions<JwtOptions> jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ip = null)
    {
        var email = NormalizeEmail(request.Email);
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Roles.Customer,
            MarketingOptIn = request.MarketingOptIn,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return await IssueTokensAsync(user, ip);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ip = null)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Email or password is incorrect.");
        }
        if (!user.IsActive)
        {
            throw new AuthenticationException("This account has been deactivated.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await IssueTokensAsync(user, ip);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string? ip = null)
    {
        var hash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AuthenticationException("Session expired. Please sign in again.");
        }
        if (!stored.User.IsActive)
        {
            throw new AuthenticationException("This account has been deactivated.");
        }

        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await IssueTokensAsync(stored.User, ip);
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        var hash = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.UserId == userId);
        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new NotFoundException("Account not found.");
        user.FullName = request.FullName.Trim();
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.MarketingOptIn = request.MarketingOptIn;
        await _db.SaveChangesAsync();
        return DtoMapper.ToUserDto(user);
    }

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, string? ip = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new NotFoundException("Account not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationFailedException("Your current password is incorrect.");
        }
        if (request.CurrentPassword == request.NewPassword)
        {
            throw new ValidationFailedException("Your new password must be different from the current one.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return await IssueTokensAsync(user, ip);
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new NotFoundException("Account not found.");
        return DtoMapper.ToUserDto(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, string? ip)
    {
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip
        });
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = DtoMapper.ToUserDto(user)
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
