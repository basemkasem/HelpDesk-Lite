namespace HelpDeskLite.Application.Interfaces;

public interface ITicketNumberGenerator
{
    Task<string> GenerateNextAsync(CancellationToken cancellationToken = default);
}
