using TaskForge.Models;

namespace TaskForge.Repositories;

public interface IAuthRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
}
