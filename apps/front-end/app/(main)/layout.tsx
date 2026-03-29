import type { Metadata } from "next";

export const metadata: Metadata = {
  title: {
    default: "Gymeal",
    template: "%s | Gymeal",
  },
};

export default function MainLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-surface-base flex flex-col">
      {/* Top navigation bar — expanded in Sprint 2 */}
      <header className="h-14 border-b border-neutral-200 bg-surface-base flex items-center px-4 md:px-6 shrink-0">
        <span className="text-lg font-bold text-primary-500 tracking-tight">
          Gymeal
        </span>
      </header>

      <main className="flex-1 w-full max-w-5xl mx-auto px-4 md:px-6 py-6">
        {children}
      </main>
    </div>
  );
}
