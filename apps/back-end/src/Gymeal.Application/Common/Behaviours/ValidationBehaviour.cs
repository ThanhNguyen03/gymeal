using FluentValidation;
using FluentValidation.Results;
using Gymeal.Domain.Common;
using MediatR;

namespace Gymeal.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs all registered FluentValidation validators
/// for a request before the handler executes. Returns a validation error Result
/// without reaching the handler if validation fails.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            string errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            Error error = Error.Validation(errorMessage);

            // Use reflection to construct Result<T>.Failure(error) generically.
            // TResponse is expected to be Result<TValue> — this is enforced by convention.
            Type responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                object? failureResult = responseType
                    .GetMethod(nameof(Result<object>.Failure))!
                    .Invoke(null, [error]);

                return (TResponse)failureResult!;
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
