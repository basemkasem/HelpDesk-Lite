using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Repositories;

public class TicketRepository(AppDbContext context) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.Attachments)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await context.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.Attachments)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Ticket?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Ticket?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Ticket?> GetByIdWithLifecycleAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Attachments)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Include(t => t.StatusHistory).ThenInclude(h => h.ChangedByUser)
            .Include(t => t.AssignmentHistory).ThenInclude(a => a.NewAssignee)
            .Include(t => t.AssignmentHistory).ThenInclude(a => a.PreviousAssignee)
            .Include(t => t.AssignmentHistory).ThenInclude(a => a.AssignedByUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default) =>
        await context.Tickets.AddAsync(ticket, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        context.Tickets.CountAsync(cancellationToken);
}
