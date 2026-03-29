namespace Gymeal.Domain.Common;

// DECISION: Error and Result<T> live in Domain (not Application) so that
// IRepository interfaces can return Result<T> without violating Clean Architecture.
// Domain has zero external dependencies — these are pure value types.
// Reference: RULE.md §5.6 (Repository Result Pattern)
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
