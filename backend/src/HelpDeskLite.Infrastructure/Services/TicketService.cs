using HelpDeskLite.Application.Configuration;
using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Application.Helpers;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;
using HelpDeskLite.Domain.Exceptions;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Infrastructure.Services;

public class TicketService(
    ITicketRepository ticketRepository,
    ICategoryRepository categoryRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    ITicketNumberGenerator ticketNumberGenerator,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    AppDbContext context,
    IOptions<FileStorageSettings> fileStorageOptions) : ITicketService
{
    private readonly FileStorageSettings _fileSettings = fileStorageOptions.Value;

    public async Task<IReadOnlyList<TicketDto>> GetTicketsAsync(CancellationToken cancellationToken = default)
    {
        var (userId, role) = RequireUser();
        var tickets = role == UserRole.Employee
            ? await ticketRepository.GetByUserIdAsync(userId, cancellationToken)
            : await ticketRepository.GetAllAsync(cancellationToken);

        return tickets.Select(MapTicket).ToList();
    }

    public async Task<TicketDetailDto?> GetTicketDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await GetAccessibleTicketAsync(id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        return MapDetail(ticket);
    }

    public async Task<TicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var detail = await GetTicketDetailAsync(id, cancellationToken);
        return detail is null ? null : MapTicketFromDetail(detail);
    }

    public async Task<TicketDto> CreateTicketAsync(
        CreateTicketRequestDto request,
        IReadOnlyList<TicketFileUpload>? files,
        CancellationToken cancellationToken = default)
    {
        var (userId, _) = RequireUser();
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            throw new BadRequestException("Invalid category selected.");
        }

        var fileList = files ?? [];
        if (fileList.Count > _fileSettings.MaxFilesPerTicket)
        {
            throw new BadRequestException($"A maximum of {_fileSettings.MaxFilesPerTicket} attachments is allowed.");
        }

        var ticketId = Guid.NewGuid();
        var ticketNumber = await ticketNumberGenerator.GenerateNextAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var initialStatus = nameof(TicketStatus.New);

        var ticket = new Ticket
        {
            Id = ticketId,
            TicketNumber = ticketNumber,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            Priority = request.Priority,
            CreatedByUserId = userId,
            Status = initialStatus,
            CreatedAt = now
        };

        ticket.StatusHistory.Add(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            OldStatus = initialStatus,
            NewStatus = initialStatus,
            ChangedByUserId = userId,
            ChangedAt = now
        });

        foreach (var file in fileList)
        {
            await using var stream = file.Content;
            var stored = await fileStorageService.SaveAsync(ticketId, file.FileName, file.ContentType, stream, cancellationToken);
            ticket.Attachments.Add(new TicketAttachment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                StoragePath = stored.StoragePath,
                UploadedAt = now
            });
        }

        await ticketRepository.AddAsync(ticket, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(userId, "TicketCreated", nameof(Ticket), ticketId.ToString(),
            $"{{\"ticketNumber\":\"{ticketNumber}\"}}", cancellationToken);

        ticket.Category = category;
        return MapTicket(ticket);
    }

    public async Task<TicketDetailDto> UpdateStatusAsync(
        Guid id,
        UpdateTicketStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        RequireStaff();
        var (userId, role) = RequireUser();
        var ticket = await ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new BadRequestException("Ticket not found.");

        if (!Enum.TryParse<TicketStatus>(ticket.Status, ignoreCase: true, out var currentStatus))
        {
            currentStatus = TicketStatus.New;
        }

        if (!Enum.TryParse<TicketStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            throw new BadRequestException("Invalid status value.");
        }

        var isManager = role == UserRole.ManagerAdmin;
        if (!TicketStatusTransitions.CanTransition(currentStatus, newStatus, isManager))
        {
            throw new BadRequestException($"Cannot transition from {currentStatus} to {newStatus}.");
        }

        if (currentStatus == newStatus)
        {
            throw new BadRequestException("Ticket is already in the requested status.");
        }

        var now = DateTime.UtcNow;
        var oldStatus = ticket.Status;
        ticket.Status = newStatus.ToString();
        ticket.UpdatedAt = now;

        context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            OldStatus = oldStatus,
            NewStatus = ticket.Status,
            ChangedByUserId = userId,
            ChangedAt = now
        });

        if (newStatus == TicketStatus.Assigned && ticket.AssigneeId is null)
        {
            // Encourage assignment when moving to Assigned without assignee
        }

        await context.SaveChangesAsync(cancellationToken);
        return (await GetTicketDetailAsync(id, cancellationToken))!;
    }

    public async Task<TicketDetailDto> AssignTicketAsync(
        Guid id,
        AssignTicketRequestDto request,
        CancellationToken cancellationToken = default)
    {
        RequireStaff();
        var (userId, role) = RequireUser();
        var ticket = await ticketRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new BadRequestException("Ticket not found.");

        if (ticket.AssigneeId == request.AssigneeId)
        {
            return (await GetTicketDetailAsync(id, cancellationToken))!;
        }

        if (request.AssigneeId.HasValue)
        {
            var assignee = await userRepository.GetByIdAsync(request.AssigneeId.Value, cancellationToken);
            if (assignee is null || !assignee.IsActive)
            {
                throw new BadRequestException("Assignee not found.");
            }

            if (assignee.Role == UserRole.Employee)
            {
                throw new BadRequestException("Tickets can only be assigned to support agents or managers.");
            }
        }

        var now = DateTime.UtcNow;
        var previousAssigneeId = ticket.AssigneeId;
        ticket.AssigneeId = request.AssigneeId;
        ticket.UpdatedAt = now;

        if (Enum.TryParse<TicketStatus>(ticket.Status, ignoreCase: true, out var status) &&
            status == TicketStatus.New && request.AssigneeId.HasValue)
        {
            ticket.Status = nameof(TicketStatus.Assigned);
            context.TicketStatusHistories.Add(new TicketStatusHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                OldStatus = nameof(TicketStatus.New),
                NewStatus = ticket.Status,
                ChangedByUserId = userId,
                ChangedAt = now
            });
        }

        context.TicketAssignmentHistories.Add(new TicketAssignmentHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PreviousAssigneeId = previousAssigneeId,
            NewAssigneeId = request.AssigneeId,
            AssignedByUserId = userId,
            AssignedAt = now
        });

        await context.SaveChangesAsync(cancellationToken);
        return (await GetTicketDetailAsync(id, cancellationToken))!;
    }

    public async Task<TicketCommentDto> AddCommentAsync(
        Guid id,
        CreateTicketCommentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var (userId, role) = RequireUser();
        _ = await GetAccessibleTicketAsync(id, cancellationToken)
            ?? throw new BadRequestException("Ticket not found.");

        if (request.IsInternal && role == UserRole.Employee)
        {
            throw new ForbiddenException("Employees cannot add internal comments.");
        }

        var now = DateTime.UtcNow;
        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = id,
            AuthorId = userId,
            Comment = request.Comment.Trim(),
            IsInternal = request.IsInternal,
            CreatedAt = now
        };

        context.TicketComments.Add(comment);

        var ticket = await ticketRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (ticket is not null)
        {
            ticket.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        var author = await userRepository.GetByIdAsync(userId, cancellationToken);
        return new TicketCommentDto
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            AuthorId = comment.AuthorId,
            AuthorName = author?.FullName ?? "Unknown",
            Comment = comment.Comment,
            IsInternal = comment.IsInternal,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IReadOnlyList<TicketActivityDto>> GetTicketHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var (userId, role) = RequireUser();
        var ticket = await ticketRepository.GetByIdWithLifecycleAsync(id, cancellationToken);
        if (ticket is null)
        {
            throw new BadRequestException("Ticket not found.");
        }

        if (role == UserRole.Employee && ticket.CreatedByUserId != userId)
        {
            throw new ForbiddenException();
        }

        var includeInternal = role is UserRole.SupportAgent or UserRole.ManagerAdmin;
        var activities = new List<TicketActivityDto>();

        activities.Add(new TicketActivityDto
        {
            Id = ticket.Id,
            ActivityType = "Created",
            Summary = "Ticket created",
            Detail = ticket.Description,
            ActorId = ticket.CreatedByUserId,
            ActorName = ticket.CreatedByUser.FullName,
            OccurredAt = ticket.CreatedAt
        });

        foreach (var history in ticket.StatusHistory)
        {
            activities.Add(new TicketActivityDto
            {
                Id = history.Id,
                ActivityType = "StatusChange",
                Summary = $"Status changed from {history.OldStatus} to {history.NewStatus}",
                ActorId = history.ChangedByUserId,
                ActorName = history.ChangedByUser.FullName,
                OccurredAt = history.ChangedAt
            });
        }

        foreach (var assignment in ticket.AssignmentHistory)
        {
            var prev = assignment.PreviousAssignee?.FullName ?? "Unassigned";
            var next = assignment.NewAssignee?.FullName ?? "Unassigned";
            activities.Add(new TicketActivityDto
            {
                Id = assignment.Id,
                ActivityType = "Assignment",
                Summary = $"Assignment changed from {prev} to {next}",
                ActorId = assignment.AssignedByUserId,
                ActorName = assignment.AssignedByUser.FullName,
                OccurredAt = assignment.AssignedAt
            });
        }

        foreach (var comment in ticket.Comments)
        {
            if (!includeInternal && comment.IsInternal)
            {
                continue;
            }

            activities.Add(new TicketActivityDto
            {
                Id = comment.Id,
                ActivityType = comment.IsInternal ? "InternalComment" : "Comment",
                Summary = comment.IsInternal ? "Internal note added" : "Comment added",
                Detail = comment.Comment,
                ActorId = comment.AuthorId,
                ActorName = comment.Author.FullName,
                OccurredAt = comment.CreatedAt,
                IsInternal = comment.IsInternal
            });
        }

        foreach (var attachment in ticket.Attachments)
        {
            activities.Add(new TicketActivityDto
            {
                Id = attachment.Id,
                ActivityType = "Attachment",
                Summary = $"Attachment added: {attachment.FileName}",
                OccurredAt = attachment.UploadedAt
            });
        }

        return activities.OrderBy(a => a.OccurredAt).ToList();
    }

    private async Task<Ticket?> GetAccessibleTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var (userId, role) = RequireUser();
        var ticket = await ticketRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        if (role == UserRole.Employee && ticket.CreatedByUserId != userId)
        {
            throw new ForbiddenException();
        }

        return ticket;
    }

    private (Guid UserId, UserRole Role) RequireUser()
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");
        var role = currentUserService.Role
            ?? throw new UnauthorizedException("User role is missing.");
        return (userId, role);
    }

    private void RequireStaff()
    {
        var role = currentUserService.Role;
        if (role is not (UserRole.SupportAgent or UserRole.ManagerAdmin))
        {
            throw new ForbiddenException("Only support staff can perform this action.");
        }
    }

    private TicketDetailDto MapDetail(Ticket ticket)
    {
        var (userId, role) = RequireUser();
        Enum.TryParse<TicketStatus>(ticket.Status, ignoreCase: true, out var currentStatus);
        var isManager = role == UserRole.ManagerAdmin;
        var isStaff = role is UserRole.SupportAgent or UserRole.ManagerAdmin;

        return new TicketDetailDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? string.Empty,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByName = ticket.CreatedByUser?.FullName ?? string.Empty,
            AssigneeId = ticket.AssigneeId,
            AssigneeName = ticket.Assignee?.FullName,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Attachments = ticket.Attachments.Select(a => new TicketAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes
            }).ToList(),
            AllowedNextStatuses = isStaff
                ? TicketStatusTransitions.GetAllowedNextStatuses(currentStatus, isManager).Select(s => s.ToString()).ToList()
                : []
        };
    }

    private static TicketDto MapTicket(Ticket ticket) => new()
    {
        Id = ticket.Id,
        TicketNumber = ticket.TicketNumber,
        Title = ticket.Title,
        Description = ticket.Description,
        CategoryId = ticket.CategoryId,
        CategoryName = ticket.Category?.Name ?? string.Empty,
        Priority = ticket.Priority,
        CreatedByUserId = ticket.CreatedByUserId,
        Status = ticket.Status,
        AssigneeId = ticket.AssigneeId,
        AssigneeName = ticket.Assignee?.FullName,
        CreatedAt = ticket.CreatedAt,
        Attachments = ticket.Attachments.Select(a => new TicketAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSizeBytes = a.FileSizeBytes
        }).ToList()
    };

    private static TicketDto MapTicketFromDetail(TicketDetailDto d) => new()
    {
        Id = d.Id,
        TicketNumber = d.TicketNumber,
        Title = d.Title,
        Description = d.Description,
        CategoryId = d.CategoryId,
        CategoryName = d.CategoryName,
        Priority = d.Priority,
        CreatedByUserId = d.CreatedByUserId,
        Status = d.Status,
        AssigneeId = d.AssigneeId,
        AssigneeName = d.AssigneeName,
        CreatedAt = d.CreatedAt,
        Attachments = d.Attachments
    };
}
