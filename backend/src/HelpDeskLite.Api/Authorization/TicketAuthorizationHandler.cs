using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HelpDeskLite.Api.Authorization;

public class TicketAuthorizationHandler(
    ICurrentUserService currentUserService,
    ITicketRepository ticketRepository) : AuthorizationHandler<TicketOperationRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TicketOperationRequirement requirement,
        Guid ticketId)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null || currentUserService.Role is null)
        {
            return;
        }

        var ticket = await ticketRepository.GetByIdAsync(ticketId);
        if (ticket is null)
        {
            return;
        }

        if (TicketAccessHelper.CanAccessTicket(currentUserService.Role.Value, currentUserService.UserId.Value, ticket.CreatedByUserId))
        {
            context.Succeed(requirement);
        }
    }
}
