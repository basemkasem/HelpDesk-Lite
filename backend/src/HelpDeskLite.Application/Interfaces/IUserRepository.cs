using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;

namespace HelpDeskLite.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByRolesAsync(IEnumerable<UserRole> roles, CancellationToken cancellationToken = default);
}
