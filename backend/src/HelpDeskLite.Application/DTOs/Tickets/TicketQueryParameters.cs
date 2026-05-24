using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.DTOs.Tickets;

public class TicketQueryParameters
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AssigneeId { get; set; }
    public bool? AssignedToMe { get; set; }
    public bool? UnassignedOnly { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
