using HelpDeskLite.Domain.Entities;

namespace HelpDeskLite.Application.Interfaces;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
