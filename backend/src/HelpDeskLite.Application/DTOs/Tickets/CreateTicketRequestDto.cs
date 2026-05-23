using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.DTOs.Tickets;

public class CreateTicketRequestDto
{
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
}
