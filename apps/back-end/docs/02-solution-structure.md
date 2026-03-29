# Solution Structure

## Overview

The back-end follows **Clean Architecture** with four projects, each depending only on the layers inside it:

```
Gymeal.Domain          ← no external dependencies
    ↑
Gymeal.Application     ← depends on Domain only (+ MediatR, FluentValidation)
    ↑
Gymeal.Infrastructure  ← depends on Application + Domain (EF Core, Redis, BCrypt)
    ↑
Gymeal.Presentation    ← depends on Infrastructure (ASP.NET Core, Serilog, JWT)
```

## Projects

### `Gymeal.Domain`
Pure business logic. Zero NuGet dependencies.

- `Entities/` — `BaseEntity`, `User`, `UserProfile`, `AuditLog`, `NutritionDocument`, `UserPreferenceEmbedding`, `UserBehaviorEvent`, `ChatFeedback`
- `Enums/` — `UserRole`, `FitnessGoalType`, `ActivityLevel`, `AuditAction`, `MealTime`, `OrderStatus`, `NutritionDocumentCategory`, `BehaviorEventType`, `FeedbackSentiment`
- `Interfaces/Repositories/` — `IUserRepository`
- `Interfaces/Services/` — `IPasswordHasher`, `ITokenService`, `ICurrentUserService`, `IDateTimeProvider`

### `Gymeal.Application`
CQRS handlers, validators, pipeline behaviours. Depends on Domain only.

- `Common/Errors/` — `Error`, `Result<T>`
- `Common/Interfaces/` — `IAppDbContext` (thin interface for EF Core ChangeTracker access)
- `Common/Behaviours/` — `LoggingBehaviour`, `ValidationBehaviour`, `AuditBehaviour` *(all sealed)*
- `Features/Auth/Commands/` — `RegisterUser`, `LoginUser`, `RefreshToken`, `LogoutUser`
- `Features/Users/` — `GetCurrentUser`, `UpdateUserProfile`

**Pipeline order**: Request → LoggingBehaviour → ValidationBehaviour → AuditBehaviour → Handler

### `Gymeal.Infrastructure`
EF Core, Redis, BCrypt implementations. Never referenced directly by Presentation.

- `Persistence/AppDbContext.cs` — soft-delete query filters, CreatedAt/UpdatedAt auto-set, audit log writing in SaveChangesAsync
- `Persistence/Configurations/` — one `IEntityTypeConfiguration<T>` per entity, named `Configuration{Entity}` (e.g. `ConfigurationUser`, `ConfigurationAuditLog`)
- `Persistence/Repositories/` — `RepositoryUser`
- `Services/` — `ServiceRedisToken`, `ServiceBcryptPasswordHasher`, `ServiceDateTimeProvider`, `ServiceAiHttpClient`

> **Naming convention:** Services, repositories, middlewares, and EF configurations use the Microsoft-Style
> Prefix convention (`Service{Name}`, `Repository{Name}`, `Middleware{Name}`, `Configuration{Entity}`).
> Controllers keep the `*Controller` suffix (ASP.NET auto-discovery) and interfaces keep `I` prefix
> (Microsoft C# guideline). All concrete classes are `sealed` by default.

### `Gymeal.Presentation`
ASP.NET Core host. Wires everything together, never contains business logic.

- `Controllers/` — `AuthController` (`/api/v1/auth`), `UsersController` (`/api/v1/users`) *(keep `*Controller` suffix — ASP.NET convention)*
- `Services/ServiceCurrentUser.cs` — reads claims from `HttpContext`
- `Middlewares/` — `MiddlewareException`, `MiddlewareCorrelationId`
- `Program.cs` — bootstraps Serilog, JWT RS256, rate limiting, middleware pipeline

## Key architectural decisions

| Decision | Rationale |
|----------|-----------|
| `IAppDbContext` in Application | Lets `AuditBehaviour` mark commands without Application importing EF Core directly; documented TRADE-OFF |
| Audit written in `SaveChangesAsync` | ChangeTracker diffs are most accurate just before/after save; avoids double-save problems |
| Cookie-only JWT | Prevents XSS token theft; access token 15 min, refresh token 7 days (Path=/api/v1/auth/refresh) |
| Result\<T\> instead of exceptions | Business errors (404, 409, 401) are typed, not thrown; controllers map `Error.Code` to HTTP status |
| Soft delete via global query filter | `HasQueryFilter(e => e.DeletedAt == null)` on every `ISoftDeletable` entity — deleted records are invisible by default |
