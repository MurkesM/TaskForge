using System.Security.Claims;
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

    public AuthService(IAuthRepository repo, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
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

        // NEW: return a JWT immediately after registration
        return _jwt.GenerateToken(user);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _repo.GetByUsernameAsync(dto.Username);
        if (user is null)
            throw new DomainException("Invalid username or password.", 401);

        var valid = _hasher.VerifyPassword(dto.Password, user.PasswordHash);
        if (!valid)
            throw new DomainException("Invalid username or password.", 401);

        return _jwt.GenerateToken(user);
    }

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