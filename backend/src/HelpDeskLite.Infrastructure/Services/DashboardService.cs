using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.DTOs.Dashboard;
using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Enums;
using HelpDeskLite.Domain.Exceptions;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Services;

public class DashboardService(
    ITicketQueryRepository ticketQueryRepository,
    ITicketService ticketService,
    ICurrentUserService currentUserService,
    AppDbContext context) : IDashboardService
{
    private const int DelayedDaysThreshold = 3;
    private const int TrendDays = 7;

    private static readonly string[] OpenStatuses =
    [
        nameof(TicketStatus.New),
        nameof(TicketStatus.Assigned),
        nameof(TicketStatus.InProgress),
        nameof(TicketStatus.WaitingForUser)
    ];

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (userId, _) = RequireUser();
        RequireEmployeeOrHigher();

        var statusOverview = await ticketQueryRepository.GetStatusCountsAsync(
            TicketQueryScope.OwnTickets, userId, cancellationToken);

        var openFilter = new TicketQueryFilter
        {
            Scope = TicketQueryScope.OwnTickets,
            CurrentUserId = userId,
            OpenOnly = true,
            Page = 1,
            PageSize = 8,
            SortBy = "createdAt",
            SortDirection = "desc"
        };

        var openQuery = await ticketQueryRepository.QueryAsync(openFilter, cancellationToken);
        var openTickets = openQuery.Tickets.Select(MapListItem).ToList();
        var openCount = statusOverview.Where(s => OpenStatuses.Contains(s.Status)).Sum(s => s.Count);

        var recentActivity = await ticketQueryRepository.GetRecentActivityForUserAsync(userId, 10, cancellationToken);

        return new EmployeeDashboardDto
        {
            OpenTicketsCount = openCount,
            TotalTicketsCount = statusOverview.Sum(s => s.Count),
            StatusOverview = statusOverview.Select(s => new StatusCountDto { Status = s.Status, Count = s.Count }).ToList(),
            OpenTickets = openTickets,
            RecentActivity = recentActivity.Select(MapActivity).ToList()
        };
    }

    public async Task<SupportQueueDashboardDto> GetSupportQueueDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (userId, _) = RequireUser();
        RequireStaff();

        var statusOverview = await ticketQueryRepository.GetStatusCountsAsync(
            TicketQueryScope.AllTickets, userId, cancellationToken);

        var allOpen = await context.Tickets.AsNoTracking()
            .Where(t => OpenStatuses.Contains(t.Status))
            .Select(t => new { t.AssigneeId, t.Priority, t.CreatedAt })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var delayedThreshold = now.AddDays(-DelayedDaysThreshold);

        return new SupportQueueDashboardDto
        {
            OpenTicketsCount = allOpen.Count,
            AssignedToMeCount = allOpen.Count(t => t.AssigneeId == userId),
            UnassignedCount = allOpen.Count(t => t.AssigneeId == null),
            CriticalOpenCount = allOpen.Count(t => t.Priority == TicketPriority.Critical),
            DelayedCount = allOpen.Count(t => t.CreatedAt < delayedThreshold),
            StatusOverview = statusOverview.Select(s => new StatusCountDto { Status = s.Status, Count = s.Count }).ToList()
        };
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync(CancellationToken cancellationToken = default)
    {
        RequireManager();

        var statusDistribution = await ticketQueryRepository.GetStatusCountsAsync(
            TicketQueryScope.AllTickets, Guid.Empty, cancellationToken);

        var teamWorkloadRaw = await GetWorkloadByAssigneeAsync(cancellationToken);
        var resolutionTrend = await ticketQueryRepository.GetResolutionTrendAsync(TrendDays, cancellationToken);
        var aging = await BuildAgingReportAsync(cancellationToken);

        var openCount = statusDistribution.Where(s => OpenStatuses.Contains(s.Status)).Sum(s => s.Count);
        var resolvedCount = statusDistribution
            .FirstOrDefault(s => s.Status == nameof(TicketStatus.Resolved))?.Count ?? 0;
        var closedCount = statusDistribution
            .FirstOrDefault(s => s.Status == nameof(TicketStatus.Closed))?.Count ?? 0;

        return new ManagerDashboardDto
        {
            OpenTicketsCount = openCount,
            ResolvedTicketsCount = resolvedCount,
            ClosedTicketsCount = closedCount,
            DelayedTicketsCount = aging.DelayedTicketCount,
            StatusDistribution = statusDistribution.Select(s => new StatusCountDto { Status = s.Status, Count = s.Count }).ToList(),
            TeamWorkload = teamWorkloadRaw,
            ResolutionTrend = resolutionTrend.Select(r => new ResolutionTrendPointDto
            {
                Date = r.Date,
                ResolvedCount = r.Count
            }).ToList(),
            AgingBuckets = aging.Buckets,
            DelayedTickets = aging.DelayedTickets
        };
    }

    public async Task<PagedResultDto<TicketListItemDto>> SearchTicketsAsync(
        TicketQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (userId, role) = RequireUser();
        var filter = BuildFilter(parameters, userId, role);

        if (role == UserRole.Employee)
        {
            filter.Scope = TicketQueryScope.OwnTickets;
        }
        else if (parameters.AssignedToMe == true)
        {
            filter.Scope = TicketQueryScope.AssignedToCurrentUser;
        }
        else
        {
            filter.Scope = TicketQueryScope.AllTickets;
        }

        var result = await ticketQueryRepository.QueryAsync(filter, cancellationToken);

        return new PagedResultDto<TicketListItemDto>
        {
            Items = result.Tickets.Select(MapListItem).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<WorkloadReportDto> GetWorkloadReportAsync(CancellationToken cancellationToken = default)
    {
        RequireStaff();

        var byAssignee = await GetWorkloadByAssigneeAsync(cancellationToken);
        var byStatus = await ticketQueryRepository.GetStatusCountsAsync(
            TicketQueryScope.AllTickets, Guid.Empty, cancellationToken);

        return new WorkloadReportDto
        {
            ByAssignee = byAssignee,
            ByStatus = byStatus.Select(s => new StatusCountDto { Status = s.Status, Count = s.Count }).ToList()
        };
    }

    public async Task<TicketAgingReportDto> GetTicketAgingReportAsync(CancellationToken cancellationToken = default)
    {
        RequireStaff();
        return await BuildAgingReportAsync(cancellationToken);
    }

    public async Task<int> BulkAssignTicketsAsync(
        BulkAssignTicketsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        RequireStaff();

        if (request.TicketIds.Count == 0)
        {
            throw new BadRequestException("Select at least one ticket.");
        }

        var updated = 0;
        foreach (var ticketId in request.TicketIds.Distinct())
        {
            await ticketService.AssignTicketAsync(
                ticketId,
                new AssignTicketRequestDto { AssigneeId = request.AssigneeId },
                cancellationToken);
            updated++;
        }

        return updated;
    }

    private async Task<IReadOnlyList<AssigneeWorkloadDto>> GetWorkloadByAssigneeAsync(CancellationToken cancellationToken)
    {
        var rows = await (
            from t in context.Tickets.AsNoTracking()
            where OpenStatuses.Contains(t.Status)
            join u in context.Users.AsNoTracking() on t.AssigneeId equals u.Id into assignees
            from u in assignees.DefaultIfEmpty()
            group t by new { t.AssigneeId, Name = u != null ? u.FullName : "Unassigned" } into g
            orderby g.Count() descending
            select new AssigneeWorkloadDto
            {
                AssigneeId = g.Key.AssigneeId,
                AssigneeName = g.Key.Name,
                OpenTicketCount = g.Count()
            }).ToListAsync(cancellationToken);

        return rows;
    }

    private async Task<TicketAgingReportDto> BuildAgingReportAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var openTickets = await context.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedByUser)
            .Where(t => OpenStatuses.Contains(t.Status))
            .ToListAsync(cancellationToken);

        var buckets = new List<AgingBucketDto>
        {
            new() { Label = "0–1 days", Count = 0 },
            new() { Label = "1–3 days", Count = 0 },
            new() { Label = "3–7 days", Count = 0 },
            new() { Label = "7+ days", Count = 0 }
        };

        foreach (var ticket in openTickets)
        {
            var age = (int)(now - ticket.CreatedAt).TotalDays;
            if (age <= 1)
            {
                buckets[0].Count++;
            }
            else if (age <= 3)
            {
                buckets[1].Count++;
            }
            else if (age <= 7)
            {
                buckets[2].Count++;
            }
            else
            {
                buckets[3].Count++;
            }
        }

        var allDelayed = openTickets
            .Where(t => t.CreatedAt < now.AddDays(-DelayedDaysThreshold))
            .OrderBy(t => t.CreatedAt)
            .Select(MapListItem)
            .ToList();

        var delayed = allDelayed.Take(20).ToList();

        return new TicketAgingReportDto
        {
            DelayedTicketCount = allDelayed.Count,
            Buckets = buckets,
            DelayedTickets = delayed
        };
    }

    private static TicketQueryFilter BuildFilter(TicketQueryParameters p, Guid userId, UserRole role)
    {
        return new TicketQueryFilter
        {
            CurrentUserId = userId,
            Search = p.Search,
            Status = p.Status,
            Priority = p.Priority,
            CategoryId = p.CategoryId,
            AssigneeId = role == UserRole.Employee ? null : p.AssigneeId,
            AssignedToMe = role == UserRole.Employee ? null : p.AssignedToMe,
            UnassignedOnly = role == UserRole.Employee ? null : p.UnassignedOnly,
            CreatedFrom = p.CreatedFrom,
            CreatedTo = p.CreatedTo,
            SortBy = p.SortBy,
            SortDirection = p.SortDirection,
            Page = Math.Max(1, p.Page),
            PageSize = Math.Clamp(p.PageSize, 1, 100)
        };
    }

    private static TicketListItemDto MapListItem(Domain.Entities.Ticket ticket)
    {
        var now = DateTime.UtcNow;
        var ageInDays = (int)(now - ticket.CreatedAt).TotalDays;
        var isOpen = OpenStatuses.Contains(ticket.Status);
        var isDelayed = isOpen && ticket.CreatedAt < now.AddDays(-DelayedDaysThreshold);

        return new TicketListItemDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? string.Empty,
            Priority = ticket.Priority,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByName = ticket.CreatedByUser?.FullName ?? string.Empty,
            Status = ticket.Status,
            AssigneeId = ticket.AssigneeId,
            AssigneeName = ticket.Assignee?.FullName,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            AgeInDays = ageInDays,
            IsDelayed = isDelayed
        };
    }

    private static DashboardActivityDto MapActivity(DashboardActivityProjection a) => new()
    {
        Id = a.Id,
        TicketId = a.TicketId,
        TicketNumber = a.TicketNumber,
        TicketTitle = a.TicketTitle,
        ActivityType = a.ActivityType,
        Summary = a.Summary,
        ActorName = a.ActorName,
        OccurredAt = a.OccurredAt
    };

    private (Guid UserId, UserRole Role) RequireUser()
    {
        if (!currentUserService.UserId.HasValue || !currentUserService.Role.HasValue)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        return (currentUserService.UserId.Value, currentUserService.Role.Value);
    }

    private void RequireStaff()
    {
        var role = currentUserService.Role;
        if (role is not (UserRole.SupportAgent or UserRole.ManagerAdmin))
        {
            throw new ForbiddenException("This action requires support agent or manager access.");
        }
    }

    private void RequireManager()
    {
        if (currentUserService.Role != UserRole.ManagerAdmin)
        {
            throw new ForbiddenException("This action requires manager access.");
        }
    }

    private void RequireEmployeeOrHigher()
    {
        if (!currentUserService.Role.HasValue)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }
    }
}
