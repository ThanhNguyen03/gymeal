# Application Layer

The Application layer implements all CQRS use-cases via MediatR. It depends only on Domain — no EF Core, no Redis, no HTTP.

---

## Pagination

`Gymeal.Application.Common.Pagination.PagedResult<T>`

Offset pagination response wrapper used by list queries.

| Property | Type | Notes |
|----------|------|-------|
| Items | IReadOnlyList\<T\> | |
| TotalCount | int | Total rows matching the filter |
| Page | int | 1-indexed current page |
| PageSize | int | Items per page (1-50) |
| HasNextPage | bool | Computed: `Page * PageSize < TotalCount` |

---

## DTOs

### Meal DTOs (`Features/Meals/DTOs/`)

| DTO | Used By | Notes |
|-----|---------|-------|
| `MealDto` | `GetMealById`, `CreateMeal`, `UpdateMeal` | Full data; **embedding excluded** |
| `MealSummaryDto` | `GetMeals`, `SearchMeals`, `GetSimilarMeals` | Lightweight for list/grid views |

### Provider DTOs (`Features/Providers/DTOs/`)

| DTO | Used By | Notes |
|-----|---------|-------|
| `ProviderDto` | `GetProviderById`, `VerifyProvider` | All provider fields |
| `ProviderSummaryDto` | `GetProviders` | Lightweight; includes `MealCount` |

---

## Meal Features

### Queries

| Query | Returns | Validation |
|-------|---------|------------|
| `GetMealsQuery(Page, PageSize)` | `PagedResult<MealSummaryDto>` | Page ≥ 1, PageSize 1-50 |
| `GetMealByIdQuery(Id)` | `MealDto` | Id non-empty |
| `SearchMealsQuery(Query, Limit)` | `IReadOnlyList<MealSummaryDto>` | Query 2-100 chars, Limit 1-50; Redis cached 5 min |
| `GetSimilarMealsQuery(MealId, First)` | `IReadOnlyList<MealSummaryDto>` | First 1-20; verifies source meal exists first |

**Search caching:** `SearchMealsQueryHandler` checks Redis with key `search:{normalized_query}:{limit}` before hitting the DB. Normalized to lowercase/trimmed to prevent cache poisoning.

### Commands

| Command | Auth | Audit | Notes |
|---------|------|-------|-------|
| `CreateMealCommand` | Provider role | ✅ `IAuditableCommand` | Verifies caller owns a provider account |
| `UpdateMealCommand` | Provider, owns meal | ✅ | Partial update; invalidates `search:*` cache prefix |
| `ToggleMealAvailabilityCommand` | Provider, owns meal | ✅ | Flips `IsAvailable`; returns new state |

---

## Provider Features

### Queries

| Query | Returns | Validation |
|-------|---------|------------|
| `GetProvidersQuery(Page, PageSize)` | `PagedResult<ProviderSummaryDto>` | Filters `IsVerified = true` by default |
| `GetProviderByIdQuery(Id)` | `ProviderDto` | Id non-empty |

### Commands

| Command | Auth | Audit | Notes |
|---------|------|-------|-------|
| `VerifyProviderCommand(ProviderId)` | Admin only | ✅ | Defense-in-depth: controller `[Authorize(Roles="Admin")]` + handler re-check |

---

## ICurrentUserService Changes

Added `Role` property (Sprint 2) to support Admin checks in handlers:

```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }       // NEW in Sprint 2
    string? IpAddress { get; }
}
```

`ServiceCurrentUser` resolves `Role` from `ClaimTypes.Role` in the JWT.
