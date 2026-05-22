using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}
