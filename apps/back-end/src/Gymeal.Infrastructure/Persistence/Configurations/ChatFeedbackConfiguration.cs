using Gymeal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gymeal.Infrastructure.Persistence.Configurations;

public class ChatFeedbackConfiguration : IEntityTypeConfiguration<ChatFeedback>
{
    public void Configure(EntityTypeBuilder<ChatFeedback> builder)
    {
        builder.ToTable("chat_feedback");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.MessageId).HasColumnName("message_id");
        builder.Property(f => f.UserId).HasColumnName("user_id");

        builder.Property(f => f.Sentiment)
            .HasColumnName("sentiment")
            .HasConversion<string>();

        builder.Property(f => f.Comment).HasColumnName("comment");
        builder.Property(f => f.CreatedAt).HasColumnName("created_at");
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(f => f.MessageId)
            .HasDatabaseName("ix_chat_feedback_message_id");
    }
}
