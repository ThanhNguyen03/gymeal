# Front-End Environment Setup

## Prerequisites

- **Node.js 20 LTS** — check with `node --version`
- **Corepack** — ships with Node; enables Yarn Berry automatically
  ```bash
  corepack enable
  ```

## First-time setup

```bash
# From repo root
yarn install            # installs all workspace dependencies

# Copy env file
cp apps/front-end/.env.example apps/front-end/.env.local
# Fill in NEXT_PUBLIC_API_URL (default: http://localhost:5001)

# Start dev server
yarn workspace gymeal-frontend dev
# Or from apps/front-end:
yarn dev
# App available at http://localhost:3000
```

## Environment variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | C# back-end base URL | `http://localhost:5001` |
| `LOGTAIL_SOURCE_TOKEN` | Logtail logging token (optional) | — |

Variables prefixed `NEXT_PUBLIC_` are inlined at build time and exposed to the browser. Never put secrets in `NEXT_PUBLIC_` variables.

## Running the full stack locally

```bash
# Terminal 1 — Infrastructure
docker compose up -d

# Terminal 2 — C# API
cd apps/back-end && dotnet run --project src/Gymeal.Presentation

# Terminal 3 — Next.js
cd apps/front-end && yarn dev
```

## Build for production

```bash
yarn workspace gymeal-frontend build
yarn workspace gymeal-frontend start
```
