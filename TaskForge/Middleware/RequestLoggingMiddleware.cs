using System.Diagnostics;

namespace TaskForge.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        _logger.LogInformation("Incoming request {method} {path} (traceId: {traceId})",
            context.Request.Method,
            context.Request.Path,
            traceId);

        await _next(context);

        sw.Stop();

        _logger.LogInformation("Completed request {method} {path} with {statusCode} in {elapsed}ms (traceId: {traceId})",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            traceId);
    }
}