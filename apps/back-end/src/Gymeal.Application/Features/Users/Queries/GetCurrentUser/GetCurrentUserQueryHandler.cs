using Gymeal.Application.Common.Errors;
using Gymeal.Application.Features.Users.DTOs;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Interfaces.Repositories;
using Gymeal.Domain.Interfaces.Services;
using MediatR;

namespace Gymeal.Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUser) : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        GetCurrentUserQuery request,
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

        UserProfile profile = user.Profile ?? new UserProfile { UserId = user.Id };

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
