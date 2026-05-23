using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public TicketPriority Priority { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Status { get; set; } = nameof(TicketStatus.New);
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category Category { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? Assignee { get; set; }
    public ICollection<TicketAttachment> Attachments { get; set; } = [];
    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<TicketAssignmentHistory> AssignmentHistory { get; set; } = [];
}
