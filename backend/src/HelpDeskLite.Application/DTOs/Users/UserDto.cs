using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
