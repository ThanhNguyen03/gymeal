/**
 * @gymeal/types — Shared TypeScript types for Gymeal monorepo.
 *
 * Sprint 0: placeholder only.
 * Sprint 1+: types generated from OpenAPI spec (apps/back-end) and
 * GraphQL schema (Hot Chocolate) will be added here.
 *
 * Usage:
 *   import type { Meal, Order } from "@gymeal/types";
 */

// ── Pagination ─────────────────────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CursorPageResult<T> {
  edges: Array<{ node: T; cursor: string }>;
  pageInfo: {
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    startCursor: string | null;
    endCursor: string | null;
  };
}

// ── API Error (Problem Details RFC 7807) ──────────────────────────────────────
export interface ApiProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  correlationId?: string;
}

// ── Health check ──────────────────────────────────────────────────────────────
export interface HealthCheckResponse {
  status: "Healthy" | "Degraded" | "Unhealthy";
  duration: number;
  entries: Record<
    string,
    { status: string; description?: string; duration: number }
  >;
}
