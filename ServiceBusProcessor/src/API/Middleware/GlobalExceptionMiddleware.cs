using FluentValidation;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                ve.Errors.Select(e => e.ErrorMessage).ToArray()),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                new[] { exception.Message }),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                new[] { "You are not authorized to perform this action." }),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                new[] { "Please try again later." })
        };

        var correlationId = context.TraceIdentifier;

        _logger.LogError(exception,
            "Unhandled exception [{StatusCode}] {Title} | CorrelationId: {CorrelationId}",
            (int)statusCode, title, correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            correlationId,
            errors,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
