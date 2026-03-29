namespace Gymeal.Domain.Common;

// NOTE: Discriminated union: either a success value or a typed error.
// Handlers return Result<T>; controllers map errors to HTTP status codes.
// Reason: Exceptions are invisible in the type signature, 10-100x slower for control flow,
// and force callers to guess what can go wrong. Result makes failure paths explicit.
// Reference: RULE.md §5.6, PLAN.md §4.4
public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly Error? _error;

    private Result(TValue value)
    {
        _value = value;
        IsSuccess = true;
        _error = Error.None;
    }

    private Result(Error error)
    {
        _error = error;
        IsSuccess = false;
        _value = default;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public Error Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Cannot access Error of a successful Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    // Implicit conversions for ergonomic usage in handlers
    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
