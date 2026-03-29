namespace Gymeal.Application.Common.Errors;

/// <summary>
/// Typed error value returned from handlers instead of throwing exceptions.
/// Use static factories to create well-known error types.
/// </summary>
/// <remarks>
/// NOTE: Exceptions are only for truly exceptional situations (DB down, null reference bugs).
/// Business rule violations (not found, validation, unauthorized) use Result&lt;T&gt; + Error.
/// Reason: Exceptions are invisible in the type signature, slow for control flow,
/// and force callers to use try/catch instead of explicit error handling.
/// Reference: PLAN.md §4.4
/// </remarks>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} with id '{id}' was not found.");

    public static Error NotFound(string message) =>
        new("Resource.NotFound", message);

    public static Error Validation(string message) =>
        new("Validation.Failed", message);

    public static Error Unauthorized(string message = "Access denied.") =>
        new("Auth.Unauthorized", message);

    public static Error Conflict(string message) =>
        new("Resource.Conflict", message);

    public static Error Forbidden(string message = "You do not have permission to perform this action.") =>
        new("Auth.Forbidden", message);
}
