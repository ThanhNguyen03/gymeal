using Microsoft.Extensions.Configuration;

namespace Gymeal.Infrastructure.Services;

/// <summary>
/// Typed HttpClient for communicating with the Python ai-service.
/// Automatically injects X-Internal-Auth header on every request.
/// The ai-service is INTERNAL ONLY — never call it with a public URL.
///
/// DECISION: internal auth header over JWT because ai-service is not public.
/// Trade-off: must ensure network isolation (Koyeb internal routing in prod).
/// </summary>
public sealed class AiServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public AiServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        string baseUrl = configuration["AI_SERVICE_INTERNAL_URL"]
            ?? throw new InvalidOperationException("AI_SERVICE_INTERNAL_URL is not configured.");
        string authSecret = configuration["INTERNAL_AUTH_SECRET"]
            ?? throw new InvalidOperationException("INTERNAL_AUTH_SECRET is not configured.");

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Internal-Auth", authSecret);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public HttpClient Client => _httpClient;
}
