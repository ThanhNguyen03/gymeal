namespace Gymeal.Application.Features.Providers.DTOs;

public record ProviderSummaryDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    IReadOnlyList<string> CuisineTags,
    decimal Rating,
    int MealCount);
