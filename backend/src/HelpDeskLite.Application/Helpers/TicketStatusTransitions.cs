using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.Helpers;

/// <summary>
/// Defines valid ticket status workflow transitions. Managers may override any transition.
/// </summary>
public static class TicketStatusTransitions
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.New] = [TicketStatus.Assigned, TicketStatus.Closed],
        [TicketStatus.Assigned] =
        [
            TicketStatus.InProgress,
            TicketStatus.WaitingForUser,
            TicketStatus.Resolved,
            TicketStatus.Closed
        ],
        [TicketStatus.InProgress] =
        [
            TicketStatus.WaitingForUser,
            TicketStatus.Resolved,
            TicketStatus.Assigned
        ],
        [TicketStatus.WaitingForUser] = [TicketStatus.InProgress, TicketStatus.Resolved],
        [TicketStatus.Resolved] = [TicketStatus.Closed, TicketStatus.InProgress],
        [TicketStatus.Closed] = [TicketStatus.InProgress]
    };

    public static bool CanTransition(TicketStatus from, TicketStatus to, bool isManagerOverride) =>
        isManagerOverride || (AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to));

    public static IReadOnlyList<TicketStatus> GetAllowedNextStatuses(TicketStatus current, bool isManagerOverride) =>
        isManagerOverride
            ? Enum.GetValues<TicketStatus>().Where(s => s != current).ToList()
            : AllowedTransitions.TryGetValue(current, out var allowed)
                ? allowed.ToList()
                : [];
}
