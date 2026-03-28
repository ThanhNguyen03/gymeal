from fastapi import APIRouter, Depends

from app.core.security import verify_internal_auth

router = APIRouter(
    prefix="/nutrition",
    tags=["nutrition"],
    dependencies=[Depends(verify_internal_auth)],
)

# Sprint 0 — route stubs only. Full implementation in Sprint 6.


@router.post("/analyze")
async def analyze_nutrition() -> dict[str, str]:
    """POST /nutrition/analyze — analyze nutritional content via LLM."""
    return {"message": "Not implemented. Available in Sprint 6."}
