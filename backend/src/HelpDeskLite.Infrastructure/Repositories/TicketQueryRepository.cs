using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Repositories;

public class TicketQueryRepository(AppDbContext context) : ITicketQueryRepository
{
    private static readonly string[] OpenStatuses =
    [
        nameof(TicketStatus.New),
        nameof(TicketStatus.Assigned),
        nameof(TicketStatus.InProgress),
        nameof(TicketStatus.WaitingForUser)
    ];

    public async Task<TicketQueryResult> QueryAsync(TicketQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(context.Tickets.AsNoTracking(), filter.Scope, filter.CurrentUserId);
        query = ApplyFilters(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var tickets = await query
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedByUser)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new TicketQueryResult { Tickets = tickets, TotalCount = totalCount };
    }

    public async Task<IReadOnlyList<StatusCountProjection>> GetStatusCountsAsync(
        TicketQueryScope scope,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyScope(context.Tickets.AsNoTracking(), scope, userId);

        return await query
            .GroupBy(t => t.Status)
            .Select(g => new StatusCountProjection { Status = g.Key, Count = g.Count() })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResolutionTrendProjection>> GetResolutionTrendAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var resolvedStatuses = new[] { nameof(TicketStatus.Resolved), nameof(TicketStatus.Closed) };

        var raw = await context.TicketStatusHistories.AsNoTracking()
            .Where(h => resolvedStatuses.Contains(h.NewStatus) && h.ChangedAt >= from)
            .GroupBy(h => DateOnly.FromDateTime(h.ChangedAt.Date))
            .Select(g => new ResolutionTrendProjection { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new List<ResolutionTrendProjection>();
        for (var i = 0; i < days; i++)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-(days - 1 - i)));
            var match = raw.FirstOrDefault(r => r.Date == date);
            result.Add(new ResolutionTrendProjection { Date = date, Count = match?.Count ?? 0 });
        }

        return result;
    }

    public async Task<IReadOnlyList<DashboardActivityProjection>> GetRecentActivityForUserAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var ticketIds = await context.Tickets.AsNoTracking()
            .Where(t => t.CreatedByUserId == userId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        if (ticketIds.Count == 0)
        {
            return [];
        }

        var statusEvents =
            from h in context.TicketStatusHistories.AsNoTracking()
            join t in context.Tickets.AsNoTracking() on h.TicketId equals t.Id
            join u in context.Users.AsNoTracking() on h.ChangedByUserId equals u.Id
            where ticketIds.Contains(h.TicketId)
            select new DashboardActivityProjection
            {
                Id = h.Id,
                TicketId = h.TicketId,
                TicketNumber = t.TicketNumber,
                TicketTitle = t.Title,
                ActivityType = "StatusChange",
                Summary = "Status changed to " + h.NewStatus,
                ActorName = u.FullName,
                OccurredAt = h.ChangedAt
            };

        var publicComments =
            from c in context.TicketComments.AsNoTracking()
            join t in context.Tickets.AsNoTracking() on c.TicketId equals t.Id
            join u in context.Users.AsNoTracking() on c.AuthorId equals u.Id
            where ticketIds.Contains(c.TicketId) && !c.IsInternal
            select new DashboardActivityProjection
            {
                Id = c.Id,
                TicketId = c.TicketId,
                TicketNumber = t.TicketNumber,
                TicketTitle = t.Title,
                ActivityType = "Comment",
                Summary = "New comment on your ticket",
                ActorName = u.FullName,
                OccurredAt = c.CreatedAt
            };

        return await statusEvents
            .Concat(publicComments)
            .OrderByDescending(a => a.OccurredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Ticket> ApplyScope(IQueryable<Ticket> query, TicketQueryScope scope, Guid userId) =>
        scope switch
        {
            TicketQueryScope.OwnTickets => query.Where(t => t.CreatedByUserId == userId),
            TicketQueryScope.AssignedToCurrentUser => query.Where(t => t.AssigneeId == userId),
            _ => query
        };

    private static IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query, TicketQueryFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                t.TicketNumber.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == filter.AssigneeId.Value);
        }

        if (filter.AssignedToMe == true)
        {
            query = query.Where(t => t.AssigneeId == filter.CurrentUserId);
        }

        if (filter.UnassignedOnly == true)
        {
            query = query.Where(t => t.AssigneeId == null);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value.ToUniversalTime());
        }

        if (filter.CreatedTo.HasValue)
        {
            var end = filter.CreatedTo.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(t => t.CreatedAt < end);
        }

        if (filter.OpenOnly == true)
        {
            query = query.Where(t => OpenStatuses.Contains(t.Status));
        }

        return query;
    }

    private static IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, string sortBy, string sortDirection)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => desc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "assignee" => desc
                ? query.OrderByDescending(t => t.Assignee != null ? t.Assignee.FullName : "")
                : query.OrderBy(t => t.Assignee != null ? t.Assignee.FullName : ""),
            _ => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };
    }
}
