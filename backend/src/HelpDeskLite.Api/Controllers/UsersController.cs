using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.DTOs.Users;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet("agents")]
    public async Task<ActionResult<ApiResponse<object>>> GetAgents(CancellationToken cancellationToken)
    {
        var agents = await userRepository.GetByRolesAsync(
            [UserRole.SupportAgent, UserRole.ManagerAdmin],
            cancellationToken);

        var result = agents.Select(u => new AgentUserDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName
        }).ToList();

        return Ok(ApiResponse<object>.Ok(result));
    }
}
