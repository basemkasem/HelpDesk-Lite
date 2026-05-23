namespace HelpDeskLite.Domain.Entities;

public class TicketAssignmentHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid? PreviousAssigneeId { get; set; }
    public Guid? NewAssigneeId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User? PreviousAssignee { get; set; }
    public User? NewAssignee { get; set; }
    public User AssignedByUser { get; set; } = null!;
}
