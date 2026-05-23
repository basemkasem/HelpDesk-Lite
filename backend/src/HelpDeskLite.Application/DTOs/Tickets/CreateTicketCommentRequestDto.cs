namespace HelpDeskLite.Application.DTOs.Tickets;

public class CreateTicketCommentRequestDto
{
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
