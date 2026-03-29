# REST Controllers

All controllers inherit from `ApiControllerBase` which provides the shared `MapError(Error)` method.

---

## ApiControllerBase

Abstract base: `Gymeal.Presentation.Controllers.ApiControllerBase`

Maps `Error.Code` suffixes to HTTP status codes:

| Code suffix | HTTP status |
|------------|-------------|
| `*NotFound` | 404 |
| `*Unauthorized` | 401 |
| `*Forbidden` | 403 |
| `*Conflict` | 409 |
| `*Failed` | 422 |
| (default) | 500 |

All error responses include `correlationId` in the `extensions` field for distributed tracing.

---

## AuthController

Route: `POST /api/v1/auth/*` — Public endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/register` | POST | None | Register; sets `access_token` + `refresh_token` HttpOnly cookies |
| `/login` | POST | None | Login; sets auth cookies |
| `/refresh` | POST | None | Rotate refresh token; reads `refresh_token` cookie |
| `/logout` | DELETE | JWT | Revoke refresh token; clears cookies |

---

## UsersController

Route: `GET|PUT /api/v1/users/*` — Requires JWT

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/me` | GET | Get current user's profile |
| `/me/profile` | PUT | Partial update — only provided fields change |

---

## AdminController

Route: `PATCH /api/v1/admin/*` — Requires `[Authorize(Roles = "Admin")]`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/providers/{id}/verify` | PATCH | Mark provider as verified — meals become publicly visible |

**Security:** Double authorization — controller `[Authorize(Roles = "Admin")]` + handler re-checks `currentUser.Role == "Admin"`.

---

## MediaController

Route: `POST /api/v1/media/*` — Requires JWT

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/upload-url` | POST | Returns server-side signed Cloudinary upload URL |

**Request body:** `{ "folder": "meals", "fileName": "grilled-chicken.jpg" }`

**Allowed folders:** `meals`, `providers`, `avatars`

**Security:**
- Folder whitelist prevents uploads to arbitrary Cloudinary paths
- File extension validated by `ServiceCloudinaryStorage` (jpg/jpeg/png/webp only)
- Cloudinary credentials never returned to client — only a short-lived signed URL
