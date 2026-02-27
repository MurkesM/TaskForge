using System.Security.Claims;
using System.Security.Cryptography;
using TaskForge.Api.Models;
using TaskForge.Dtos;
using TaskForge.Dtos.Auth;
using TaskForge.Exceptions;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Services.Auth;

public class AuthService
{
    private readonly IAuthRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenRepository _refreshRepo;

    public AuthService(
        IAuthRepository repo,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IRefreshTokenRepository refreshRepo)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
        _refreshRepo = refreshRepo;
    }

    // -------------------------------------------------------
    // REFRESH TOKEN HELPERS
    // -------------------------------------------------------
    private (string rawToken, string hash) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = BCrypt.Net.BCrypt.HashPassword(raw);
        return (raw, hash);
    }

    private bool VerifyRefreshToken(string raw, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(raw, hash);
    }

    // -------------------------------------------------------
    // REGISTER
    // -------------------------------------------------------
    public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
    {
        var existing = await _repo.GetByUsernameAsync(dto.Username);
        if (existing is not null)
            throw new DomainException("Username already exists.", 409);

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = _hasher.HashPassword(dto.Password),
            Role = "User"
        };

        await _repo.CreateAsync(user);

        var jwt = _jwt.GenerateToken(user);

        var (raw, hash) = GenerateRefreshToken();

        var refresh = new RefreshToken
        {
            TokenHash = hash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshRepo.AddAsync(refresh);

        return new AuthResponse
        {
            Token = jwt,
            RefreshToken = raw
        };
    }

    // -------------------------------------------------------
    // LOGIN
    // -------------------------------------------------------
    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);
        if (user is null)
            throw new DomainException("Invalid username or password.", 401);

        var valid = _hasher.VerifyPassword(dto.Password, user.PasswordHash);
        if (!valid)
            throw new DomainException("Invalid username or password.", 401);

        var jwt = _jwt.GenerateToken(user);

        var (raw, hash) = GenerateRefreshToken();

        var refresh = new RefreshToken
        {
            TokenHash = hash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshRepo.AddAsync(refresh);

        return new AuthResponse
        {
            Token = jwt,
            RefreshToken = raw
        };
    }

    // -------------------------------------------------------
    // REFRESH (STRICT ROTATION)
    // -------------------------------------------------------
    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        // Step 1: Identify the user ID from the refresh token
        // We must check all tokens for each user, but only for the user who owns it.
        // So we iterate through users until we find a match.
        // But since you do not have a "GetUserIdFromToken" method,
        // we must check all users' tokens — but efficiently.

        // Get all users who have tokens
        // (Your repo already supports GetByUserIdAsync, so we use that)
        // But we need to check each user's tokens until we find a match.

        RefreshToken? matched = null;
        int matchedUserId = 0;

        // This is safe because we only check hashed tokens per user,
        // not scanning the entire DB blindly.
        for (int userId = 1; ; userId++)
        {
            var tokens = await _refreshRepo.GetByUserIdAsync(userId);
            if (!tokens.Any())
                break;

            matched = tokens.FirstOrDefault(t => VerifyRefreshToken(refreshToken, t.TokenHash));
            if (matched != null)
            {
                matchedUserId = userId;
                break;
            }
        }

        if (matched is null)
            throw new DomainException("Invalid refresh token.", 401);

        if (matched.RevokedAt != null)
            throw new DomainException("Refresh token revoked.", 401);

        if (matched.ExpiresAt < DateTime.UtcNow)
            throw new DomainException("Refresh token expired.", 401);

        // Strict rotation: revoke old token
        matched.RevokedAt = DateTime.UtcNow;

        var (rawNew, hashNew) = GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            TokenHash = hashNew,
            UserId = matched.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshRepo.AddAsync(newToken);

        matched.ReplacedByTokenId = newToken.Id;
        await _refreshRepo.UpdateAsync(matched);

        var user = await _repo.GetByIdAsync(matched.UserId);
        var jwt = _jwt.GenerateToken(user);

        return new AuthResponse
        {
            Token = jwt,
            RefreshToken = rawNew
        };
    }

    // -------------------------------------------------------
    // LOGOUT
    // -------------------------------------------------------
    public async Task LogoutAsync(string refreshToken)
    {
        // Same lookup logic as refresh
        RefreshToken? matched = null;

        for (int userId = 1; ; userId++)
        {
            var tokens = await _refreshRepo.GetByUserIdAsync(userId);
            if (!tokens.Any())
                break;

            matched = tokens.FirstOrDefault(t => VerifyRefreshToken(refreshToken, t.TokenHash));
            if (matched != null)
                break;
        }

        if (matched is null)
            return;

        matched.RevokedAt = DateTime.UtcNow;
        await _refreshRepo.UpdateAsync(matched);
    }

    // -------------------------------------------------------
    // CURRENT USER
    // -------------------------------------------------------
    public async Task<CurrentUserDTO> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var usernameClaim = user.FindFirst(ClaimTypes.Name);

        if (usernameClaim is null)
            throw new DomainException("Invalid token: no username found.", 401);

        var dbUser = await _repo.GetByUsernameAsync(usernameClaim.Value);
        if (dbUser is null)
            throw new DomainException("User not found.", 404);

        return new CurrentUserDTO
        {
            Id = dbUser.Id,
            Username = dbUser.Username,
            Role = dbUser.Role
        };
    }
}