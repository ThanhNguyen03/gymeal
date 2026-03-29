namespace Gymeal.Domain.Entities;

/// <summary>
/// Aggregated preference vector derived from a user's behavior history.
/// One row per user (UserId is PK). Recomputed by QStash daily cron.
/// Used for personalized meal recommendations (Sprint 5).
/// </summary>
public class UserPreferenceEmbedding
{
    public Guid UserId { get; set; }

    /// <summary>768-dimension preference vector — pgvector cosine similarity for recommendations.</summary>
    public float[]? PreferenceVector { get; set; }

    public int InteractionCount { get; set; }
    public DateTime ComputedAt { get; set; }
}
