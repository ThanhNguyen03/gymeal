namespace Gymeal.Domain.ValueObjects;

/// <summary>
/// Represents daily macro and calorie targets for a user.
/// Immutable value object — create a new instance to update targets.
/// </summary>
public record MacroProfile(
    int DailyCalorieTarget,
    int ProteinTargetG,
    int CarbsTargetG,
    int FatTargetG);
