using TaskForge.Models;

namespace TaskForge.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }

    // The hashed token value (never store raw tokens)
    public string TokenHash { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    // Null unless revoked
    public DateTime? RevokedAt { get; set; }

    // If this token was rotated, store the new token's ID
    public int? ReplacedByTokenId { get; set; }

    // Optional but useful for session management
    public string? Device { get; set; }
    public string? IpAddress { get; set; }

    // Navigation
    public User? User { get; set; }
}