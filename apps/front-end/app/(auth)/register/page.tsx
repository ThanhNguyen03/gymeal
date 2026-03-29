"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/lib/auth/AuthProvider";
import { ApiError } from "@/lib/api/client";

interface IPasswordStrength {
  score: number; // 0-4
  label: string;
  color: string;
}

function getPasswordStrength(password: string): IPasswordStrength {
  let score = 0;
  if (password.length >= 8) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^a-zA-Z0-9]/.test(password)) score++;

  const map: Record<number, { label: string; color: string }> = {
    0: { label: "Too weak", color: "bg-danger" },
    1: { label: "Weak", color: "bg-danger" },
    2: { label: "Fair", color: "bg-warning" },
    3: { label: "Good", color: "bg-accent-500" },
    4: { label: "Strong", color: "bg-success" },
  };

  return { score, ...map[score] };
}

export default function RegisterPage() {
  const router = useRouter();
  const { register } = useAuth();

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const strength = getPasswordStrength(password);
  const canSubmit = fullName && email && password.length >= 8 && !isLoading;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await register(email, password, fullName);
      // Redirect to profile wizard for first-time setup
      router.push("/profile");
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          setError("This email is already registered. Try signing in instead.");
        } else if (err.status === 422) {
          const body = err.body as { detail?: string } | undefined;
          setError(body?.detail ?? "Please check your input and try again.");
        } else {
          setError("Something went wrong. Please try again.");
        }
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <>
      <h2 className="text-xl font-semibold text-neutral-900 mb-6">
        Create your account
      </h2>

      {error && (
        <div
          role="alert"
          className="mb-4 p-3 bg-danger-light text-danger rounded-md text-sm"
        >
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        <div>
          <label
            htmlFor="fullName"
            className="block text-sm font-medium text-neutral-700 mb-1"
          >
            Full name
          </label>
          <input
            id="fullName"
            type="text"
            autoComplete="name"
            required
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Alex Johnson"
            className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm
                       focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
                       disabled:bg-neutral-100"
            disabled={isLoading}
          />
        </div>

        <div>
          <label
            htmlFor="email"
            className="block text-sm font-medium text-neutral-700 mb-1"
          >
            Email
          </label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@example.com"
            className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm
                       focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
                       disabled:bg-neutral-100"
            disabled={isLoading}
          />
        </div>

        <div>
          <label
            htmlFor="password"
            className="block text-sm font-medium text-neutral-700 mb-1"
          >
            Password
          </label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Min. 8 characters"
            className="w-full px-3 py-2 border border-neutral-300 rounded-md text-sm
                       focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
                       disabled:bg-neutral-100"
            disabled={isLoading}
          />

          {/* Password strength indicator */}
          {password.length > 0 && (
            <div className="mt-2">
              <div className="flex gap-1 mb-1" aria-hidden="true">
                {[1, 2, 3, 4].map((step) => (
                  <div
                    key={step}
                    className={`h-1 flex-1 rounded-full transition-colors ${
                      strength.score >= step
                        ? strength.color
                        : "bg-neutral-200"
                    }`}
                  />
                ))}
              </div>
              <p className="text-xs text-neutral-500">
                Strength:{" "}
                <span className="font-medium">{strength.label}</span>
                {strength.score < 4 && (
                  <span>
                    {" "}
                    — add uppercase, numbers, and special characters
                  </span>
                )}
              </p>
            </div>
          )}
        </div>

        <button
          type="submit"
          disabled={!canSubmit}
          className="w-full bg-primary-500 hover:bg-primary-600 disabled:bg-neutral-300
                     text-white font-medium py-2 px-4 rounded-md transition-colors
                     focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
        >
          {isLoading ? "Creating account…" : "Create account"}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-neutral-500">
        Already have an account?{" "}
        <Link
          href="/login"
          className="text-primary-600 hover:text-primary-700 font-medium"
        >
          Sign in
        </Link>
      </p>
    </>
  );
}
