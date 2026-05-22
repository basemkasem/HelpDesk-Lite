namespace HelpDeskLite.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string Status { get; set; } = "Open";

    public User CreatedByUser { get; set; } = null!;
}
