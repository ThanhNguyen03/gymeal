using System.Text.Json;
using Gymeal.Application;
using Gymeal.Infrastructure;
using Gymeal.Presentation.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Sentry ────────────────────────────────────────────────────────────────────
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["BACKEND_SENTRY_DSN"];
    options.TracesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
    options.Environment = builder.Environment.EnvironmentName;
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd", policy =>
    {
        string allowedOrigin = builder.Configuration["CORS_ALLOWED_ORIGIN"]
            ?? throw new InvalidOperationException("CORS_ALLOWED_ORIGIN is not configured.");

        policy
            .WithOrigins(allowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── Application + Infrastructure layers ──────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Gymeal API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL is not configured."),
        name: "postgres",
        tags: ["ready"])
    .AddRedis(
        redisConnectionString: builder.Configuration["REDIS_URL"]
            ?? throw new InvalidOperationException("REDIS_URL is not configured."),
        name: "redis",
        tags: ["ready"])
    .AddUrlGroup(
        uri: new Uri($"{builder.Configuration["AI_SERVICE_INTERNAL_URL"]}/health"),
        name: "ai-service",
        tags: ["ready"]);

// ── Rate limiting ─────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// ── Hot Chocolate GraphQL stub ────────────────────────────────────────────────
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query").Field("_placeholder").Type<StringType>().Resolve("ok"))
    .AddMutationType(d => d.Name("Mutation").Field("_placeholder").Type<StringType>().Resolve("ok"));

WebApplication app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
// NOTE: order matters — CorrelationId must be first to propagate to all logs/errors
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gymeal API v1"));
}

app.UseSentryTracing();
app.UseCors("FrontEnd");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

// ── Health endpoints ──────────────────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    // Liveness: always returns 200 if app is running
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: check all tagged "ready" dependencies
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

// ── GraphQL ───────────────────────────────────────────────────────────────────
app.MapGraphQL("/graphql");

// ── REST controllers ──────────────────────────────────────────────────────────
app.MapControllers();

app.Run();

// ── Helpers ───────────────────────────────────────────────────────────────────
static Task WriteHealthResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    context.Response.ContentType = "application/json";
    JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    string json = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        duration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new { status = e.Value.Status.ToString(), e.Value.Description, duration = e.Value.Duration.TotalMilliseconds }
        )
    }, options);
    return context.Response.WriteAsync(json);
}

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
