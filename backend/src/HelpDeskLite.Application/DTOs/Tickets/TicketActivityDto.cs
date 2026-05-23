namespace HelpDeskLite.Application.DTOs.Tickets;

public class TicketActivityDto
{
    public Guid Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsInternal { get; set; }
}
