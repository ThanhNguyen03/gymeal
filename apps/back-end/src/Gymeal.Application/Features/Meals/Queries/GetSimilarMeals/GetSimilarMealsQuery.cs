using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.GetSimilarMeals;

public sealed record GetSimilarMealsQuery(Guid MealId, int First) : IRequest<Result<IReadOnlyList<MealSummaryDto>>>;
