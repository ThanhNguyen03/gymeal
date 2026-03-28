import sentry_sdk
import structlog
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.core.config import settings
from app.routers import admin, chat, nutrition, recommendations

# ── Sentry ────────────────────────────────────────────────────────────────────
if settings.AI_SERVICE_SENTRY_DSN:
    sentry_sdk.init(
        dsn=settings.AI_SERVICE_SENTRY_DSN,
        traces_sample_rate=0.1,
        profiles_sample_rate=0.1,
    )

# ── Structured logging ────────────────────────────────────────────────────────
structlog.configure(
    processors=[
        structlog.processors.TimeStamper(fmt="iso"),
        structlog.stdlib.add_log_level,
        structlog.processors.StackInfoRenderer(),
        structlog.processors.JSONRenderer(),
    ],
    wrapper_class=structlog.make_filtering_bound_logger(20),  # INFO
    logger_factory=structlog.PrintLoggerFactory(),
)

log = structlog.get_logger()

# ── FastAPI app ────────────────────────────────────────────────────────────────
app = FastAPI(
    title="Gymeal AI Service",
    version="0.1.0",
    description="Internal AI nutrition coaching service. Not internet-facing. Called only by C# back-end.",
    docs_url="/docs",
    redoc_url=None,
)

# ── CORS — only allow C# back-end internal URL ────────────────────────────────
# NOTE: ai-service is not public. CORS here is defence-in-depth only.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Restricted at network level (Koyeb internal routing)
    allow_credentials=False,
    allow_methods=["POST", "GET"],
    allow_headers=["X-Internal-Auth", "X-User-Id", "X-Correlation-Id", "Content-Type"],
)


# ── Health endpoints ──────────────────────────────────────────────────────────
@app.get("/health", tags=["health"], include_in_schema=False)
async def health_liveness() -> dict[str, str]:
    """Liveness probe — returns 200 if the process is running."""
    return {"status": "ok"}


@app.get("/health/ready", tags=["health"], include_in_schema=False)
async def health_readiness() -> dict[str, str]:
    """
    Readiness probe — checks DB connectivity and Groq reachability.
    Returns 200 only when all dependencies are healthy.
    """
    # TODO Sprint 1: add real DB + Groq connectivity checks
    return {"status": "ok", "note": "Deep checks not yet implemented (Sprint 1)"}


# ── Routers ───────────────────────────────────────────────────────────────────
app.include_router(chat.router)
app.include_router(recommendations.router)
app.include_router(nutrition.router)
app.include_router(admin.router)


@app.on_event("startup")
async def on_startup() -> None:
    log.info("gymeal_ai_service_started", port=settings.AI_SERVICE_PORT)


@app.on_event("shutdown")
async def on_shutdown() -> None:
    log.info("gymeal_ai_service_stopped")
