# Domain Layer

## Domain.Common — Error & Result\<T\>

`Gymeal.Domain.Common` houses the two foundational value types used everywhere:

### Error

```csharp
public record Error(string Code, string Message)
```

Static factory methods: `NotFound`, `Validation`, `Unauthorized`, `Conflict`, `Forbidden`.
`Error.None` represents the absence of an error (used internally by `Result<T>`).

### Result\<T\>

```csharp
public sealed class Result<TValue>
```

All repository interface methods return `Result<T>` (RULE.md §5.6). Callers pattern-match:

```csharp
Result<User> result = await userRepository.GetByIdAsync(id, ct);
if (result.IsFailure) return result.Error;   // propagate
User user = result.Value;                    // safe — only after IsSuccess check
```

**Why Domain, not Application?** `IRepository` interfaces live in Domain. If `Result<T>` lived in Application, Domain would have to reference Application — a Clean Architecture violation. Placing `Result<T>` in Domain keeps the dependency arrow pointing inward.

**Backwards compatibility:** `Gymeal.Application/Common/Errors/Error.cs` contains `global using Error = Gymeal.Domain.Common.Error;` so legacy Application code continues to compile without modification.

---

## BaseEntity

All persistent entities (except `UserProfile`, which uses `UserId` as its PK) extend `BaseEntity`:

```csharp
public abstract class BaseEntity {
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }  // set by AppDbContext.SaveChangesAsync
    public DateTime UpdatedAt { get; set; }  // updated on every save
}
```

## ISoftDeletable

Entities that support soft-delete implement this interface:

```csharp
public interface ISoftDeletable {
    DateTime? DeletedAt { get; set; }
    void SoftDelete() => DeletedAt = DateTime.UtcNow;
}
```

`AppDbContext.OnModelCreating` applies `HasQueryFilter(e => e.DeletedAt == null)` to every `ISoftDeletable` entity via reflection. Deleted records are invisible to all queries unless you call `.IgnoreQueryFilters()`.

**Currently soft-deletable**: `User`
**Not soft-deletable**: `AuditLog` (audit records must never be deleted)

## Entities

### User
Core identity entity.

| Property | Type | Notes |
|----------|------|-------|
| Email | string | Unique, lowercase, max 320 chars |
| PasswordHash | string | BCrypt hash (work factor 12) |
| Role | UserRole | Default: Customer |
| IsVerified | bool | Email verification (Sprint 2) |
| DeletedAt | DateTime? | ISoftDeletable |

### UserProfile
One-to-one with User (UserId is PK + FK). Created with default values during registration; completed via the profile wizard.

| Property | Type | Notes |
|----------|------|-------|
| FullName | string | |
| Age | int? | |
| WeightKg | decimal? | Precision 5,2 |
| HeightCm | decimal? | Precision 5,1 |
| BodyFatPct | decimal? | Precision 4,1 |
| FitnessGoal | string? | FitnessGoalType enum value |
| ActivityLevel | string? | ActivityLevel enum value |
| DietaryRestrictions | List\<string\> | Stored as PostgreSQL `text[]` |
| Allergies | List\<string\> | Stored as PostgreSQL `text[]` |
| DailyCalorieTarget | int? | |
| ProteinTargetG | int? | |

### AuditLog
Immutable record of every change to auditable entities.

| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | Who made the change |
| EntityType | string | e.g. "UserProfile" |
| EntityId | Guid | The changed entity's PK |
| Action | AuditAction | Create / Update / Delete |
| Changes | string | JSON diff (`{"field": {"from": ..., "to": ...}}`) |
| IpAddress | string? | From request |

PasswordHash is explicitly excluded from Changes to prevent credential leakage in audit logs.

### Provider
Business entity that sells meals. Owned by a `User` with `Provider` role.

| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK → users, unique |
| Name | string | Display name |
| Description | string | |
| LogoUrl | string? | |
| CuisineTags | List\<string\> | Stored as `text[]`, GIN index |
| IsVerified | bool | Set by Admin via VerifyProviderCommand |
| Rating | decimal | Computed; updated on order completion |
| TotalOrders | int | Denormalized counter |
| DeletedAt | DateTime? | ISoftDeletable |

### Meal
Core catalog entity. Nutritional data + vector embedding for AI similarity search.

| Property | Type | Notes |
|----------|------|-------|
| ProviderId | Guid | FK → providers |
| Name | string | Max 200 chars; GIN trigram index |
| Description | string | GIN trigram index |
| ImageUrl | string? | Cloudinary URL |
| Category | EMealCategory | |
| PriceInCents | int | **Integer cents** — avoids float precision bugs |
| Calories | int | |
| ProteinG / CarbsG / FatG / FiberG | decimal | |
| Ingredients / AllergenTags / FitnessGoalTags | List\<string\> | `text[]` columns |
| IsAvailable | bool | Partial index `WHERE is_available = true` |
| Embedding | float[]? | `vector(768)` — **never exposed in API** |
| DeletedAt | DateTime? | ISoftDeletable |

### MealRequest
Custom order request from customer to provider. Immutable record (no soft delete — status field tracks lifecycle).

| Property | Type | Notes |
|----------|------|-------|
| UserId | Guid | FK → users |
| ProviderId | Guid | FK → providers |
| Description | string | Customer's request details |
| Status | EMealRequestStatus | Pending → Accepted/Rejected → Completed |
| ResponseMessage | string? | Provider's reply |
| QuotePriceInCents | int? | Provider's quoted price |

## Enums

| Enum | Values |
|------|--------|
| EUserRole | Customer, Provider, Admin |
| EFitnessGoalType | Cut, Bulk, Maintain, Endurance, Recomp |
| EActivityLevel | Sedentary, LightlyActive, ModeratelyActive, VeryActive, ExtremelyActive |
| EAuditAction | Create, Update, Delete |
| EMealCategory | Breakfast, Lunch, Dinner, Snack, PreWorkout, PostWorkout |
| EMealRequestStatus | Pending, Accepted, Rejected, Completed |
| EMealTime | Breakfast, Lunch, Dinner, Snack, PreWorkout, PostWorkout |
| EOrderStatus | Pending, Confirmed, Preparing, Ready, Delivered, Cancelled |
| ENutritionDocumentCategory | Macronutrient, Micronutrient, SportNutrition, DietaryGuideline, GoalSpecific |
| EBehaviorEventType | MealViewed, MealOrdered, MealSearched, MealDismissed, ChatAsked, RecommendationClicked |
| EFeedbackSentiment | ThumbsUp, ThumbsDown |

## Repository Interfaces

All methods return `Result<T>` (RULE.md §5.6). Located in `Gymeal.Domain/Interfaces/Repositories/`.

| Interface | Key Methods |
|-----------|-------------|
| `IUserRepository` | `GetByIdAsync`, `GetByEmailAsync`, `ExistsAsync`, `AddAsync`, `UpdateAsync` |
| `IMealRepository` | `GetByIdAsync`, `GetPagedAsync`, `SearchAsync`, `GetSimilarAsync`, `AddAsync`, `UpdateAsync`, `CountAsync` |
| `IProviderRepository` | `GetByIdAsync`, `GetByUserIdAsync`, `GetVerifiedPagedAsync`, `AddAsync`, `UpdateAsync`, `VerifyAsync`, `CountVerifiedAsync` |

## Service Interfaces

Located in `Gymeal.Domain/Interfaces/Services/`.

| Interface | Purpose |
|-----------|---------|
| `ISearchService` | pg_trgm full-text search — `SearchMealsAsync(query, limit)` |
| `ICacheService` | Redis abstraction — `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `RemoveByPrefixAsync` |
| `IStorageService` | Cloudinary signed upload URLs — `GetSignedUploadUrlAsync(folder, fileName)` |
| `ICurrentUserService` | Extracts `UserId` and `Role` from JWT claims |
| `IDateTimeProvider` | Testable `UtcNow` abstraction |
| `IPasswordHasher` | BCrypt hash/verify abstraction |
| `ITokenService` | JWT generation, refresh token store/revoke/validate |

## AI Tables (owned by EF Core, shared with Python ai-service)

These entities follow the same EF Core configuration conventions but are read/written by both the C# backend and the Python ai-service (via SQLAlchemy).

- `NutritionDocument` — RAG knowledge base; `Embedding float[]?` → `vector(768)`
- `UserPreferenceEmbedding` — per-user preference vector; `UserId` is PK
- `UserBehaviorEvent` — click/view/order events for collaborative filtering
- `ChatFeedback` — thumbs-up/down on AI chat responses

Vector columns use `HasColumnType("vector(768)")` in EF Core configuration.
