namespace Gymeal.Domain.Interfaces.Services;

/// <summary>
/// Abstracts DateTime.UtcNow to enable deterministic unit testing.
/// Always inject this instead of calling DateTime.UtcNow directly.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
