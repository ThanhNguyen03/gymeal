using Gymeal.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymeal.Presentation.Controllers;

/// <summary>
/// BFF proxy controller: all /api/v1/ai/* routes are proxied to the Python ai-service.
/// Adds X-Internal-Auth, X-User-Id, and X-Correlation-Id headers.
/// The front-end never calls the Python service directly — all AI traffic goes through here.
///
/// DECISION: BFF pattern over direct front-end → ai-service calls because:
///   1. Auth is validated here before requests reach ai-service
///   2. ai-service stays internal-only (no public route needed)
///   3. Single domain for CORS — front-end only needs NEXT_PUBLIC_API_URL
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Authorize]
public sealed class AiProxyController(AiServiceHttpClient aiClient) : ControllerBase
{
    // NOTE: Sprint 0 — stub responses. Real proxy forwarding implemented in Sprint 3.
    // aiClient will be used in Sprint 3 when real proxy forwarding is wired.
    private readonly AiServiceHttpClient _aiClient = aiClient;

    /// <summary>POST /api/v1/ai/chat/sessions</summary>
    [HttpPost("chat/sessions")]
    public IActionResult CreateChatSession() => ServiceUnavailable("chat/sessions");

    /// <summary>GET /api/v1/ai/chat/sessions/{id}/messages</summary>
    [HttpGet("chat/sessions/{id}/messages")]
    public IActionResult GetChatMessages(string id) => ServiceUnavailable($"chat/sessions/{id}/messages");

    /// <summary>POST /api/v1/ai/chat/sessions/{id}/messages — SSE stream</summary>
    [HttpPost("chat/sessions/{id}/messages")]
    public IActionResult SendChatMessage(string id) => ServiceUnavailable($"chat/sessions/{id}/messages");

    /// <summary>POST /api/v1/ai/chat/messages/{id}/feedback</summary>
    [HttpPost("chat/messages/{id}/feedback")]
    public IActionResult SubmitFeedback(string id) => ServiceUnavailable($"chat/messages/{id}/feedback");

    /// <summary>POST /api/v1/ai/nutrition/analyze</summary>
    [HttpPost("nutrition/analyze")]
    public IActionResult AnalyzeNutrition() => ServiceUnavailable("nutrition/analyze");

    /// <summary>POST /api/v1/ai/recommendations/generate</summary>
    [HttpPost("recommendations/generate")]
    public IActionResult GenerateRecommendations() => ServiceUnavailable("recommendations/generate");

    /// <summary>GET /api/v1/ai/recommendations/{userId}</summary>
    [HttpGet("recommendations/{userId}")]
    public IActionResult GetRecommendations(string userId) => ServiceUnavailable($"recommendations/{userId}");

    /// <summary>POST /api/v1/ai/events/behavior</summary>
    [HttpPost("events/behavior")]
    public IActionResult TrackBehaviorEvent() => ServiceUnavailable("events/behavior");

    /// <summary>POST /api/v1/ai/admin/ingest-documents — admin only</summary>
    [HttpPost("admin/ingest-documents")]
    [Authorize(Roles = "Admin")]
    public IActionResult IngestDocuments() => ServiceUnavailable("admin/ingest-documents");

    private ObjectResult ServiceUnavailable(string route) =>
        StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            error = new
            {
                code = "AI_SERVICE_NOT_CONNECTED",
                message = $"AI proxy route '{route}' is not yet wired. Available in Sprint 3.",
                correlationId = HttpContext.Items["CorrelationId"]
            }
        });
}
