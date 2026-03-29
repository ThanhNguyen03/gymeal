using Gymeal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public sealed class ConfigurationUserPreferenceEmbedding : IEntityTypeConfiguration<UserPreferenceEmbedding>
{
    public void Configure(EntityTypeBuilder<UserPreferenceEmbedding> builder)
    {
        builder.ToTable("user_preference_embeddings");

        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserId).HasColumnName("user_id");

        // NOTE: vector(768) — aggregated preference vector from user behavior events.
        // Recomputed by QStash daily cron (Sprint 5).
        builder.Property(e => e.PreferenceVector)
            .HasColumnName("preference_vector")
            .HasColumnType("vector(768)");

        builder.Property(e => e.InteractionCount).HasColumnName("interaction_count").HasDefaultValue(0);
        builder.Property(e => e.ComputedAt).HasColumnName("computed_at");
    }
}
