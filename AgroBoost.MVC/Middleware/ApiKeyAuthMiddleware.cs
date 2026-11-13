using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AgroBoost.MVC.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _validApiKey;
    private const string ApiKeyHeader = "X-Api-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _validApiKey = configuration["ApiKey:ValidKey"] ?? "AgroBoost-2024-Public-Catalog";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) || 
                string.IsNullOrEmpty(providedKey) || 
                providedKey != _validApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new { error = "Invalid or missing API Key" };
                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }
        }

        await _next(context);
    }
}