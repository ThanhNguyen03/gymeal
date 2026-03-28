import { redirect } from "next/navigation";

/**
 * Root page — redirects to dashboard if authenticated, login if not.
 * Auth check implemented in Sprint 1 (middleware.ts).
 */
export default function RootPage() {
  redirect("/dashboard");
}
