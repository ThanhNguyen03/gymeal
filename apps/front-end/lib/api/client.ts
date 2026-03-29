/**
 * Single API client for all HTTP requests to the C# back-end.
 * The front-end NEVER calls the Python ai-service directly — BFF pattern.
 * All requests go through NEXT_PUBLIC_API_URL (the back-end URL).
 */

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type TRequestOptions = RequestInit & {
  params?: Record<string, string | number | boolean | undefined>;
};

function buildUrl(path: string, params?: Record<string, string | number | boolean | undefined>): string {
  const url = new URL(path, API_BASE_URL);
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined) {
        url.searchParams.set(key, String(value));
      }
    });
  }
  return url.toString();
}

function generateCorrelationId(): string {
  return crypto.randomUUID();
}

async function request<T>(path: string, options: TRequestOptions = {}): Promise<T> {
  const { params, headers, ...rest } = options;
  const url = buildUrl(path, params);

  const response = await fetch(url, {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      "X-Correlation-Id": generateCorrelationId(),
      ...headers,
    },
    credentials: "include", // send cookies (refresh token)
  });

  if (!response.ok) {
    const errorBody = await response.json().catch(() => ({}));
    throw new ApiError(response.status, errorBody);
  }

  // 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly body: unknown,
  ) {
    super(`API error ${status}`);
    this.name = "ApiError";
  }
}

export const apiClient = {
  get: <T>(path: string, options?: TRequestOptions) =>
    request<T>(path, { method: "GET", ...options }),

  post: <T>(path: string, body?: unknown, options?: TRequestOptions) =>
    request<T>(path, {
      method: "POST",
      body: body !== undefined ? JSON.stringify(body) : undefined,
      ...options,
    }),

  put: <T>(path: string, body?: unknown, options?: TRequestOptions) =>
    request<T>(path, {
      method: "PUT",
      body: body !== undefined ? JSON.stringify(body) : undefined,
      ...options,
    }),

  patch: <T>(path: string, body?: unknown, options?: TRequestOptions) =>
    request<T>(path, {
      method: "PATCH",
      body: body !== undefined ? JSON.stringify(body) : undefined,
      ...options,
    }),

  delete: <T>(path: string, options?: TRequestOptions) =>
    request<T>(path, { method: "DELETE", ...options }),
};
