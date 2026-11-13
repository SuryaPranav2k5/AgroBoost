using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AgroBoost.MVC.Middleware;

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
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userId = context.User?.Identity?.IsAuthenticated == true 
            ? context.User.FindFirst("sub")?.Value ?? context.User.Identity?.Name 
            : "Anonymous";

        _logger.LogInformation("Request started: {Method} {Path} by {UserId}", 
            requestMethod, requestPath, userId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - User: {UserId}",
                requestMethod, requestPath, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, userId);
        }
    }
}