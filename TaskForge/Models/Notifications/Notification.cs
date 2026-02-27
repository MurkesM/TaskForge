namespace TaskForge.Api.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? Metadata { get; set; }
}