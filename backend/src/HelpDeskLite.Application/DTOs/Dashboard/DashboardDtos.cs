using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.DTOs.Dashboard;

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardActivityDto
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

public class AssigneeWorkloadDto
{
    public Guid? AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public int OpenTicketCount { get; set; }
}

public class ResolutionTrendPointDto
{
    public DateOnly Date { get; set; }
    public int ResolvedCount { get; set; }
}

public class AgingBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EmployeeDashboardDto
{
    public int OpenTicketsCount { get; set; }
    public int TotalTicketsCount { get; set; }
    public IReadOnlyList<StatusCountDto> StatusOverview { get; set; } = [];
    public IReadOnlyList<TicketListItemDto> OpenTickets { get; set; } = [];
    public IReadOnlyList<DashboardActivityDto> RecentActivity { get; set; } = [];
}

public class SupportQueueDashboardDto
{
    public int OpenTicketsCount { get; set; }
    public int AssignedToMeCount { get; set; }
    public int UnassignedCount { get; set; }
    public int CriticalOpenCount { get; set; }
    public int DelayedCount { get; set; }
    public IReadOnlyList<StatusCountDto> StatusOverview { get; set; } = [];
}

public class ManagerDashboardDto
{
    public int OpenTicketsCount { get; set; }
    public int ResolvedTicketsCount { get; set; }
    public int ClosedTicketsCount { get; set; }
    public int DelayedTicketsCount { get; set; }
    public IReadOnlyList<StatusCountDto> StatusDistribution { get; set; } = [];
    public IReadOnlyList<AssigneeWorkloadDto> TeamWorkload { get; set; } = [];
    public IReadOnlyList<ResolutionTrendPointDto> ResolutionTrend { get; set; } = [];
    public IReadOnlyList<AgingBucketDto> AgingBuckets { get; set; } = [];
    public IReadOnlyList<TicketListItemDto> DelayedTickets { get; set; } = [];
}

public class WorkloadReportDto
{
    public IReadOnlyList<AssigneeWorkloadDto> ByAssignee { get; set; } = [];
    public IReadOnlyList<StatusCountDto> ByStatus { get; set; } = [];
}

public class TicketAgingReportDto
{
    public int DelayedTicketCount { get; set; }
    public IReadOnlyList<AgingBucketDto> Buckets { get; set; } = [];
    public IReadOnlyList<TicketListItemDto> DelayedTickets { get; set; } = [];
}
