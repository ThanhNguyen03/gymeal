using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>
/// Tracks user interactions with meals (views, orders, searches, dismissals).
/// Feeds the preference learner to compute UserPreferenceEmbedding (Sprint 5).
/// </summary>
public class UserBehaviorEvent : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? MealId { get; set; }
    public EBehaviorEventType EventType { get; set; }

    /// <summary>Contextual metadata as JSON (e.g., search query, recommendation source).</summary>
    public string? Metadata { get; set; }

    public DateTime OccurredAt { get; set; }
}
