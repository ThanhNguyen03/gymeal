namespace Gymeal.Presentation.Middlewares;

/// <summary>
/// Reads X-Correlation-Id from the incoming request.
/// If absent, generates a new UUID v4 and assigns it.
/// Propagates the correlation ID to the response header and
/// stores it in HttpContext.Items for downstream handlers and logs.
/// </summary>
/// <remarks>
/// NOTE: CancellationToken allows the request to be cancelled if the user navigates away
/// or the HTTP connection drops. Without it, abandoned requests waste server resources.
/// </remarks>
sealed class MiddlewareCorrelationId(RequestDelegate next)
{
    private const string CORRELATION_ID_HEADER = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        string correlationId = GetOrCreateCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CORRELATION_ID_HEADER] = correlationId;

        // Attach to logging scope so correlation ID appears in every log line
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CORRELATION_ID_HEADER, out Microsoft.Extensions.Primitives.StringValues existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
