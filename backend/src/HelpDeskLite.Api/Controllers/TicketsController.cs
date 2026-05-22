using HelpDeskLite.Api.Authorization;
using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.Employee)]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetTickets(CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetTicketsAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(tickets));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetTicket(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound(ApiResponse<object>.Fail("Ticket not found."));
        }

        return Ok(ApiResponse<object>.Ok(ticket));
    }

    [HttpGet("agent-queue")]
    [Authorize(Policy = PolicyNames.SupportAgent)]
    public ActionResult<ApiResponse<object>> GetAgentQueue()
    {
        return Ok(ApiResponse<object>.Ok(new { message = "Support agent queue endpoint (stub)." }));
    }
}
