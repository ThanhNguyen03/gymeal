using System.Security.Cryptography;
using System.Text.Json;
using AspNetCoreRateLimit;
using Gymeal.Application;
using Gymeal.Domain.Interfaces.Services;
using Gymeal.Infrastructure;
using Gymeal.Presentation.Middlewares;
using Gymeal.Presentation.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Json;

// ── Serilog (configure before anything else to capture startup errors) ────────
// DECISION: Serilog chosen over default Microsoft.Extensions.Logging for:
// 1. Structured JSON output (compatible with Logtail, Seq, Grafana Loki)
// 2. LogContext.PushProperty — correlation ID propagation in CorrelationIdMiddleware
// 3. Enrichers for machine name, thread ID, request path
// Reference: PLAN.md §13 (Logging & Observability)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter()));

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

// ── HttpContextAccessor (needed by ServiceCurrentUser) ────────────────────────
builder.Services.AddHttpContextAccessor();

// ── ServiceCurrentUser (Scoped — one per request) ─────────────────────────────
builder.Services.AddScoped<ICurrentUserService, ServiceCurrentUser>();

// ── JWT RS256 Authentication ──────────────────────────────────────────────────
// SECURITY: RS256 asymmetric — private key on back-end, public key shared with edge.
// Token is read from HttpOnly cookie, not Authorization header.
// Reference: PLAN.md §7 (Application Security)
string publicKeyBase64 = builder.Configuration["JWT_PUBLIC_KEY_BASE64"]
    ?? throw new InvalidOperationException("JWT_PUBLIC_KEY_BASE64 is not configured.");

byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
RSA rsaPublic = RSA.Create();
rsaPublic.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
RsaSecurityKey rsaSecurityKey = new(rsaPublic);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            IssuerSigningKey = rsaSecurityKey,
            // NOTE: ClockSkew = Zero prevents the default 5-minute grace period.
            // With 15-minute access tokens, a 5-minute grace is 33% of lifetime — too long.
            ClockSkew = TimeSpan.Zero,
        };

        // Read JWT from HttpOnly cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

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
        BearerFormat = "JWT",
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
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
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration,
    AspNetCoreRateLimit.RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// ── Hot Chocolate GraphQL stub ────────────────────────────────────────────────
// NOTE: Hot Chocolate chosen over Apollo Server (JS) because:
// 1. Native .NET — no Node.js runtime, no separate service
// 2. Shares ASP.NET Core DI, auth, and middleware pipeline
// 3. Built-in WebSocket subscriptions (graphql-transport-ws)
// 4. DataLoader built-in for N+1 prevention
// Trade-off: Smaller ecosystem than Apollo; schema federation not needed for this project scope.
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query").Field("_placeholder").Type<HotChocolate.Types.StringType>().Resolve(_ => new ValueTask<object?>("ok")))
    .AddMutationType(d => d.Name("Mutation").Field("_placeholder").Type<HotChocolate.Types.StringType>().Resolve(_ => new ValueTask<object?>("ok")));

WebApplication app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────────
// NOTE: Order matters — CorrelationId must be first to propagate to all logs/errors.
app.UseMiddleware<MiddlewareCorrelationId>();
app.UseMiddleware<MiddlewareException>();

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
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse,
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse,
});

// ── GraphQL ───────────────────────────────────────────────────────────────────
app.MapGraphQL("/graphql");

// ── REST controllers ──────────────────────────────────────────────────────────
app.MapControllers();

app.Run();

// ── Helpers ───────────────────────────────────────────────────────────────────
static Task WriteHealthResponse(
    HttpContext context,
    Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    context.Response.ContentType = "application/json";
    JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    string json = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        duration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status = e.Value.Status.ToString(),
                e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
            }),
    }, options);
    return context.Response.WriteAsync(json);
}

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
