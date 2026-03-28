from fastapi import APIRouter, Depends

from app.core.security import verify_internal_auth

router = APIRouter(
    prefix="/chat",
    tags=["chat"],
    dependencies=[Depends(verify_internal_auth)],
)

# Sprint 0 — route stubs only. Full implementation in Sprint 3.


@router.post("/sessions")
async def create_chat_session() -> dict[str, str]:
    """POST /chat/sessions — create a new chat session."""
    return {"message": "Not implemented. Available in Sprint 3."}


@router.get("/sessions/{session_id}/messages")
async def get_chat_messages(session_id: str) -> dict[str, str]:
    """GET /chat/sessions/{id}/messages — cursor-paginated messages."""
    return {"message": "Not implemented. Available in Sprint 3."}


@router.post("/sessions/{session_id}/messages")
async def send_chat_message(session_id: str) -> dict[str, str]:
    """POST /chat/sessions/{id}/messages — SSE streaming response."""
    return {"message": "Not implemented. Available in Sprint 3."}


@router.post("/messages/{message_id}/feedback")
async def submit_message_feedback(message_id: str) -> dict[str, str]:
    """POST /chat/messages/{id}/feedback — thumbs up/down."""
    return {"message": "Not implemented. Available in Sprint 3."}
