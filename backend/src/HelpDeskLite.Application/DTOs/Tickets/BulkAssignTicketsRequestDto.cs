namespace HelpDeskLite.Application.DTOs.Tickets;

public class BulkAssignTicketsRequestDto
{
    public IReadOnlyList<Guid> TicketIds { get; set; } = [];
    public Guid? AssigneeId { get; set; }
}
