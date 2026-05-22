using HelpDeskLite.Application.Configuration;
using HelpDeskLite.Application.DTOs.Auth;
using HelpDeskLite.Application.DTOs.Users;
using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Exceptions;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDeskLite.Infrastructure.Services;

public class AuthService(
    AppDbContext context,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService,
    IAuditService auditService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive ||
            !passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException();
        }

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        });

        user.LastLoginAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(user.Id, "UserLoggedIn", nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = MapUser(user)
        };
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync(userId, "UserLoggedOut", nameof(User), userId.ToString(), cancellationToken: cancellationToken);
    }

    public Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Refresh token rotation will be implemented in a future epic.
        return Task.FromResult<LoginResponseDto?>(null);
    }

    private static UserDto MapUser(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role
    };
}
