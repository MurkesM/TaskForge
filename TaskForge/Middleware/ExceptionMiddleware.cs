using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        try
        {
            await _next(context);
        }
        catch (DomainException dex)
        {
            _logger.LogWarning(dex, "Domain validation error");

            var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var problem = factory.CreateProblemDetails(
                context,
                statusCode: dex.StatusCode,
                title: "Business rule violation",
                detail: dex.Message
            );

            context.Response.StatusCode = dex.StatusCode;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
            return;
        }

    }
}