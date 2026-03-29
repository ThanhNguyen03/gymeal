using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

public sealed class Meal : BaseEntity, ISoftDeletable
{
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public EMealCategory Category { get; set; }

    // SECURITY: Store price as integer cents to avoid floating-point precision issues.
    // 1999 cents = $19.99 — always exact. Never use decimal for money in financial operations.
    public int PriceInCents { get; set; }

    public int Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
    public decimal FiberG { get; set; }
    public List<string> Ingredients { get; set; } = [];
    public List<string> AllergenTags { get; set; } = [];
    public List<string> FitnessGoalTags { get; set; } = [];
    public bool IsAvailable { get; set; } = true;

    // NOTE: Embedding is internal only — never expose via GraphQL or REST responses.
    // Used exclusively for pgvector cosine similarity queries.
    public float[]? Embedding { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Provider Provider { get; set; } = null!;
}
