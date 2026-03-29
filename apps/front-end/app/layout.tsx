import type { Metadata } from "next";
import "@/app/globals.css";
import { AuthProvider } from "@/lib/auth/AuthProvider";

export const metadata: Metadata = {
  title: {
    default: "Gymeal — Fuel Your Performance",
    template: "%s | Gymeal",
  },
  description:
    "AI-powered meal planning and nutrition coaching for athletes and gymers. Track macros, discover meals, and hit your fitness goals.",
  keywords: ["meal planning", "nutrition", "gym", "fitness", "AI coaching", "macros"],
  openGraph: {
    type: "website",
    siteName: "Gymeal",
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="bg-surface-base text-neutral-900 font-sans antialiased">
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
