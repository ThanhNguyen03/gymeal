using Gymeal.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

/// <summary>
/// Abstract base controller that provides shared error mapping logic.
/// All API controllers inherit from this to keep MapError DRY.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ObjectResult MapError(Error error)
    {
        (int status, string title) = error.Code switch
        {
            var c when c.EndsWith("NotFound", StringComparison.Ordinal) => (404, "Not Found"),
            var c when c.EndsWith("Unauthorized", StringComparison.Ordinal) => (401, "Unauthorized"),
            var c when c.EndsWith("Forbidden", StringComparison.Ordinal) => (403, "Forbidden"),
            var c when c.EndsWith("Conflict", StringComparison.Ordinal) => (409, "Conflict"),
            var c when c.EndsWith("Failed", StringComparison.Ordinal) => (422, "Unprocessable Entity"),
            _ => (500, "Internal Server Error"),
        };

        return StatusCode(status, new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Message,
            Extensions = { ["correlationId"] = HttpContext.Items["CorrelationId"] },
        });
    }
}
