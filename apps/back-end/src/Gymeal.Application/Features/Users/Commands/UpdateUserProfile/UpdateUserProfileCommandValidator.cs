using FluentValidation;

namespace Gymeal.Application.Features.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Age)
            .InclusiveBetween(13, 100).WithMessage("Age must be between 13 and 100.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(30m, 300m).WithMessage("Weight must be between 30 and 300 kg.")
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(100m, 250m).WithMessage("Height must be between 100 and 250 cm.")
            .When(x => x.HeightCm.HasValue);

        RuleFor(x => x.BodyFatPct)
            .InclusiveBetween(3m, 60m).WithMessage("Body fat percentage must be between 3% and 60%.")
            .When(x => x.BodyFatPct.HasValue);

        RuleFor(x => x.DailyCalorieTarget)
            .InclusiveBetween(800, 10000).WithMessage("Daily calorie target must be between 800 and 10000.")
            .When(x => x.DailyCalorieTarget.HasValue);

        RuleFor(x => x.ProteinTargetG)
            .InclusiveBetween(20, 500).WithMessage("Protein target must be between 20g and 500g.")
            .When(x => x.ProteinTargetG.HasValue);

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Full name must be at most 100 characters.")
            .When(x => x.FullName is not null);
    }
}
