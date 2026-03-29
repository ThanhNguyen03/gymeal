# Next.js Setup

## App Router structure

```
app/
├── layout.tsx          ← root layout, wraps everything in AuthProvider
├── page.tsx            ← landing page (redirects to /dashboard or /login)
├── error.tsx           ← global error boundary (React error boundary)
├── globals.css         ← Tailwind base + custom tokens + shimmer keyframe
├── (auth)/             ← route group — no shared URL segment
│   ├── layout.tsx      ← minimal centered layout, no sidebar
│   ├── login/
│   └── register/
└── (main)/             ← route group — authenticated area
    ├── layout.tsx      ← top nav stub (expanded in Sprint 2)
    ├── dashboard/
    └── profile/        ← multi-step profile wizard
```

Route groups `(auth)` and `(main)` are folder-level namespaces only — they don't appear in the URL.

## Middleware

`middleware.ts` runs at the Edge before every request (except static files):

1. Reads `access_token` cookie
2. Structurally validates the JWT (checks `exp` claim — **not** the RS256 signature)
3. Redirects unauthenticated users to `/login?next=<pathname>` for protected routes
4. Redirects authenticated users away from `/login` and `/register` to `/dashboard`

Full RS256 signature verification happens at the C# backend on every API call.

## Security headers

Configured in `next.config.ts` (Sprint 0):
- `Content-Security-Policy` — restricts script/style/connect sources
- `Strict-Transport-Security` — HSTS 1 year
- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`

## API proxy

All `/api/v1/*` requests are proxied to the C# backend via the `NEXT_PUBLIC_API_URL` config. The front-end never calls the Python ai-service directly — it goes through C# (BFF pattern).
