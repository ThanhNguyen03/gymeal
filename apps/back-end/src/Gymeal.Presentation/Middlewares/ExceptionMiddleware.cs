using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Middlewares;

/// <summary>
/// Global exception handler. Catches all unhandled exceptions and returns
/// RFC 7807 Problem Details JSON. Reports to Sentry in production.
/// </summary>
sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                context.Items["CorrelationId"]);

            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
    {
        int statusCode = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        ProblemDetails problem = new()
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
            Instance = context.Request.Path,
            Extensions = { ["correlationId"] = context.Items["CorrelationId"] }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        );
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        429 => "Too Many Requests",
        _ => "Internal Server Error"
    };
}
