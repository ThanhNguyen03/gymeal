# Gymeal

AI-powered meal planning and nutrition coaching platform for athletes and gymers.

> **Sprint 0 complete** — Foundation, monorepo, and all three app skeletons scaffolded.

---

## What is Gymeal?

Gymeal connects athletes with meal providers and an AI nutrition coach. Users set their fitness goals and macros, browse provider meals, place orders, and get personalized recommendations powered by a RAG pipeline over USDA/NIH nutrition data.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  apps/front-end  (Next.js 15 + React 19 + Tailwind 4)       │
│  Vercel — NEXT_PUBLIC_API_URL → C# back-end only (BFF)      │
└───────────────────────┬─────────────────────────────────────┘
                        │ REST + GraphQL + SSE
┌───────────────────────▼─────────────────────────────────────┐
│  apps/back-end  (ASP.NET Core 8, C# — Clean Architecture)   │
│  Koyeb — Gymeal.Presentation / .Application / .Domain       │
│  REST /api/v1/* + Hot Chocolate GraphQL + BFF AI proxy      │
└───────┬───────────────┬────────────────────┬────────────────┘
        │               │                    │
   PostgreSQL       Redis 7             X-Internal-Auth
   (Supabase)      (Upstash)          ┌──────▼──────────────┐
   pgvector                           │  apps/ai-service     │
   pg_trgm                            │  (FastAPI + Python)  │
                                      │  Groq LLM + pgvector │
                                      │  Koyeb — INTERNAL    │
                                      └─────────────────────┘
```

**BFF Pattern:** The front-end never calls the Python ai-service directly. All AI traffic goes through the C# back-end (`/api/v1/ai/*` routes), which proxies requests after authentication.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Front-end | Next.js 15, React 19, TypeScript, Tailwind 4, Apollo Client |
| Back-end | ASP.NET Core 8, C# 12, Clean Architecture, MediatR, Hot Chocolate (GraphQL), EF Core 8 |
| AI Service | FastAPI, Python 3.12, SQLAlchemy 2.x, Groq (llama3-70b), pgvector |
| Database | PostgreSQL 16 + pgvector + pg_trgm (Supabase) |
| Cache | Redis 7 (Upstash) |
| Payments | Stripe (sandbox/test mode only) |
| Image CDN | Cloudinary |
| Error tracking | Sentry (all 3 apps) |
| Monorepo | Turborepo + Yarn 4 workspaces |

---

## Quick Start

### Prerequisites

```bash
# Install all dependencies at once (macOS)
brew bundle

# Enable Yarn 4
corepack enable
```

Or run the setup script:

```bash
bash scripts/setup.sh
```

### Manual setup

```bash
# 1. Clone
git clone https://github.com/ThanhNguyen03/gymeal.git
cd gymeal

# 2. Node dependencies
corepack enable
yarn install

# 3. Python dependencies
cd apps/ai-service && poetry install && cd ../..

# 4. .NET packages
dotnet restore apps/back-end

# 5. Environment
cp .example.env .env
# Fill in your values — see .example.env for step-by-step instructions

# 6. Start infrastructure
docker compose up -d

# 7. Start all services
yarn dev
```

### Service URLs (local)

| Service | URL |
|---|---|
| Front-end | http://localhost:3000 |
| Back-end API | http://localhost:5000 |
| Back-end health | http://localhost:5000/health |
| Swagger UI | http://localhost:5000/swagger |
| GraphQL | http://localhost:5000/graphql |
| AI Service | http://localhost:8001 |
| AI Service health | http://localhost:8001/health |

---

## Project Structure

```
gymeal/
├── apps/
│   ├── back-end/           # ASP.NET Core 8 (C#)
│   │   ├── src/
│   │   │   ├── Gymeal.Domain/
│   │   │   ├── Gymeal.Application/
│   │   │   ├── Gymeal.Infrastructure/
│   │   │   └── Gymeal.Presentation/
│   │   └── tests/
│   ├── ai-service/         # FastAPI (Python 3.12)
│   │   └── app/
│   │       ├── core/       # config, database, security
│   │       ├── routers/    # chat, recommendations, nutrition, admin
│   │       ├── models/     # SQLAlchemy ORM (read-only, no migrations)
│   │       └── services/
│   └── front-end/          # Next.js 15
│       ├── app/            # App Router (route groups: (auth), (main))
│       ├── components/
│       └── lib/
│           └── api/client.ts  # Single BFF API client
├── packages/
│   └── types/              # @gymeal/types — shared TypeScript types
├── docker/
│   └── init-db.sql         # Enable pg_trgm + vector extensions
├── scripts/
│   └── setup.sh            # Developer setup script
├── .github/
│   ├── workflows/ci.yml    # CI: lint + build + security audit
│   └── dependabot.yml      # Weekly dependency updates
├── docker-compose.yml      # Local PostgreSQL + Redis
├── .example.env            # Environment variable template + setup guide
├── PLAN.md                 # Full project plan + sprint checklists
├── RULE.md                 # Coding rules + architecture decisions
├── CHANGE_LOG.md           # Sprint-by-sprint changelog
└── SECURITY.md             # Security policy
```

---

## Sprint Progress

| Sprint | Status | Goal |
|---|---|---|
| **Sprint 0** | ✅ Complete | Foundation, monorepo, all 3 app skeletons, CI/CD |
| Sprint 1 | Pending | Auth, JWT, User Profile, Soft Delete, Audit Logs |
| Sprint 2 | Pending | Meal Catalog, Provider, AI Knowledge Base (pgvector) |
| Sprint 3 | Pending | AI Chat via BFF Proxy (SSE streaming) |
| Sprint 4 | Pending | Orders & Payment (Stripe sandbox) |
| Sprint 5 | Pending | Recommendations + Cold-Start Strategy |
| Sprint 6 | Pending | Nutrition Logging |
| Sprint 7 | Pending | Testing (E2E + Integration) |
| Sprint 8 | Pending | Documentation + Polish |

---

## Zero-Cost Stack

Total: **$0.00/month** — see `.example.env` for provisioning guides.

| Service | Free Limit | Use |
|---|---|---|
| Vercel | 100 GB/month | Front-end hosting |
| Koyeb (×2 orgs) | 512 MB RAM each | Back-end + AI service |
| Supabase | 500 MB DB | PostgreSQL + pgvector |
| Upstash Redis | 10k req/day | Cache + sessions |
| Groq | 30 req/min | LLM inference |
| Sentry | 5k errors/month | Error tracking (all 3 apps) |
| Stripe | Test mode — $0 | Payments (sandbox only) |
| Cloudinary | 25 credits/month | Image CDN |
| Logtail | 1 GB/month | Log aggregation |
| Cloudflare | Unlimited | WAF + DDoS + DNS |

---

## Contributing

See [SECURITY.md](SECURITY.md) for the security policy and responsible disclosure.
