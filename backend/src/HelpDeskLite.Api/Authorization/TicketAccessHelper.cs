using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Api.Authorization;

public static class TicketAccessHelper
{
    public static bool CanAccessTicket(UserRole role, Guid userId, Guid ticketOwnerId) =>
        role is UserRole.SupportAgent or UserRole.ManagerAdmin || ticketOwnerId == userId;
}
