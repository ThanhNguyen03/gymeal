using Gymeal.Application.Common.Behaviours;
using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using Gymeal.Domain.Enums;
using MediatR;

namespace Gymeal.Application.Features.Meals.Commands.CreateMeal;

public sealed record CreateMealCommand(
    string Name,
    string Description,
    string? ImageUrl,
    EMealCategory Category,
    int PriceInCents,
    int Calories,
    decimal ProteinG,
    decimal CarbsG,
    decimal FatG,
    decimal FiberG,
    List<string> Ingredients,
    List<string> AllergenTags,
    List<string> FitnessGoalTags) : IRequest<Result<MealDto>>, IAuditableCommand;
