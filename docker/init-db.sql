-- Gymeal PostgreSQL init script
-- Runs once when the Docker container is first created.
-- Enables extensions required by the application.

-- pg_trgm: trigram-based full-text search for meal name search (PLAN.md §8)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- pgvector: vector similarity search for meal embeddings + user preference vectors
CREATE EXTENSION IF NOT EXISTS vector;

-- uuid-ossp: UUID generation in SQL (used by some legacy queries — EF Core generates UUIDs in app)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
