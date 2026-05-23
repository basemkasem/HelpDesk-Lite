using HelpDeskLite.Api.Authorization;
using HelpDeskLite.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HelpDeskLite.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddHelpDeskAuthorization(this IServiceCollection services)
    {
        // Scoped: handler uses ICurrentUserService and ITicketRepository (request-scoped).
        services.AddScoped<IAuthorizationHandler, TicketAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.Employee, policy =>
                policy.RequireRole(
                    nameof(UserRole.Employee),
                    nameof(UserRole.SupportAgent),
                    nameof(UserRole.ManagerAdmin)));

            options.AddPolicy(PolicyNames.SupportAgent, policy =>
                policy.RequireRole(
                    nameof(UserRole.SupportAgent),
                    nameof(UserRole.ManagerAdmin)));

            options.AddPolicy(PolicyNames.Manager, policy =>
                policy.RequireRole(nameof(UserRole.ManagerAdmin)));
        });

        return services;
    }
}
