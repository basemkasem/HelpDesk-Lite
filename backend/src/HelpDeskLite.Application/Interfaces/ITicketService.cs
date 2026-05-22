using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Interfaces;

public interface ITicketService
{
    Task<IReadOnlyList<TicketDto>> GetTicketsAsync(CancellationToken cancellationToken = default);
    Task<TicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
