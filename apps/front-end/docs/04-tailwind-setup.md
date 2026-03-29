# Tailwind Setup

## Custom design tokens

Defined in `tailwind.config.ts`. Always use these tokens instead of raw Tailwind colors.

### Colors

| Token | Usage |
|-------|-------|
| `primary-{50..900}` | Brand color (buttons, links, focus rings) |
| `neutral-{50..900}` | Text, borders, backgrounds |
| `success` | Positive states (password strength: strong) |
| `warning` | Caution states (password strength: fair) |
| `danger` | Errors, destructive actions |
| `danger-light` | Error alert backgrounds |
| `accent-500` | Accent (password strength: good) |
| `surface-base` | Page/card background |
| `surface-muted` | Page background behind cards |

### Border radius

| Token | Value |
|-------|-------|
| `rounded-card` | Card border radius |

### Shadows

| Token | Usage |
|-------|-------|
| `shadow-card` | Card/panel shadow |

## Skeleton shimmer

`globals.css` defines the shimmer animation used by `ContentSkeleton`:

```css
@keyframes skeleton-shimmer { ... }
.skeleton-shimmer { animation: skeleton-shimmer 1.5s infinite linear; }
```

Use `ContentSkeleton` from `components/skeletons/` for loading states.

## Responsive breakpoints

Standard Tailwind breakpoints apply: `sm` (640px), `md` (768px), `lg` (1024px), `xl` (1280px), `2xl` (1536px).

Common patterns in this codebase:
- Full-width on mobile → centered card on md+: `w-full max-w-md mx-auto`
- Stack on mobile → side-by-side on md+: `flex flex-col md:flex-row`
