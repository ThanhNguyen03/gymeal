using Gymeal.Application.Common.Pagination;
using Gymeal.Application.Features.Meals.DTOs;

namespace Gymeal.Infrastructure.GraphQL.Types;

[ObjectType<PagedResult<MealSummaryDto>>]
public static partial class TypePagedMeals
{
    static partial void Configure(IObjectTypeDescriptor<PagedResult<MealSummaryDto>> descriptor)
    {
        descriptor.Name("PagedMeals");
        descriptor.Description("Offset-paginated list of meals.");
    }
}
