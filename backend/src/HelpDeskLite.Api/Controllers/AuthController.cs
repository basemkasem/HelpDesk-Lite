using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.DTOs.Auth;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ICurrentUserService currentUserService, IUserRepository userRepository) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Unauthorized."));
        }

        await authService.LogoutAsync(currentUserService.UserId.Value, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { message = "Logged out successfully." }));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public ActionResult<ApiResponse<object>> Refresh()
    {
        // Refresh token rotation will be implemented in a future epic.
        return StatusCode(StatusCodes.Status501NotImplemented, ApiResponse<object>.Fail("Refresh token endpoint is not yet implemented."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Me(CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("Unauthorized."));
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("User not found."));
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Role
        }));
    }
}
