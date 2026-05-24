using HelpDeskLite.Api.Authorization;
using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.SupportAgent)]
public class ReportsController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("workload")]
    public async Task<ActionResult<ApiResponse<object>>> GetWorkload(CancellationToken cancellationToken)
    {
        var report = await dashboardService.GetWorkloadReportAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(report));
    }

    [HttpGet("ticket-aging")]
    public async Task<ActionResult<ApiResponse<object>>> GetTicketAging(CancellationToken cancellationToken)
    {
        var report = await dashboardService.GetTicketAgingReportAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(report));
    }
}
