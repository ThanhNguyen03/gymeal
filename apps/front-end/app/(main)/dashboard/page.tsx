import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Dashboard",
};

export default function DashboardPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-neutral-900 mb-2">Dashboard</h1>
      <p className="text-neutral-500 text-sm">
        Your nutrition overview will appear here in Sprint 2.
      </p>
    </div>
  );
}
