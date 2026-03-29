# Infrastructure Layer

The Infrastructure layer wires everything to external systems: PostgreSQL (via EF Core), Redis, and Cloudinary. It implements all Domain interfaces. Application layer depends on abstractions only — never on Infrastructure directly.

---

## EF Core Configurations

Located in `Persistence/Configurations/`. Each entity has a dedicated `Configuration{Entity}.cs`.

### ConfigurationProvider

Table: `providers`

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK |
| user_id | uuid | FK → users; unique index `ix_providers_user_id` |
| name | varchar(200) | |
| cuisine_tags | text[] | GIN index `gin_providers_cuisine_tags` |
| is_verified | boolean | Default false |
| deleted_at | timestamp | Soft delete filter |

### ConfigurationMeal

Table: `meals`

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK |
| provider_id | uuid | FK → providers; index `ix_meals_provider_id` |
| price_in_cents | integer | Money as cents — no float precision bugs |
| ingredients / allergen_tags / fitness_goal_tags | text[] | Array columns |
| embedding | vector(768) | pgvector; IVFFlat index `ivfflat_meals_embedding` |
| name / description | text | GIN trigram indexes for pg_trgm search |
| is_available | boolean | Partial index `ix_meals_available WHERE is_available = true` |
| deleted_at | timestamp | Soft delete filter |

**Required extensions in migration:**
```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS vector;
```

### ConfigurationMealRequest

Table: `meal_requests` — No soft delete (immutable order records).

---

## Repositories

| Class | Interface | Key Design Decisions |
|-------|-----------|----------------------|
| `RepositoryMeal` | `IMealRepository` | `GetPagedAsync` and `GetByIdAsync` use `.Include(m => m.Provider)` — N+1 prevention; `GetSimilarAsync` uses `FromSqlInterpolated` for pgvector cosine distance |
| `RepositoryProvider` | `IProviderRepository` | `GetVerifiedPagedAsync` filters `IsVerified = true`; `VerifyAsync` sets `IsVerified = true` atomically |
| `RepositoryUser` | `IUserRepository` | Sprint 1 — see `docs/02-solution-structure.md` |

All methods return `Result<T>` (RULE.md §5.6).

---

## Services

### ServiceTrgmSearch

Implements `ISearchService`. Runs pg_trgm `word_similarity` queries via `FromSqlInterpolated` (parameterized). Similarity threshold: 0.15. Required extension: `pg_trgm`.

**SECURITY:** `FromSqlInterpolated` produces parameterized SQL — never string concatenation.

### ServiceRedisCache

Implements `ICacheService`. Uses `IDistributedCache` for get/set/remove, and `IConnectionMultiplexer` directly for prefix-based key deletion (SCAN + DEL). `IDistributedCache` doesn't expose SCAN operations.

Default TTL: 5 minutes. Search result keys: `search:{normalized_query}:{limit}`.

### ServiceCloudinaryStorage

Implements `IStorageService`. Generates server-side signed upload URLs — Cloudinary credentials never leave the server.

**SECURITY:** File extension whitelist: `.jpg`, `.jpeg`, `.png`, `.webp`. No SVG (XSS risk).

---

## Dependency Registration

`DependencyInjection.cs` registers:

```csharp
// Repositories
services.AddScoped<IUserRepository, RepositoryUser>();
services.AddScoped<IMealRepository, RepositoryMeal>();
services.AddScoped<IProviderRepository, RepositoryProvider>();

// Services
services.AddScoped<ISearchService, ServiceTrgmSearch>();
services.AddScoped<ICacheService, ServiceRedisCache>();
services.AddScoped<IStorageService, ServiceCloudinaryStorage>();

// Redis (IConnectionMultiplexer for prefix scan)
services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisUrl));
```

### Required Environment Variables

| Variable | Purpose |
|----------|---------|
| `DATABASE_URL` | PostgreSQL connection string |
| `REDIS_URL` | Redis connection string |
| `CLOUDINARY_CLOUD_NAME` | Cloudinary cloud name |
| `CLOUDINARY_API_KEY` | Cloudinary API key |
| `CLOUDINARY_API_SECRET` | Cloudinary API secret (never in source) |
