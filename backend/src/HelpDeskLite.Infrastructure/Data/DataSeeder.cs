using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HelpDeskLite.Infrastructure.Data;

public class DataSeeder(
    AppDbContext context,
    IPasswordHasherService passwordHasher,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        var employee = CreateUser("employee@helpdesk.local", "Demo Employee", UserRole.Employee, "Employee123!");
        var agent = CreateUser("agent@helpdesk.local", "Demo Support Agent", UserRole.SupportAgent, "Agent123!");
        var admin = CreateUser("admin@helpdesk.local", "Demo Manager Admin", UserRole.ManagerAdmin, "Admin123!");

        context.Users.AddRange(employee, agent, admin);

        context.Tickets.AddRange(
            new Ticket { Id = Guid.NewGuid(), Title = "Laptop not connecting to VPN", CreatedByUserId = employee.Id, Status = "Open" },
            new Ticket { Id = Guid.NewGuid(), Title = "Password reset request", CreatedByUserId = employee.Id, Status = "InProgress" },
            new Ticket { Id = Guid.NewGuid(), Title = "Printer offline in office 3", CreatedByUserId = agent.Id, Status = "Open" });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed data created for users and tickets.");
    }

    private User CreateUser(string email, string fullName, UserRole role, string password)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        return user;
    }
}
