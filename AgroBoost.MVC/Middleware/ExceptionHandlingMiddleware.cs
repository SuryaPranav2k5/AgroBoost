using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AgroBoost.MVC.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var isApiRequest = context.Request.Path.StartsWithSegments("/api");

        if (isApiRequest)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message,
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        else
        {
            // A redirect is issued as 302. Do NOT set StatusCode first: once the status
            // code is written the response has started, and Redirect() then throws
            // "StatusCode cannot be set because the response has already started".
            // If the response has genuinely started already, all we can do is stop.
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Redirect("/Home/Error");
        }
    }
}