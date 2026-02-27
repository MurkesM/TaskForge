using TaskForge.Api.Models;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
}
