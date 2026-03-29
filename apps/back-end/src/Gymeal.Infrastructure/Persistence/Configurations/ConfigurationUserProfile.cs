using Gymeal.Domain.Entities;
using Gymeal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public sealed class ConfigurationUserProfile : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(p => p.UserId);
        builder.Property(p => p.UserId).HasColumnName("user_id");

        builder.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(100);
        builder.Property(p => p.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(p => p.Age).HasColumnName("age");
        builder.Property(p => p.WeightKg).HasColumnName("weight_kg").HasPrecision(5, 2);
        builder.Property(p => p.HeightCm).HasColumnName("height_cm").HasPrecision(5, 2);
        builder.Property(p => p.BodyFatPct).HasColumnName("body_fat_pct").HasPrecision(4, 1);

        builder.Property(p => p.FitnessGoal)
            .HasColumnName("fitness_goal")
            .HasConversion<string>()
            .HasDefaultValue(EFitnessGoalType.Maintain);

        builder.Property(p => p.ActivityLevel)
            .HasColumnName("activity_level")
            .HasConversion<string>()
            .HasDefaultValue(EActivityLevel.ModeratelyActive);

        // PostgreSQL text[] arrays
        builder.Property(p => p.DietaryRestrictions)
            .HasColumnName("dietary_restrictions")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder.Property(p => p.Allergies)
            .HasColumnName("allergies")
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder.Property(p => p.DailyCalorieTarget)
            .HasColumnName("daily_calorie_target")
            .HasDefaultValue(2000);

        builder.Property(p => p.ProteinTargetG)
            .HasColumnName("protein_target_g")
            .HasDefaultValue(150);

        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
    }
}
