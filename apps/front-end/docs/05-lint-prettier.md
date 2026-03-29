# Lint & Prettier

## ESLint

Configured in `eslint.config.mjs` (flat config format, ESLint 9+).

Run:
```bash
yarn workspace gymeal-frontend lint
```

Key rules:
- `@typescript-eslint/no-explicit-any` — error; use `unknown` instead
- `@typescript-eslint/no-unused-vars` — error
- `react-hooks/rules-of-hooks` — error
- `react-hooks/exhaustive-deps` — warn

## Prettier

Run:
```bash
yarn workspace gymeal-frontend format        # fix in-place
yarn workspace gymeal-frontend format:check  # CI check (no writes)
```

Key settings (`.prettierrc`):
- `printWidth: 80`
- `tabWidth: 2`
- `singleQuote: false` (double quotes)
- `trailingComma: "all"`
- `semi: true`

## Pre-commit hooks

Husky + lint-staged run lint and format checks on staged files before each commit. If the hook fails, fix the reported issues and re-stage.

## CI

GitHub Actions runs `lint` and `format:check` for every push/PR via `.github/workflows/ci.yml`. A failed lint or format check blocks merging.
