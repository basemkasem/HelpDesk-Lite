using System.Security.Claims;
using HelpDeskLite.Application.Authentication;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace HelpDeskLite.Infrastructure.Authentication;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var role = User?.FindFirstValue(CustomClaimTypes.Role);
            return Enum.TryParse<UserRole>(role, out var parsed) ? parsed : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(UserRole role) => Role == role;

    public bool IsInAnyRole(params UserRole[] roles) => Role.HasValue && roles.Contains(Role.Value);
}
