using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;
using HelpDeskLite.Domain.Exceptions;

namespace HelpDeskLite.Infrastructure.Services;

public class TicketService(
    ITicketRepository ticketRepository,
    ICurrentUserService currentUserService) : ITicketService
{
    public async Task<IReadOnlyList<TicketDto>> GetTicketsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var role = currentUserService.Role
            ?? throw new UnauthorizedException("User role is missing.");

        var tickets = role == UserRole.Employee
            ? await ticketRepository.GetByUserIdAsync(userId, cancellationToken)
            : await ticketRepository.GetAllAsync(cancellationToken);

        return tickets.Select(MapTicket).ToList();
    }

    public async Task<TicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");
        var role = currentUserService.Role
            ?? throw new UnauthorizedException("User role is missing.");

        if (role == UserRole.Employee && ticket.CreatedByUserId != userId)
        {
            throw new ForbiddenException();
        }

        return MapTicket(ticket);
    }

    private static TicketDto MapTicket(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        CreatedByUserId = ticket.CreatedByUserId,
        Status = ticket.Status
    };
}
