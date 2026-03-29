using Gymeal.Application.Features.Meals.DTOs;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Features.Meals.Queries.SearchMeals;

public sealed record SearchMealsQuery(string Query, int Limit) : IRequest<Result<IReadOnlyList<MealSummaryDto>>>;
