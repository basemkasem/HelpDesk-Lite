using FluentValidation;
using HelpDeskLite.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskLite.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }
}
