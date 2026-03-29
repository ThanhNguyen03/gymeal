using Gymeal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public class UserBehaviorEventConfiguration : IEntityTypeConfiguration<UserBehaviorEvent>
{
    public void Configure(EntityTypeBuilder<UserBehaviorEvent> builder)
    {
        builder.ToTable("user_behavior_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.MealId).HasColumnName("meal_id");

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>();

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => new { e.UserId, e.EventType })
            .HasDatabaseName("ix_user_behavior_events_user_event");
    }
}
