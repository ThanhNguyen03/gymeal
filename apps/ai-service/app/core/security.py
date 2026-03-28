from fastapi import Header, HTTPException, status

from app.core.config import settings


async def verify_internal_auth(
    x_internal_auth: str = Header(..., alias="X-Internal-Auth"),
) -> None:
    """
    FastAPI dependency: verifies X-Internal-Auth shared secret header.
    All routes except /health must pass this check.

    DECISION: internal auth header over JWT because ai-service is not public.
    Trade-off: must ensure network isolation (Koyeb internal routing in prod).
    """
    if x_internal_auth != settings.INTERNAL_AUTH_SECRET:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail={"error": {"code": "INVALID_INTERNAL_AUTH", "message": "Invalid internal auth token."}},
        )
