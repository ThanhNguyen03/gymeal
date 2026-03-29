"use client";

import * as Sentry from "@sentry/nextjs";
import { useEffect } from "react";

interface IErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

/**
 * Root error boundary — catches unhandled errors in the React tree.
 * Reports to Sentry in production (Sentry.captureException added in Sprint 0 wiring).
 */
export default function GlobalError({ error, reset }: IErrorProps) {
  useEffect(() => {
    Sentry.captureException(error);
    console.error("[GlobalError]", error);
  }, [error]);

  return (
    <html lang="en">
      <body className="min-h-screen bg-surface-muted flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-card shadow-card p-8 text-center">
          <div className="w-16 h-16 bg-danger-light rounded-full flex items-center justify-center mx-auto mb-4">
            <span className="text-danger text-2xl" aria-hidden="true">!</span>
          </div>
          <h1 className="text-xl font-semibold text-neutral-900 mb-2">
            Something went wrong
          </h1>
          <p className="text-neutral-500 text-sm mb-6">
            {process.env.NODE_ENV === "development"
              ? error.message
              : "An unexpected error occurred. Please try again."}
          </p>
          {error.digest && (
            <p className="text-xs text-neutral-400 mb-4 font-mono">
              Error ID: {error.digest}
            </p>
          )}
          <button
            onClick={reset}
            className="w-full bg-primary-500 hover:bg-primary-600 text-white font-medium py-2 px-4 rounded-lg transition-colors"
          >
            Try again
          </button>
        </div>
      </body>
    </html>
  );
}
