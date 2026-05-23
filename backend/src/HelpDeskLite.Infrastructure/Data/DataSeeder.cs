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
        await SeedCategoriesAsync(cancellationToken);
        await SeedKnowledgeBaseAsync(cancellationToken);

        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Users already seeded.");
            return;
        }

        var employee = CreateUser("employee@helpdesk.local", "Demo Employee", UserRole.Employee, "Employee123!");
        var agent = CreateUser("agent@helpdesk.local", "Demo Support Agent", UserRole.SupportAgent, "Agent123!");
        var admin = CreateUser("admin@helpdesk.local", "Demo Manager Admin", UserRole.ManagerAdmin, "Admin123!");

        context.Users.AddRange(employee, agent, admin);

        var hardware = await context.Categories.FirstAsync(c => c.Name == "Hardware", cancellationToken);
        var software = await context.Categories.FirstAsync(c => c.Name == "Software", cancellationToken);
        var now = DateTime.UtcNow;

        var ticket1Id = Guid.NewGuid();
        var ticket2Id = Guid.NewGuid();
        var ticket3Id = Guid.NewGuid();

        context.Tickets.AddRange(
            new Ticket
            {
                Id = ticket1Id,
                TicketNumber = "HD-SEED-00001",
                Title = "Laptop not connecting to VPN",
                Description = "My laptop cannot connect to the corporate VPN since this morning.",
                CategoryId = hardware.Id,
                Priority = TicketPriority.High,
                CreatedByUserId = employee.Id,
                AssigneeId = agent.Id,
                Status = nameof(TicketStatus.InProgress),
                CreatedAt = now
            },
            new Ticket
            {
                Id = ticket2Id,
                TicketNumber = "HD-SEED-00002",
                Title = "Password reset request",
                Description = "I need my Active Directory password reset for my employee account.",
                CategoryId = software.Id,
                Priority = TicketPriority.Medium,
                CreatedByUserId = employee.Id,
                AssigneeId = agent.Id,
                Status = nameof(TicketStatus.WaitingForUser),
                CreatedAt = now.AddHours(-2)
            },
            new Ticket
            {
                Id = ticket3Id,
                TicketNumber = "HD-SEED-00003",
                Title = "Printer offline in office 3",
                Description = "The shared printer on floor 3 shows offline for all users.",
                CategoryId = hardware.Id,
                Priority = TicketPriority.Low,
                CreatedByUserId = agent.Id,
                Status = nameof(TicketStatus.New),
                CreatedAt = now.AddHours(-5)
            });

        context.TicketStatusHistories.AddRange(
            new TicketStatusHistory
            {
                Id = Guid.NewGuid(), TicketId = ticket1Id, OldStatus = nameof(TicketStatus.New),
                NewStatus = nameof(TicketStatus.Assigned), ChangedByUserId = admin.Id, ChangedAt = now.AddMinutes(-30)
            },
            new TicketStatusHistory
            {
                Id = Guid.NewGuid(), TicketId = ticket1Id, OldStatus = nameof(TicketStatus.Assigned),
                NewStatus = nameof(TicketStatus.InProgress), ChangedByUserId = agent.Id, ChangedAt = now.AddMinutes(-10)
            });

        context.TicketComments.Add(
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1Id,
                AuthorId = agent.Id,
                Comment = "Checking VPN gateway logs.",
                IsInternal = true,
                CreatedAt = now.AddMinutes(-5)
            });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed data created for users and tickets.");
    }

    private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Hardware", Description = "Laptops, printers, peripherals", SortOrder = 1 },
            new Category { Id = Guid.NewGuid(), Name = "Software", Description = "Applications and licenses", SortOrder = 2 },
            new Category { Id = Guid.NewGuid(), Name = "Network", Description = "VPN, Wi-Fi, connectivity", SortOrder = 3 },
            new Category { Id = Guid.NewGuid(), Name = "Account Access", Description = "Passwords, permissions, SSO", SortOrder = 4 },
            new Category { Id = Guid.NewGuid(), Name = "Other", Description = "General requests", SortOrder = 5 });

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedKnowledgeBaseAsync(CancellationToken cancellationToken)
    {
        if (await context.KnowledgeBaseArticles.AnyAsync(cancellationToken))
        {
            return;
        }

        context.KnowledgeBaseArticles.AddRange(
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                Title = "VPN troubleshooting guide",
                Summary = "Steps to fix common VPN connection issues on Windows and Mac.",
                Keywords = "vpn,connect,network,remote,ssl",
                Url = "/kb/vpn-troubleshooting"
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                Title = "Reset your password",
                Summary = "How to reset your Active Directory password using the self-service portal.",
                Keywords = "password,reset,login,account,access",
                Url = "/kb/password-reset"
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                Title = "Printer offline — quick fixes",
                Summary = "Check power, queue, and driver when a shared printer shows offline.",
                Keywords = "printer,offline,print,hardware",
                Url = "/kb/printer-offline"
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                Title = "Install approved software",
                Summary = "Request software installs from the internal software catalog.",
                Keywords = "software,install,application,license",
                Url = "/kb/software-install"
            });

        await context.SaveChangesAsync(cancellationToken);
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
