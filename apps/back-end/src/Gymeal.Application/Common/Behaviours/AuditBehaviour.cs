using MediatR;
using Microsoft.Extensions.Logging;

namespace Gymeal.Application.Common.Behaviours;

/// <summary>
/// Marker interface for commands that should be recorded in the audit_logs table.
/// Implement this on any command that mutates user-generated data.
/// The actual audit writing is done by AppDbContext.SaveChangesAsync (Infrastructure layer).
/// </summary>
public interface IAuditableCommand { }

/// <summary>
/// MediatR pipeline behaviour that ensures auditable commands are logged.
/// This behaviour is intentionally lightweight — it marks the operation as auditable
/// so that downstream infrastructure (AppDbContext) can capture the change trail.
/// </summary>
/// <remarks>
/// NOTE: The audit writing itself happens in AppDbContext.SaveChangesAsync using
/// EF Core's ChangeTracker. This behaviour's role is to pass through commands that
/// implement IAuditableCommand, confirming they are intentional mutation operations.
///
/// This design keeps the Application layer clean — no direct ChangeTracker access here.
/// Reference: PLAN.md §4.6
/// </remarks>
public sealed class AuditBehaviour<TRequest, TResponse>(Microsoft.Extensions.Logging.ILogger<AuditBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Auditable command: {CommandName}", typeof(TRequest).Name);
        return await next();
    }
}
