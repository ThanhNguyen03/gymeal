import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Sign in to Gymeal",
};

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-surface-muted flex flex-col items-center justify-center p-4">
      {/* Brand header */}
      <div className="mb-8 text-center">
        <h1 className="text-3xl font-bold text-primary-500 tracking-tight">
          Gymeal
        </h1>
        <p className="text-neutral-500 text-sm mt-1">
          Fuel your performance
        </p>
      </div>

      {/* Auth card */}
      <div className="w-full max-w-md bg-surface-base rounded-card shadow-card p-8">
        {children}
      </div>
    </div>
  );
}
