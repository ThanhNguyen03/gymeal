namespace Gymeal.Domain.Entities;

/// <summary>
/// Base class for all domain entities. Provides identity and audit timestamps.
/// </summary>
/// <remarks>
/// NOTE: CreatedAt and UpdatedAt are set automatically by AppDbContext.SaveChangesAsync.
/// Entities should never set these manually.
/// </remarks>
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
