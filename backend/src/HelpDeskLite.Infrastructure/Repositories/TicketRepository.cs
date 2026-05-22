using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Repositories;

public class TicketRepository(AppDbContext context) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Tickets.AsNoTracking().OrderBy(t => t.Title).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await context.Tickets.AsNoTracking()
            .Where(t => t.CreatedByUserId == userId)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
