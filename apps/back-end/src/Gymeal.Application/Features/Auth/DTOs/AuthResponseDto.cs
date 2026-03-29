namespace Gymeal.Application.Features.Auth.DTOs;

/// <summary>Returned after successful register, login, or token refresh.</summary>
public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string Role);
