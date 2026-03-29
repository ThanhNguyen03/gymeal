using Gymeal.Domain.Entities;
using Gymeal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public sealed class ConfigurationMealRequest : IEntityTypeConfiguration<MealRequest>
{
    public void Configure(EntityTypeBuilder<MealRequest> builder)
    {
        builder.ToTable("meal_requests");

        builder.HasKey(mr => mr.Id);
        builder.Property(mr => mr.Id).HasColumnName("id");

        builder.Property(mr => mr.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(mr => mr.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(mr => mr.Description).HasColumnName("description").IsRequired();
        builder.Property(mr => mr.ResponseMessage).HasColumnName("response_message");
        builder.Property(mr => mr.QuotePriceInCents).HasColumnName("quote_price_in_cents");
        builder.Property(mr => mr.CreatedAt).HasColumnName("created_at");
        builder.Property(mr => mr.UpdatedAt).HasColumnName("updated_at");

        builder.Property(mr => mr.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasDefaultValue(EMealRequestStatus.Pending);

        builder.HasIndex(mr => mr.UserId).HasDatabaseName("ix_meal_requests_user_id");
        builder.HasIndex(mr => mr.ProviderId).HasDatabaseName("ix_meal_requests_provider_id");

        // No soft delete — meal requests are immutable audit records; status tracks lifecycle
        builder.HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Provider)
            .WithMany()
            .HasForeignKey(mr => mr.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
