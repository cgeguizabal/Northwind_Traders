using System.Net;
using System.Text.Json;

namespace NorthwindTraders.API.Middleware;

// ASP.NET Core middleware that catches any unhandled exception in the pipeline
// and returns a structured JSON error response instead of a raw 500 HTML page.
// Registered in Program.cs with app.UseMiddleware<GlobalExceptionHandlingMiddleware>().
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    // Called for every HTTP request — passes control to the next middleware in the pipeline
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Switch expression \u2014 maps known exception types to appropriate HTTP status codes
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException       => (HttpStatusCode.NotFound,            "The requested resource was not found."),
            ArgumentException          => (HttpStatusCode.BadRequest,          "Invalid request data."),
            OperationCanceledException => ((HttpStatusCode)499,                "Request cancelled."),
            _                          => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        // traceId \u2014 links this response to the server log entry for easier debugging
        var body = JsonSerializer.Serialize(new
        {
            statusCode = (int)statusCode,
            message,
            traceId = context.TraceIdentifier
        });

        return context.Response.WriteAsync(body);
    }
}
