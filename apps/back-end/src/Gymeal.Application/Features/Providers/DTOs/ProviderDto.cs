namespace Gymeal.Application.Features.Providers.DTOs;

public record ProviderDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    string? LogoUrl,
    IReadOnlyList<string> CuisineTags,
    bool IsVerified,
    decimal Rating,
    int TotalOrders,
    DateTime CreatedAt);
