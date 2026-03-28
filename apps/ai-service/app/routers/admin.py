from fastapi import APIRouter, Depends

from app.core.security import verify_internal_auth

router = APIRouter(
    prefix="/admin",
    tags=["admin"],
    dependencies=[Depends(verify_internal_auth)],
)

# Sprint 0 — route stubs only. Full implementation in Sprint 2.


@router.post("/ingest-documents")
async def ingest_documents() -> dict[str, str]:
    """POST /admin/ingest-documents — seed nutrition docs into pgvector. Admin only."""
    return {"message": "Not implemented. Available in Sprint 2."}


@router.post("/events/behavior")
async def track_behavior_event() -> dict[str, str]:
    """POST /events/behavior — log user behavior events for preference learning."""
    return {"message": "Not implemented. Available in Sprint 5."}
