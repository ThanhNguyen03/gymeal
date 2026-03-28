from fastapi import APIRouter, Depends

from app.core.security import verify_internal_auth

router = APIRouter(
    prefix="/recommendations",
    tags=["recommendations"],
    dependencies=[Depends(verify_internal_auth)],
)

# Sprint 0 — route stubs only. Full implementation in Sprint 5.


@router.post("/generate")
async def generate_recommendations() -> dict[str, str]:
    """POST /recommendations/generate — generate personalized meal recommendations."""
    return {"message": "Not implemented. Available in Sprint 5."}


@router.get("/{user_id}")
async def get_recommendations(user_id: str) -> dict[str, str]:
    """GET /recommendations/{userId} — fetch cached recommendations."""
    return {"message": "Not implemented. Available in Sprint 5."}
