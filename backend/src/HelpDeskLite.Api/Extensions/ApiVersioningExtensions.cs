namespace HelpDeskLite.Api.Extensions;

/// <summary>
/// API versioning preparation for a future epic.
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddHelpDeskApiVersioning(this IServiceCollection services)
    {
        // services.AddApiVersioning(options =>
        // {
        //     options.DefaultApiVersion = new ApiVersion(1, 0);
        //     options.AssumeDefaultVersionWhenUnspecified = true;
        //     options.ReportApiVersions = true;
        // });
        return services;
    }
}
