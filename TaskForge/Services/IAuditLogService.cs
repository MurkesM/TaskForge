using TaskForge.Api.Models;
using TaskForge.Dtos;

public interface IAuditLogService
{
    Task LogAsync(AuditLog log);

    Task<CursorResultDto<AuditLog>> QueryAsync(int userId, AuditQueryParameters query);

}