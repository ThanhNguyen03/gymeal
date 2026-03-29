using MediatR;
using Microsoft.Extensions.Logging;

namespace Gymeal.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs request entry/exit and execution time.
/// Runs first in the pipeline — before validation and audit.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}", requestName);

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        TResponse response = await next();

        stopwatch.Stop();

        logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
