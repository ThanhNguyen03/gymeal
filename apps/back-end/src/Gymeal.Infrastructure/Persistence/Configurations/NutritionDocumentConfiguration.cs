using Gymeal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public class NutritionDocumentConfiguration : IEntityTypeConfiguration<NutritionDocument>
{
    public void Configure(EntityTypeBuilder<NutritionDocument> builder)
    {
        builder.ToTable("nutrition_documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(d => d.Content).HasColumnName("content").IsRequired();
        builder.Property(d => d.Source).HasColumnName("source").HasMaxLength(500);

        builder.Property(d => d.Category)
            .HasColumnName("category")
            .HasConversion<string>();

        // NOTE: vector(768) column — populated by Python seed script via pgvector.
        // EF Core owns the schema (creates the column), Python reads/writes the data.
        builder.Property(d => d.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(768)");

        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
    }
}
