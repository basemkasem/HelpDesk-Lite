using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.Interfaces;

public enum TicketQueryScope
{
    OwnTickets,
    AllTickets,
    AssignedToCurrentUser
}

public class TicketQueryFilter
{
    public TicketQueryScope Scope { get; set; }
    public Guid CurrentUserId { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AssigneeId { get; set; }
    public bool? AssignedToMe { get; set; }
    public bool? UnassignedOnly { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool? OpenOnly { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TicketQueryResult
{
    public IReadOnlyList<Ticket> Tickets { get; set; } = [];
    public int TotalCount { get; set; }
}

public interface ITicketQueryRepository
{
    Task<TicketQueryResult> QueryAsync(TicketQueryFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusCountProjection>> GetStatusCountsAsync(TicketQueryScope scope, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResolutionTrendProjection>> GetResolutionTrendAsync(int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardActivityProjection>> GetRecentActivityForUserAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
}

public class StatusCountProjection
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AssigneeWorkloadProjection
{
    public Guid? AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ResolutionTrendProjection
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class DashboardActivityProjection
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string TicketTitle { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public DateTime OccurredAt { get; set; }
}
