using TaskForge.Api.Models;

public interface IAuditLogService
{
    Task LogAsync(AuditLog log);
}
