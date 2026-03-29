using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>
/// Immutable record of every create/update/delete on audited entities.
/// Written automatically by AppDbContext.SaveChangesAsync — never write manually.
/// </summary>
/// <remarks>
/// NOTE: AuditLog does NOT implement ISoftDeletable — audit records are never deleted.
/// Reason: The entire point of an audit trail is permanent, tamper-evident history.
/// Reference: PLAN.md §4.6
/// </remarks>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public EAuditAction Action { get; set; }

    /// <summary>JSON diff: { "before": {...}, "after": {...} }</summary>
    public string Changes { get; set; } = "{}";

    public string? IpAddress { get; set; }
}
