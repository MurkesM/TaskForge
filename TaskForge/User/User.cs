namespace TaskForge.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // Stored as a hashed value
    public string PasswordHash { get; set; } = string.Empty;

    // Optional: roles for authorization
    public string Role { get; set; } = "User";
}