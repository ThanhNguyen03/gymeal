using System.Text.Json;
using Gymeal.Application.Common.Interfaces;
using Gymeal.Domain.Entities;
using Gymeal.Domain.Enums;
using Gymeal.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Gymeal.Infrastructure.Persistence;

/// <summary>
/// The single source of truth for all database schema and migrations.
/// </summary>
/// <remarks>
/// DECISION: EF Core owns ALL migrations — both backend and AI tables.
/// Python ai-service uses SQLAlchemy ORM for read/write only — no Alembic.
/// Two migration tools on one DB = conflict risk. One tool, one schema owner.
/// Reference: PLAN.md §10 (Migration Ownership Rule)
///
/// DECISION: Soft delete global query filter applied here via ISoftDeletable convention.
/// All entities implementing ISoftDeletable are automatically filtered to exclude deleted rows.
/// Reference: PLAN.md §4.5
/// </remarks>
public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime) : DbContext(options), IAppDbContext
{
    // ── Backend tables ────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ── Sprint 2 tables ───────────────────────────────────────────────────────
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealRequest> MealRequests => Set<MealRequest>();

    // ── AI tables (schema owned here, read/write by Python ai-service) ────────
    public DbSet<NutritionDocument> NutritionDocuments => Set<NutritionDocument>();
    public DbSet<UserPreferenceEmbedding> UserPreferenceEmbeddings => Set<UserPreferenceEmbedding>();
    public DbSet<UserBehaviorEvent> UserBehaviorEvents => Set<UserBehaviorEvent>();
    public DbSet<ChatFeedback> ChatFeedback => Set<ChatFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // NOTE: Soft delete global query filter applied here for ALL ISoftDeletable entities.
        // Any entity that implements ISoftDeletable automatically gets the filter.
        // To access soft-deleted records: use .IgnoreQueryFilters() on the query.
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = dateTime.UtcNow;

        // Auto-set CreatedAt / UpdatedAt on BaseEntity subclasses
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        // Auto-set UpdatedAt on UserProfile (no BaseEntity inheritance)
        foreach (EntityEntry<UserProfile> entry in ChangeTracker.Entries<UserProfile>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        // Capture audit entries BEFORE saving so we have before/after state
        List<AuditEntry> auditEntries = OnBeforeSaveChanges();

        int result = await base.SaveChangesAsync(cancellationToken);

        // Write audit logs after save so entity IDs are guaranteed to be set
        if (auditEntries.Count > 0)
        {
            await WriteAuditLogsAsync(auditEntries, cancellationToken);
        }

        return result;
    }

    // ── Audit helpers ─────────────────────────────────────────────────────────

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        List<AuditEntry> entries = [];

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            // Skip audit logs (would cause infinite recursion) and unchanged/detached entities
            if (entry.Entity is AuditLog
                || entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            AuditEntry auditEntry = new()
            {
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State switch
                {
                    EntityState.Added => EAuditAction.Create,
                    EntityState.Modified => EAuditAction.Update,
                    EntityState.Deleted => EAuditAction.Delete,
                    _ => EAuditAction.Update,
                },
            };

            foreach (PropertyEntry prop in entry.Properties)
            {
                if (prop.Metadata.IsPrimaryKey()) continue;

                // WARNING: Skip password hashes from audit logs — NEVER log credentials.
                // Reason: Audit logs may be queried by support staff or stored in log aggregators.
                if (prop.Metadata.Name == "PasswordHash") continue;

                string propName = prop.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.After[propName] = prop.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.Before[propName] = prop.OriginalValue;
                        break;
                    case EntityState.Modified when prop.IsModified:
                        auditEntry.Before[propName] = prop.OriginalValue;
                        auditEntry.After[propName] = prop.CurrentValue;
                        break;
                }
            }

            entries.Add(auditEntry);
        }

        return entries;
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        object? idValue = entry.Property("Id")?.CurrentValue;
        return idValue is Guid guid ? guid : Guid.Empty;
    }

    private async Task WriteAuditLogsAsync(List<AuditEntry> entries, CancellationToken ct)
    {
        foreach (AuditEntry entry in entries)
        {
            AuditLog log = new()
            {
                UserId = currentUser.UserId,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Action = entry.Action,
                Changes = JsonSerializer.Serialize(new
                {
                    before = entry.Before.Count > 0 ? entry.Before : null,
                    after = entry.After.Count > 0 ? entry.After : null,
                }),
                IpAddress = currentUser.IpAddress,
                CreatedAt = dateTime.UtcNow,
                UpdatedAt = dateTime.UtcNow,
            };

            AuditLogs.Add(log);
        }

        await base.SaveChangesAsync(ct);
    }

    private sealed class AuditEntry
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public EAuditAction Action { get; set; }
        public Dictionary<string, object?> Before { get; } = [];
        public Dictionary<string, object?> After { get; } = [];
    }
}

/// <summary>Extension for applying the soft delete query filter via convention.</summary>
internal static class SoftDeleteExtensions
{
    public static void AddSoftDeleteQueryFilter(
        this Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        System.Reflection.MethodInfo? method = typeof(SoftDeleteExtensions)
            .GetMethod(nameof(GetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(entityType.ClrType);

        System.Linq.Expressions.LambdaExpression filter =
            (System.Linq.Expressions.LambdaExpression)method.Invoke(null, null)!;

        entityType.SetQueryFilter(filter);
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> GetSoftDeleteFilter<T>()
        where T : class, ISoftDeletable
        => entity => entity.DeletedAt == null;
}
