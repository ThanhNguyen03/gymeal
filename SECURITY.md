# Security Policy

## Supported Versions

| Service | Version | Supported |
|---|---|---|
| Back-end (C#) | Sprint 0+ | ✅ |
| AI Service (Python) | Sprint 0+ | ✅ |
| Front-end (Next.js) | Sprint 0+ | ✅ |

## Reporting a Vulnerability

Please do **not** report security vulnerabilities via GitHub Issues.

Email security concerns to: **security@gymeal.app**

Include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (optional)

We aim to respond within **72 hours** and resolve critical issues within **7 days**.

## Security Architecture Summary

### Authentication
- RS256 JWT access tokens (15-min expiry) + refresh tokens (7-day expiry, HTTP-only cookie)
- No client-side secret storage

### BFF Pattern
- Front-end only calls C# back-end (`NEXT_PUBLIC_API_URL`)
- Python ai-service is **not internet-facing** — internal only via `X-Internal-Auth` shared secret
- Network isolation enforced at Koyeb internal routing layer in production

### Payment Security
- Stripe sandbox/test mode only — no real card data processed
- Webhook endpoint validates Stripe HMAC signature (`STRIPE_WEBHOOK_SECRET`)

### Infrastructure
- Cloudflare WAF + DDoS protection at edge
- HSTS, CSP, X-Frame-Options: DENY on all responses
- Rate limiting: 30 req/s, 500 req/15min per IP

### Dependencies
- Automated weekly security audits via GitHub Dependabot
- `yarn audit`, `dotnet list package --vulnerable`, Poetry lock file
