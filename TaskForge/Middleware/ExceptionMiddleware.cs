using FluentValidation;
using System.Net;
using System.Text.Json;
using TaskForge.Exceptions;

namespace TaskForge.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var status = ex switch
        {
            DomainException d => d.StatusCode,
            ValidationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = status;

        object errorBody = ex switch
        {
            ValidationException v => new
            {
                error = "ValidationError",
                message = "One or more validation errors occurred.",
                details = v.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage }),
                correlationId
            },

            DomainException d => new
            {
                error = "DomainError",
                message = d.Message,
                correlationId
            },

            UnauthorizedAccessException => new
            {
                error = "Unauthorized",
                message = "You are not authorized to perform this action.",
                correlationId
            },

            KeyNotFoundException => new
            {
                error = "NotFound",
                message = ex.Message,
                correlationId
            },

            _ => new
            {
                error = "ServerError",
                message = "An unexpected error occurred.",
                correlationId
            }
        };

        var json = JsonSerializer.Serialize(errorBody);
        await context.Response.WriteAsync(json);
    }
}
