using FluentValidation;
using HelpDeskLite.Api.Authorization;
using HelpDeskLite.Application.Common;
using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PolicyNames.Employee)]
public class TicketsController(
    ITicketService ticketService,
    IValidator<CreateTicketRequestDto> createTicketValidator,
    IValidator<UpdateTicketStatusRequestDto> updateStatusValidator,
    IValidator<CreateTicketCommentRequestDto> createCommentValidator) : ControllerBase
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
        var ticket = await ticketService.GetTicketDetailAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound(ApiResponse<object>.Fail("Ticket not found."));
        }

        return Ok(ApiResponse<object>.Ok(ticket));
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<ApiResponse<object>>> GetTicketHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await ticketService.GetTicketHistoryAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(history));
    }

    [HttpPost]
    [RequestSizeLimit(52_428_800)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<object>>> CreateTicket(
        [FromForm] CreateTicketForm form,
        CancellationToken cancellationToken)
    {
        var request = new CreateTicketRequestDto
        {
            Title = form.Title,
            CategoryId = form.CategoryId,
            Description = form.Description,
            Priority = form.Priority
        };

        var validation = await createTicketValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var files = new List<TicketFileUpload>();
        if (form.Attachments is not null)
        {
            foreach (var file in form.Attachments)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var memory = new MemoryStream();
                await file.CopyToAsync(memory, cancellationToken);
                memory.Position = 0;
                files.Add(new TicketFileUpload(file.FileName, file.ContentType, memory, memory.Length));
            }
        }

        var ticket = await ticketService.CreateTicketAsync(request, files, cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ApiResponse<object>.Ok(ticket));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PolicyNames.SupportAgent)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var validation = await updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var ticket = await ticketService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ticket));
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = PolicyNames.SupportAgent)]
    public async Task<ActionResult<ApiResponse<object>>> AssignTicket(
        Guid id,
        [FromBody] AssignTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketService.AssignTicketAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ticket));
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<ApiResponse<object>>> AddComment(
        Guid id,
        [FromBody] CreateTicketCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var validation = await createCommentValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var comment = await ticketService.AddCommentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(comment));
    }

    [HttpGet("agent-queue")]
    [Authorize(Policy = PolicyNames.SupportAgent)]
    public async Task<ActionResult<ApiResponse<object>>> GetAgentQueue(CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetTicketsAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(tickets));
    }
}

public class CreateTicketForm
{
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}
