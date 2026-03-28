#!/usr/bin/env bash
# ============================================================
# Gymeal — Developer Setup Script
# Run once after cloning the repository.
# Usage: bash scripts/setup.sh
# ============================================================
set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log()  { echo -e "${GREEN}[setup]${NC} $1"; }
warn() { echo -e "${YELLOW}[warn]${NC}  $1"; }
fail() { echo -e "${RED}[error]${NC} $1"; exit 1; }

# ── 1. Check required tools ───────────────────────────────────────────────────
log "Checking required tools..."

command -v node   >/dev/null 2>&1 || fail "Node.js not found. Run: brew install node  OR use nvm: nvm install $(cat .nvmrc)"
command -v yarn   >/dev/null 2>&1 || fail "Yarn not found. Run: npm install -g corepack && corepack enable"
command -v dotnet >/dev/null 2>&1 || fail ".NET SDK not found. Run: brew install --cask dotnet-sdk  OR visit https://dot.net/download"
command -v poetry >/dev/null 2>&1 || fail "Poetry not found. Run: brew install poetry  OR curl -sSL https://install.python-poetry.org | python3 -"
command -v docker >/dev/null 2>&1 || fail "Docker not found. Install Docker Desktop: https://www.docker.com/products/docker-desktop"

log "All required tools found."

# ── 2. Enable Yarn v4 ─────────────────────────────────────────────────────────
log "Enabling Corepack for Yarn 4..."
corepack enable 2>/dev/null || warn "Corepack not available — you may need to run: npm install -g corepack"

# ── 3. Install Node dependencies ──────────────────────────────────────────────
log "Installing Node.js dependencies..."
yarn install

# ── 4. Install Python dependencies ───────────────────────────────────────────
log "Installing Python dependencies for ai-service..."
cd apps/ai-service
poetry install --no-interaction
cd ../..

# ── 5. Restore .NET packages ─────────────────────────────────────────────────
log "Restoring .NET packages..."
dotnet restore apps/back-end

# ── 6. Copy .example.env → .env (only if .env doesn't exist) ─────────────────
if [ ! -f ".env" ]; then
  log "Creating .env from .example.env..."
  cp .example.env .env
  warn ".env created. Fill in your real values before running services."
  warn "See .example.env for step-by-step instructions for each service."
else
  log ".env already exists — skipping copy."
fi

# ── 7. Start infrastructure (Docker) ─────────────────────────────────────────
log "Starting Docker infrastructure (PostgreSQL + Redis)..."
docker compose up -d

log "Waiting for PostgreSQL to be ready..."
RETRIES=10
until docker compose exec postgres pg_isready -U gymeal -d gymeal_db >/dev/null 2>&1 || [ $RETRIES -eq 0 ]; do
  RETRIES=$((RETRIES - 1))
  sleep 2
done

if [ $RETRIES -eq 0 ]; then
  warn "PostgreSQL did not become ready in time. Run: docker compose up -d"
else
  log "PostgreSQL is ready."
fi

# ── Done ──────────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}=====================================================${NC}"
echo -e "${GREEN}  Gymeal setup complete!${NC}"
echo -e "${GREEN}=====================================================${NC}"
echo ""
echo "  Start all services:  yarn dev"
echo "  Back-end only:       cd apps/back-end && dotnet run --project src/Gymeal.Presentation"
echo "  AI service only:     cd apps/ai-service && poetry run uvicorn app.main:app --reload --port 8001"
echo "  Front-end only:      cd apps/front-end && yarn dev"
echo ""
echo "  Health checks:"
echo "    Back-end:   http://localhost:5000/health"
echo "    AI service: http://localhost:8001/health"
echo "    Front-end:  http://localhost:3000"
echo ""
