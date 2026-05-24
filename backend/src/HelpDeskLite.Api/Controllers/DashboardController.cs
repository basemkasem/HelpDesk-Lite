using HelpDeskLite.Api.Authorization;
using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.Employee)]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("employee")]
    public async Task<ActionResult<ApiResponse<object>>> GetEmployeeDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.GetEmployeeDashboardAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(dashboard));
    }

    [HttpGet("support-queue")]
    [Authorize(Policy = PolicyNames.SupportAgent)]
    public async Task<ActionResult<ApiResponse<object>>> GetSupportQueueDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.GetSupportQueueDashboardAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(dashboard));
    }

    [HttpGet("manager")]
    [Authorize(Policy = PolicyNames.Manager)]
    public async Task<ActionResult<ApiResponse<object>>> GetManagerDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.GetManagerDashboardAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(dashboard));
    }
}
