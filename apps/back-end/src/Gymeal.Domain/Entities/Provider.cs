namespace Gymeal.Domain.Entities;

public sealed class Provider : BaseEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public List<string> CuisineTags { get; set; } = [];
    public bool IsVerified { get; set; }
    public decimal Rating { get; set; }
    public int TotalOrders { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Meal> Meals { get; set; } = [];
}
