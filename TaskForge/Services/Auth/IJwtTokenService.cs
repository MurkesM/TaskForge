using TaskForge.Models;

namespace TaskForge.Services.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
