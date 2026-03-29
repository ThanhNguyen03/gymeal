namespace Gymeal.Application.Common.Errors;

/// <summary>
/// Discriminated union: either a success value or a typed error.
/// Handlers return Result&lt;T&gt;; controllers map errors to HTTP status codes.
/// </summary>
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
