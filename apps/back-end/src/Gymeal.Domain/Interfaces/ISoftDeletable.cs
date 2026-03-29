namespace Gymeal.Domain.Entities;

/// <summary>
/// Marks an entity as soft-deletable.
/// EF Core global query filter in AppDbContext excludes rows where DeletedAt is not null.
/// </summary>
/// <remarks>
/// DECISION: Soft deletes chosen over hard deletes for all user-generated data.
/// Reason: Hard deletes destroy audit trails and break referential integrity.
/// Accidental deletions are unrecoverable. Soft delete preserves data for auditing,
/// compliance, and recovery while hiding it from normal application queries.
/// Reference: PLAN.md §4.5
/// </remarks>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }

    void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}
