using System.Net;
using System.Text.Json;
using FluentValidation;
using HelpDeskLite.Application.Common;
using HelpDeskLite.Domain.Exceptions;

namespace HelpDeskLite.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;
        var userId = context.User?.FindFirst("sub")?.Value;

        logger.LogError(exception, "Unhandled exception. CorrelationId={CorrelationId} UserId={UserId}", correlationId, userId);

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("Validation failed.", validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail(unauthorizedEx.Message)),
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(forbiddenEx.Message)),
            BadRequestException badRequestEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(badRequestEx.Message)),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("Unauthorized.")),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
