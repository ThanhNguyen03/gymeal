using Gymeal.Domain.Enums;

namespace Gymeal.Application.Features.Meals.DTOs;

// NOTE: Embedding is intentionally excluded — internal pgvector column, never exposed in API.
public record MealDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string Name,
    string Description,
    string? ImageUrl,
    EMealCategory Category,
    int PriceInCents,
    int Calories,
    decimal ProteinG,
    decimal CarbsG,
    decimal FatG,
    decimal FiberG,
    IReadOnlyList<string> Ingredients,
    IReadOnlyList<string> AllergenTags,
    IReadOnlyList<string> FitnessGoalTags,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime UpdatedAt);
