using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>
/// Fitness profile for a user. UserId is both the PK and FK to users.
/// Created with minimal data at registration; completed via the profile wizard.
/// </summary>
public class UserProfile
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? BodyFatPct { get; set; }
    public EFitnessGoalType FitnessGoal { get; set; } = EFitnessGoalType.Maintain;
    public EActivityLevel ActivityLevel { get; set; } = EActivityLevel.ModeratelyActive;
    public List<string> DietaryRestrictions { get; set; } = [];
    public List<string> Allergies { get; set; } = [];
    public int DailyCalorieTarget { get; set; }
    public int ProteinTargetG { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
