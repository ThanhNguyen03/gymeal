# GraphQL Setup — Hot Chocolate

## Why Hot Chocolate

Hot Chocolate was chosen over Apollo Server (JS) for:
1. Native .NET — no Node.js runtime, shares ASP.NET Core DI and middleware pipeline
2. Built-in DataLoader for N+1 prevention
3. Built-in WebSocket subscriptions (`graphql-transport-ws`)
4. Authorization integrated with ASP.NET Core `[Authorize]`

---

## Schema Registration (Program.cs)

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddTypeExtension<QueryMeals>()
    .AddTypeExtension<QueryProviders>()
    .AddType<TypeMeal>()
    .AddType<TypeProvider>()
    .AddDataLoader<DataLoaderProviderById>()
    .AddDataLoader<DataLoaderMealsByProviderId>()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization();
```

Endpoint: `POST /graphql` (GraphQL Playground available in development)

---

## Query Types

### QueryMeals (`[ExtendObjectType("Query")]`)

| Field | Args | Returns | Auth |
|-------|------|---------|------|
| `getMealsPaged` | `page: Int!, pageSize: Int!` | `PagedMeals` | Public |
| `getMealById` | `id: UUID!` | `Meal` | Public |
| `searchMeals` | `query: String!, limit: Int!` | `[MealSummary]` | Public; Redis-cached 5 min |
| `getSimilarMeals` | `mealId: UUID!, first: Int!` | `[MealSummary]` | Public; pgvector cosine distance |

### QueryProviders (`[ExtendObjectType("Query")]`)

| Field | Args | Returns | Auth |
|-------|------|---------|------|
| `getProviders` | `page: Int!, pageSize: Int!` | `PagedProviders` | Public; verified only |
| `getProviderById` | `id: UUID!` | `Provider` | Public |

---

## GraphQL Types

### TypeMeal

Maps `Meal` entity. **`embedding` field is explicitly ignored** — pgvector column must never be exposed in API responses.

`provider` field uses `DataLoaderProviderById` to prevent N+1.

### TypeProvider

Maps `Provider` entity. `meals` field uses `DataLoaderMealsByProviderId` to prevent N+1.

---

## DataLoaders (N+1 Prevention)

| DataLoader | Batches | Strategy |
|-----------|---------|---------|
| `DataLoaderProviderById` | `BatchDataLoader<Guid, Provider>` | Loads providers by IDs in single `WHERE id IN (...)` query |
| `DataLoaderMealsByProviderId` | `GroupedDataLoader<Guid, Meal>` | Loads meals grouped by provider ID in single query |

**Without DataLoader:** 20 providers → 20 separate SQL queries for provider data.
**With DataLoader:** 20 providers → 1 SQL query batching all 20 IDs.

---

## Security Notes

- `embedding` column is **never** included in GraphQL schema — intentionally ignored in `TypeMeal`
- GraphQL endpoint inherits ASP.NET Core authentication middleware — JWT cookie auth applies
- Future mutations will use `[Authorize]` attribute per resolver
