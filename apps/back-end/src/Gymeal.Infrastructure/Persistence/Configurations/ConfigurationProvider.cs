using Gymeal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public sealed class ConfigurationProvider : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").IsRequired();
        builder.Property(p => p.LogoUrl).HasColumnName("logo_url");
        builder.Property(p => p.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
        builder.Property(p => p.Rating).HasColumnName("rating").HasDefaultValue(0m);
        builder.Property(p => p.TotalOrders).HasColumnName("total_orders").HasDefaultValue(0);
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // Stored as text[] — GIN index for efficient array contains queries
        builder.Property(p => p.CuisineTags)
            .HasColumnName("cuisine_tags")
            .HasColumnType("text[]");

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("ix_providers_user_id");

        builder.HasIndex(p => p.CuisineTags)
            .HasMethod("gin")
            .HasDatabaseName("gin_providers_cuisine_tags");

        // Soft delete global query filter
        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Meals)
            .WithOne(m => m.Provider)
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
