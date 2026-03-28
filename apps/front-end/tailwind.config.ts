import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
    "./lib/**/*.{ts,tsx}",
  ],
  theme: {
    screens: {
      xs: "375px",   // small phones
      sm: "640px",   // large phones
      md: "768px",   // tablets
      lg: "1024px",  // laptops
      xl: "1280px",  // desktops
      "2xl": "1536px",
      "3xl": "2560px", // large monitors
    },
    extend: {
      colors: {
        // ── Brand / Primary ────────────────────────────────────────────────────
        // Emerald: primary action color — health, nutrition, freshness
        primary: {
          50: "#ecfdf5",
          100: "#d1fae5",
          200: "#a7f3d0",
          300: "#6ee7b7",
          400: "#34d399",
          500: "#10b981", // default
          600: "#059669",
          700: "#047857",
          800: "#065f46",
          900: "#064e3b",
        },
        // ── Accent ─────────────────────────────────────────────────────────────
        // Amber: energy, performance, gym motivation
        accent: {
          50: "#fffbeb",
          100: "#fef3c7",
          200: "#fde68a",
          300: "#fcd34d",
          400: "#fbbf24",
          500: "#f59e0b", // default
          600: "#d97706",
          700: "#b45309",
          800: "#92400e",
          900: "#78350f",
        },
        // ── Success ────────────────────────────────────────────────────────────
        // Lime: goal achieved, macros on track
        success: {
          light: "#ecfccb",
          DEFAULT: "#84cc16",
          dark: "#3f6212",
        },
        // ── Semantic ───────────────────────────────────────────────────────────
        danger: {
          light: "#fee2e2",
          DEFAULT: "#ef4444",
          dark: "#991b1b",
        },
        warning: {
          light: "#fef9c3",
          DEFAULT: "#eab308",
          dark: "#713f12",
        },
        info: {
          light: "#dbeafe",
          DEFAULT: "#3b82f6",
          dark: "#1e3a8a",
        },
        // ── Surfaces ───────────────────────────────────────────────────────────
        surface: {
          base: "#ffffff",     // page background
          muted: "#f9fafb",    // card backgrounds, input fills
          subtle: "#f3f4f6",   // hover states, dividers
          inverse: "#111827",  // dark backgrounds
        },
        // ── Neutrals ───────────────────────────────────────────────────────────
        neutral: {
          50: "#f9fafb",
          100: "#f3f4f6",
          200: "#e5e7eb",
          300: "#d1d5db",
          400: "#9ca3af",
          500: "#6b7280",
          600: "#4b5563",
          700: "#374151",
          800: "#1f2937",
          900: "#111827",
        },
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "sans-serif"],
        mono: ["JetBrains Mono", "Menlo", "monospace"],
      },
      borderRadius: {
        card: "1rem",
        pill: "9999px",
      },
      boxShadow: {
        card: "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)",
        "card-hover": "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)",
      },
      keyframes: {
        // Skeleton shimmer loading animation
        "skeleton-shimmer": {
          "0%": { backgroundPosition: "-200% 0" },
          "100%": { backgroundPosition: "200% 0" },
        },
      },
      animation: {
        "skeleton-shimmer": "skeleton-shimmer 1.5s ease-in-out infinite",
      },
    },
  },
  plugins: [],
};

export default config;
