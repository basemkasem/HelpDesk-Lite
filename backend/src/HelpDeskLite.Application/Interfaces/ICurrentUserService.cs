using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(UserRole role);
    bool IsInAnyRole(params UserRole[] roles);
}
