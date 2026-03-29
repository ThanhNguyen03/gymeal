import { type NextRequest, NextResponse } from "next/server";

// Routes that require authentication
const PROTECTED_PREFIXES = [
  "/dashboard",
  "/meals",
  "/chat",
  "/orders",
  "/nutrition",
  "/profile",
  "/checkout",
  "/provider",
  "/admin",
];

// Auth routes — redirect to /dashboard if already authenticated
const AUTH_ROUTES = ["/login", "/register"];

/**
 * Structural JWT validation — checks form and expiry only.
 * RS256 signature verification is intentionally skipped at edge:
 * the RSA private key is only available in C# backend, and full
 * verification happens there on every authenticated API call.
 */
function isStructurallyValidToken(token: string): boolean {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return false;

    // base64url decode the payload
    const payloadBase64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const payloadJson = atob(payloadBase64);
    const payload = JSON.parse(payloadJson) as { exp?: number };

    if (typeof payload.exp !== "number") return false;

    // 30-second buffer to avoid redirecting on tokens about to expire
    const nowSeconds = Math.floor(Date.now() / 1000);
    return payload.exp > nowSeconds + 30;
  } catch {
    return false;
  }
}

export function middleware(request: NextRequest): NextResponse {
  const { pathname } = request.nextUrl;

  const token = request.cookies.get("access_token")?.value;
  const hasValidToken = token ? isStructurallyValidToken(token) : false;

  const isProtected = PROTECTED_PREFIXES.some((prefix) =>
    pathname.startsWith(prefix),
  );
  const isAuthRoute = AUTH_ROUTES.some((route) => pathname.startsWith(route));

  if (isProtected && !hasValidToken) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("next", pathname);
    return NextResponse.redirect(loginUrl);
  }

  if (isAuthRoute && hasValidToken) {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|public/).*)"],
};
