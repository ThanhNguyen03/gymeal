using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Gymeal.Application.Common.Interfaces;

/// <summary>
/// Minimal interface exposing only what the Application layer needs from the DbContext.
/// Keeps Application layer decoupled from EF Core's concrete AppDbContext.
/// </summary>
/// <remarks>
/// TRADE-OFF: Exposing ChangeTracker here is a deliberate breach of strict Clean Architecture.
/// The alternative (domain events + infrastructure event handlers) adds significant complexity
/// for a single-developer project. This interface is kept minimal and documented.
/// Reference: PLAN.md §4.6 (AuditBehaviour design)
/// </remarks>
public interface IAppDbContext
{
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
