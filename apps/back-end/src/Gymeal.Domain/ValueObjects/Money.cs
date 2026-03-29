namespace Gymeal.Domain.ValueObjects;

/// <summary>
/// Monetary value stored as integer cents to avoid floating-point precision issues.
/// </summary>
public record Money(int AmountCents, string Currency = "USD")
{
    /// <summary>Returns the decimal representation (e.g., 999 cents → 9.99).</summary>
    public decimal ToDecimal() => AmountCents / 100m;
}
