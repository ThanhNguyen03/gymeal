using System.Security.Claims;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Gymeal.Presentation.Services;

/// <summary>
/// Resolves the authenticated user's ID and IP address from the current HTTP context.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public sealed class ServiceCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            string? sub = httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User
                .FindFirstValue("sub");

            return Guid.TryParse(sub, out Guid userId) ? userId : null;
        }
    }

    public string? Role =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
