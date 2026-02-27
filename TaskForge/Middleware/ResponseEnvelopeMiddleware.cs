using System.Text;
using System.Text.Json;

namespace TaskForge.Middleware;

public class ResponseEnvelopeMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseEnvelopeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip Swagger and non-JSON responses
        var path = context.Request.Path.Value?.ToLower();
        if (path is not null && (path.Contains("swagger") || path.Contains("index.html")))
        {
            await _next(context);
            return;
        }

        // Capture the original response body
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        // If an error occurred, let ExceptionMiddleware handle it
        if (context.Response.StatusCode >= 400)
        {
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
            return;
        }

        // Read the original response content
        memStream.Position = 0;
        var bodyText = await new StreamReader(memStream).ReadToEndAsync();

        // If empty or not JSON, skip wrapping
        if (string.IsNullOrWhiteSpace(bodyText) || !context.Response.ContentType?.Contains("application/json") == true)
        {
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
            return;
        }

        // Build the envelope
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var envelope = new
        {
            data = JsonSerializer.Deserialize<object>(bodyText),
            correlationId
        };

        var json = JsonSerializer.Serialize(envelope);

        // Write wrapped response
        context.Response.Body = originalBody;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json, Encoding.UTF8);
    }
}