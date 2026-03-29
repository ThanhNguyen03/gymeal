using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

// NOTE: MealRequest does NOT implement ISoftDeletable — these are immutable order records.
// Deleting a request would create audit gaps. Status changes track lifecycle instead.
public sealed class MealRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public EMealRequestStatus Status { get; set; } = EMealRequestStatus.Pending;
    public string? ResponseMessage { get; set; }
    public int? QuotePriceInCents { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
