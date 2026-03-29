using Gymeal.Domain.Entities;
using Gymeal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public sealed class ConfigurationMeal : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("meals");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description").IsRequired();
        builder.Property(m => m.ImageUrl).HasColumnName("image_url");

        builder.Property(m => m.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasDefaultValue(EMealCategory.Lunch);

        builder.Property(m => m.PriceInCents).HasColumnName("price_in_cents").IsRequired();
        builder.Property(m => m.Calories).HasColumnName("calories").IsRequired();
        builder.Property(m => m.ProteinG).HasColumnName("protein_g");
        builder.Property(m => m.CarbsG).HasColumnName("carbs_g");
        builder.Property(m => m.FatG).HasColumnName("fat_g");
        builder.Property(m => m.FiberG).HasColumnName("fiber_g");
        builder.Property(m => m.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);
        builder.Property(m => m.DeletedAt).HasColumnName("deleted_at");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        // Array columns stored as text[]
        builder.Property(m => m.Ingredients).HasColumnName("ingredients").HasColumnType("text[]");
        builder.Property(m => m.AllergenTags).HasColumnName("allergen_tags").HasColumnType("text[]");
        builder.Property(m => m.FitnessGoalTags).HasColumnName("fitness_goal_tags").HasColumnType("text[]");

        // pgvector column — requires CREATE EXTENSION IF NOT EXISTS vector in migration
        // NOTE: Embedding is NEVER exposed via GraphQL or REST (MealDto intentionally excludes it)
        builder.Property(m => m.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(768)");

        builder.HasIndex(m => m.ProviderId)
            .HasDatabaseName("ix_meals_provider_id");

        // GIN trigram indexes for pg_trgm similarity search
        // Requires: CREATE EXTENSION IF NOT EXISTS pg_trgm
        builder.HasIndex(m => m.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("gin_meals_name_trgm");

        builder.HasIndex(m => m.Description)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("gin_meals_description_trgm");

        // IVFFlat index for pgvector ANN (approximate nearest neighbour) queries
        builder.HasIndex(m => m.Embedding)
            .HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops")
            .HasAnnotation("Npgsql:IndexWithParameters", "lists = 100")
            .HasDatabaseName("ivfflat_meals_embedding");

        // Partial index — only index available meals for catalog queries
        builder.HasIndex(m => m.IsAvailable)
            .HasFilter("is_available = true")
            .HasDatabaseName("ix_meals_available");

        // Soft delete global query filter
        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
