/**
 * Base skeleton primitive following the Orochi shimmer pattern.
 * All composite skeleton components (MealCard.skeleton, etc.) are built on this.
 *
 * Usage:
 *   <ContentSkeleton variant="text" width="w-3/4" height="h-4" />
 *   <ContentSkeleton variant="circular" width="w-10" height="h-10" />
 *   <ContentSkeleton variant="rounded" width="w-full" height="h-48" />
 */
import { memo } from "react";

type TSkeletonVariant = "text" | "circular" | "rectangular" | "rounded";

const VARIANT_CLASS: Record<TSkeletonVariant, string> = {
  text: "rounded-sm",
  circular: "rounded-full",
  rectangular: "rounded-none",
  rounded: "rounded-md",
};

interface IContentSkeletonProps {
  variant?: TSkeletonVariant;
  /** Tailwind width class, e.g. "w-full", "w-48" */
  width?: string;
  /** Tailwind height class, e.g. "h-4", "h-12" */
  height?: string;
  className?: string;
}

function ContentSkeletonBase({
  variant = "rounded",
  width = "w-full",
  height = "h-4",
  className = "",
}: IContentSkeletonProps) {
  return (
    <div
      aria-hidden="true"
      className={[
        "skeleton-shimmer",
        "bg-neutral-200",
        VARIANT_CLASS[variant],
        width,
        height,
        className,
      ]
        .filter(Boolean)
        .join(" ")}
    />
  );
}

/**
 * Wrapped with memo() — skeleton shapes are purely presentational and never
 * need to re-render while data is loading.
 */
export const ContentSkeleton = memo(ContentSkeletonBase);
