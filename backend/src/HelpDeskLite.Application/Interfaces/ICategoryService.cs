using HelpDeskLite.Application.DTOs.Tickets;

namespace HelpDeskLite.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
