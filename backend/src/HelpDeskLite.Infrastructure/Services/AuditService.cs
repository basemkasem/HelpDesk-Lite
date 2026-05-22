using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Infrastructure.Data;

namespace HelpDeskLite.Infrastructure.Services;

public class AuditService(AppDbContext context) : IAuditService
{
    public async Task LogAsync(Guid? userId, string action, string entityType, string? entityId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
