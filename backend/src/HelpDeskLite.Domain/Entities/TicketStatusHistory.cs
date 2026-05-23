namespace HelpDeskLite.Domain.Entities;

public class TicketStatusHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
