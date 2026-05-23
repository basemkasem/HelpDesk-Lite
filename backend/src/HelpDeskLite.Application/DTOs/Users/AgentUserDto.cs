namespace HelpDeskLite.Application.DTOs.Users;

public class AgentUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
