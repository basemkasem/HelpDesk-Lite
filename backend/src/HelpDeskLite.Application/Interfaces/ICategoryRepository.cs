using HelpDeskLite.Domain.Entities;

namespace HelpDeskLite.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
