using TaskForge.Api.Models;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task<IEnumerable<RefreshToken>> GetUserTokensAsync(int userId);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
}