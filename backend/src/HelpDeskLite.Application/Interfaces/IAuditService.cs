namespace HelpDeskLite.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string entityType, string? entityId = null, string? metadata = null, CancellationToken cancellationToken = default);
}
