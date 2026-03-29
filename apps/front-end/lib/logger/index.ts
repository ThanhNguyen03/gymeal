/**
 * Structured logger for the Next.js front-end.
 * Console transport: always active (JSON in production, pretty in development).
 * Logtail transport: active when LOGTAIL_SOURCE_TOKEN env var is set.
 *
 * NOTE: This logger runs in the Node.js runtime only (server components, API routes).
 * For client-side error reporting, Sentry (@sentry/nextjs) is used directly.
 * Reason: Winston is a Node.js library — it cannot run in the browser bundle.
 */
import winston from "winston";

const isDevelopment = process.env.NODE_ENV === "development";

const logger = winston.createLogger({
  level: isDevelopment ? "debug" : "info",
  defaultMeta: { service: "gymeal-frontend" },
  transports: [
    new winston.transports.Console({
      format: isDevelopment
        ? winston.format.combine(
            winston.format.colorize(),
            winston.format.timestamp(),
            winston.format.printf(
              ({ timestamp, level, message, ...meta }) =>
                `${timestamp} [${level}] ${message} ${Object.keys(meta).length ? JSON.stringify(meta) : ""}`,
            ),
          )
        : winston.format.combine(
            winston.format.timestamp(),
            winston.format.json(),
          ),
    }),
  ],
});

// Logtail HTTP transport — wired in Sprint 2 when @logtail/winston is installed.
// Activate by setting LOGTAIL_SOURCE_TOKEN env var.

export { logger };
export type { Logger } from "winston";
