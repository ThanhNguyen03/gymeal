using Gymeal.Domain.Enums;

namespace Gymeal.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid UserId,
    string Email,
    string Role,
    string? FullName,
    string? AvatarUrl,
    int? Age,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? BodyFatPct,
    EFitnessGoalType FitnessGoal,
    EActivityLevel ActivityLevel,
    List<string> DietaryRestrictions,
    List<string> Allergies,
    int DailyCalorieTarget,
    int ProteinTargetG);
