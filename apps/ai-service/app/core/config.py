from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """
    Application settings loaded from environment variables.
    All values must be set in .env (see .example.env at repo root for instructions).
    """

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore",
    )

    # Database
    DATABASE_URL: str

    # Internal auth (shared secret — NOT a JWT)
    # DECISION: internal auth header over JWT because ai-service is not public.
    # Trade-off: must ensure network isolation (Koyeb internal routing in prod).
    INTERNAL_AUTH_SECRET: str

    # Groq LLM
    GROQ_API_KEY: str

    # LLM token budget — adjust per model (llama3-70b has 8192 total)
    MAX_CONTEXT_TOKENS: int = 3584
    MAX_RESPONSE_TOKENS: int = 512

    # Cold-start recommendation threshold (number of behavior events before full AI)
    COLD_START_THRESHOLD: int = 20

    # Sentry
    AI_SERVICE_SENTRY_DSN: str = ""

    # Service
    AI_SERVICE_PORT: int = 8001
    AI_SERVICE_HOST: str = "0.0.0.0"


settings = Settings()  # type: ignore[call-arg]
