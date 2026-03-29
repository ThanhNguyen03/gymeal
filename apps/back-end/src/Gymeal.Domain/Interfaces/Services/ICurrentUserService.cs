namespace Gymeal.Domain.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    string? IpAddress { get; }
}
