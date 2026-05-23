using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Interfaces;

public interface ITicketService
{
    Task<IReadOnlyList<TicketDto>> GetTicketsAsync(CancellationToken cancellationToken = default);
    Task<TicketDetailDto?> GetTicketDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TicketDto> CreateTicketAsync(CreateTicketRequestDto request, IReadOnlyList<TicketFileUpload>? files, CancellationToken cancellationToken = default);
    Task<TicketDetailDto> UpdateStatusAsync(Guid id, UpdateTicketStatusRequestDto request, CancellationToken cancellationToken = default);
    Task<TicketDetailDto> AssignTicketAsync(Guid id, AssignTicketRequestDto request, CancellationToken cancellationToken = default);
    Task<TicketCommentDto> AddCommentAsync(Guid id, CreateTicketCommentRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketActivityDto>> GetTicketHistoryAsync(Guid id, CancellationToken cancellationToken = default);
}

public record TicketFileUpload(string FileName, string ContentType, Stream Content, long Length);

public class TicketCommentDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
