using HelpDeskLite.Application.DTOs.Tickets;
using HelpDeskLite.Application.Interfaces;

namespace HelpDeskLite.Infrastructure.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetActiveAsync(cancellationToken);
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();
    }
}
