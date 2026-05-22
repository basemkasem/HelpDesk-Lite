namespace HelpDeskLite.Application.DTOs.Tickets;

public class TicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string Status { get; set; } = string.Empty;
}
