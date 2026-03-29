# Back-End Environment Setup

## Prerequisites

- **.NET 8 SDK** — required to build and run the API
  ```bash
  brew install --cask dotnet-sdk
  dotnet --version  # should print 8.x.x
  ```
- **Docker Desktop** — runs PostgreSQL 16 + Redis
- **Git**

## First-time setup

```bash
# 1. Start infrastructure
cd /path/to/gymeal
docker compose up -d

# 2. Verify containers are healthy
docker compose ps   # postgres and redis should show "healthy"

# 3. Copy environment config
cp apps/back-end/src/Gymeal.Presentation/.env.example \
   apps/back-end/src/Gymeal.Presentation/.env
# Edit .env and fill in JWT_PRIVATE_KEY_BASE64, JWT_PUBLIC_KEY_BASE64

# 4. Apply migrations
cd apps/back-end
dotnet ef database update \
  --project src/Gymeal.Infrastructure \
  --startup-project src/Gymeal.Presentation

# 5. Run the API
dotnet run --project src/Gymeal.Presentation
# API available at https://localhost:5001
```

## Generating an RS256 key pair

```bash
# Generate private key (PKCS#8 format)
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 \
  -out private.pem

# Derive public key
openssl rsa -in private.pem -pubout -out public.pem

# Base64-encode for config (single line, no headers)
openssl pkey -in private.pem -outform DER | base64 | tr -d '\n'   # -> JWT_PRIVATE_KEY_BASE64
openssl pkey -pubin -in public.pem -outform DER | base64 | tr -d '\n'  # -> JWT_PUBLIC_KEY_BASE64
```

## Environment variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=gymeal;Username=gymeal;Password=gymeal_dev` |
| `REDIS_URL` | Redis connection string | `localhost:6379` |
| `JWT_PRIVATE_KEY_BASE64` | DER-encoded PKCS#8 RSA private key, base64 | (generated above) |
| `JWT_PUBLIC_KEY_BASE64` | DER-encoded SubjectPublicKeyInfo RSA public key, base64 | (generated above) |
| `JWT_ISSUER` | JWT `iss` claim | `gymeal-api` |
| `JWT_AUDIENCE` | JWT `aud` claim | `gymeal-client` |

## Running tests

```bash
cd apps/back-end
dotnet test
```

## Creating a new migration

```bash
cd apps/back-end
dotnet ef migrations add <MigrationName> \
  --project src/Gymeal.Infrastructure \
  --startup-project src/Gymeal.Presentation
```
