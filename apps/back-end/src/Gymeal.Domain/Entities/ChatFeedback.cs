using Gymeal.Domain.Enums;

namespace Gymeal.Domain.Entities;

/// <summary>
/// Thumbs-up/down feedback on AI chat responses.
/// Feeds the preference learner and measures chat quality (Sprint 5).
/// </summary>
public sealed class ChatFeedback : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public EFeedbackSentiment Sentiment { get; set; }
    public string? Comment { get; set; }
}
