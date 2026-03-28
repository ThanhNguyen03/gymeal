import { type NextRequest, NextResponse } from "next/server";

/**
 * Edge middleware — JWT validation stub.
 * Sprint 0: pass-through only. Real JWT edge validation in Sprint 1.
 *
 * Protected routes: all /(main)/* routes require authentication.
 * Public routes: /(auth)/login, /(auth)/register, /health
 */
export function middleware(request: NextRequest): NextResponse {
  const { pathname } = request.nextUrl;

  // TODO Sprint 1: validate JWT from cookie, redirect to /login if invalid
  // const token = request.cookies.get("access_token")?.value;
  // if (!token && isProtectedRoute(pathname)) {
  //   return NextResponse.redirect(new URL("/login", request.url));
  // }

  return NextResponse.next();
}

export const config = {
  // Run middleware on all routes except static files and Next.js internals
  matcher: ["/((?!_next/static|_next/image|favicon.ico|public/).*)"],
};
