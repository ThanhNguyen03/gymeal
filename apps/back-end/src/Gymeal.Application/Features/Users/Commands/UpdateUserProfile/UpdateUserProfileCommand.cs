using Gymeal.Application.Common.Behaviours;
using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Users.DTOs;
using Gymeal.Domain.Enums;
using MediatR;

namespace Gymeal.Application.Features.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(
    string? FullName,
    int? Age,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? BodyFatPct,
    EFitnessGoalType? FitnessGoal,
    EActivityLevel? ActivityLevel,
    List<string>? DietaryRestrictions,
    List<string>? Allergies,
    int? DailyCalorieTarget,
    int? ProteinTargetG) : IRequest<Result<UserProfileDto>>, IAuditableCommand;
