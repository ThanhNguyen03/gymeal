# TypeScript Configuration

## `tsconfig.json` highlights

```json
{
  "compilerOptions": {
    "strict": true,           // enables all strict checks
    "noUncheckedIndexedAccess": true,  // array[i] returns T | undefined
    "exactOptionalPropertyTypes": true // { a?: string } ≠ { a: string | undefined }
  }
}
```

## Path aliases

Import using `@/` instead of relative paths:

```typescript
import { useAuth } from "@/lib/auth/AuthProvider";  // ✓
import { useAuth } from "../../../lib/auth/AuthProvider";  // ✗
```

`@/` maps to the `apps/front-end/` root (configured in both `tsconfig.json` and `next.config.ts`).

## Strict mode implications

- `null` and `undefined` are distinct types — use optional chaining and nullish coalescing
- Function parameters and return types are inferred but can always be explicit
- `noUncheckedIndexedAccess` means `array[0]` is `T | undefined` — check before use

## Key type files

| File | Exports |
|------|---------|
| `lib/api/auth.ts` | `AuthResponse`, `UserProfile` |
| `lib/api/profile.ts` | `UpdateProfileRequest` |
| `lib/api/client.ts` | `ApiError`, `apiClient` |
