using TaskForge.Api.Models;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IAuditLogRepository repo)
    {
        _repo = repo;
    }

    public Task LogAsync(AuditLog log)
    {
        return _repo.AddAsync(log);
    }
}