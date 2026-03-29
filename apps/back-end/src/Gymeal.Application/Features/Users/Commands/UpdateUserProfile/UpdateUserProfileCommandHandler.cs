using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Users.DTOs;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime) : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Error.Unauthorized();
        }

        User? user = await userRepository.GetByIdAsync(currentUser.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User", currentUser.UserId.Value);
        }

        // Ensure profile exists (created at registration with minimal data)
        user.Profile ??= new UserProfile { UserId = user.Id };
        UserProfile profile = user.Profile;

        // Apply only the fields that were provided (partial update)
        if (request.FullName is not null) profile.FullName = request.FullName;
        if (request.Age.HasValue) profile.Age = request.Age;
        if (request.WeightKg.HasValue) profile.WeightKg = request.WeightKg;
        if (request.HeightCm.HasValue) profile.HeightCm = request.HeightCm;
        if (request.BodyFatPct.HasValue) profile.BodyFatPct = request.BodyFatPct;
        if (request.FitnessGoal.HasValue) profile.FitnessGoal = request.FitnessGoal.Value;
        if (request.ActivityLevel.HasValue) profile.ActivityLevel = request.ActivityLevel.Value;
        if (request.DietaryRestrictions is not null) profile.DietaryRestrictions = request.DietaryRestrictions;
        if (request.Allergies is not null) profile.Allergies = request.Allergies;
        if (request.DailyCalorieTarget.HasValue) profile.DailyCalorieTarget = request.DailyCalorieTarget.Value;
        if (request.ProteinTargetG.HasValue) profile.ProteinTargetG = request.ProteinTargetG.Value;

        profile.UpdatedAt = dateTime.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        return new UserProfileDto(
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString(),
            FullName: profile.FullName,
            AvatarUrl: profile.AvatarUrl,
            Age: profile.Age,
            WeightKg: profile.WeightKg,
            HeightCm: profile.HeightCm,
            BodyFatPct: profile.BodyFatPct,
            FitnessGoal: profile.FitnessGoal,
            ActivityLevel: profile.ActivityLevel,
            DietaryRestrictions: profile.DietaryRestrictions,
            Allergies: profile.Allergies,
            DailyCalorieTarget: profile.DailyCalorieTarget,
            ProteinTargetG: profile.ProteinTargetG);
    }
}
