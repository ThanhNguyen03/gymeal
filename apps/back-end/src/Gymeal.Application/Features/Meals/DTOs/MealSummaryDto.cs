using Gymeal.Domain.Enums;

namespace Gymeal.Application.Features.Meals.DTOs;

// Lightweight DTO for list/grid views — omits macro details and tags.
public record MealSummaryDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    EMealCategory Category,
    int Calories,
    decimal ProteinG,
    int PriceInCents,
    string ProviderName,
    bool IsAvailable);
