using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>Authentication and authorization entity. Owns a 1:1 UserProfile.</summary>
public sealed class User : BaseEntity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public EUserRole Role { get; set; } = EUserRole.Customer;
    public bool IsVerified { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public UserProfile? Profile { get; set; }
}
