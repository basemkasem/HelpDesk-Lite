using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDeskLite.Infrastructure.Authentication;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) =>
        _hasher.HashPassword(user, password);

    public bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword) =>
        _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
}
