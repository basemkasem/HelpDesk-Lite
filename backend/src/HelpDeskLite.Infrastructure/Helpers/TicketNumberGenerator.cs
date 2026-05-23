using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Helpers;

public class TicketNumberGenerator(AppDbContext context) : ITicketNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var count = await context.Tickets.CountAsync(cancellationToken);
        var next = count + 1;
        return $"HD-{DateTime.UtcNow:yyyyMMdd}-{next:D5}";
    }
}
