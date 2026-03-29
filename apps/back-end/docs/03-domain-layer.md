# Domain Layer

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

## Enums

| Enum | Values |
|------|--------|
| UserRole | Customer, Provider, Admin |
| FitnessGoalType | Cut, Bulk, Maintain, Endurance, Recomp |
| ActivityLevel | Sedentary, LightlyActive, ModeratelyActive, VeryActive, ExtremelyActive |
| AuditAction | Create, Update, Delete |
| MealTime | Breakfast, Lunch, Dinner, Snack, PreWorkout, PostWorkout |
| OrderStatus | Pending, Confirmed, Preparing, Ready, Delivered, Cancelled |
| NutritionDocumentCategory | Macronutrient, Micronutrient, SportNutrition, DietaryGuideline, GoalSpecific |
| BehaviorEventType | MealViewed, MealOrdered, MealSearched, MealDismissed, ChatAsked, RecommendationClicked |
| FeedbackSentiment | ThumbsUp, ThumbsDown |

## AI Tables (owned by EF Core, shared with Python ai-service)

These entities follow the same EF Core configuration conventions but are read/written by both the C# backend and the Python ai-service (via SQLAlchemy).

- `NutritionDocument` — RAG knowledge base; `Embedding float[]?` → `vector(768)`
- `UserPreferenceEmbedding` — per-user preference vector; `UserId` is PK
- `UserBehaviorEvent` — click/view/order events for collaborative filtering
- `ChatFeedback` — thumbs-up/down on AI chat responses

Vector columns use `HasColumnType("vector(768)")` in EF Core configuration.
