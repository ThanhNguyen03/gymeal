using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>
/// RAG knowledge base chunk. Seeded from USDA, NIH, and curated nutrition docs.
/// Embedded via Groq nomic-embed and stored as pgvector for similarity search.
/// </summary>
/// <remarks>
/// DECISION: EF Core owns this table's schema even though Python reads/writes rows.
/// Reason: Single migration owner prevents two tools conflicting on the same DB.
/// Python ai-service uses SQLAlchemy ORM for read/write — no Alembic.
/// Reference: PLAN.md §10 (Migration Ownership Rule)
/// </remarks>
public sealed class NutritionDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public ENutritionDocumentCategory Category { get; set; }

    /// <summary>
    /// 768-dimension embedding vector for pgvector cosine similarity search.
    /// Mapped as vector(768) column — populated by Python seed script, not C#.
    /// </summary>
    public float[]? Embedding { get; set; }
}
