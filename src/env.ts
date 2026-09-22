// Worker bindings plus the values that only exist as secrets or vars.
// Secrets are set with `wrangler secret put` and never committed.
export type AppEnv = Env & {
  OLLAMA_API_KEY?: string;
  OLLAMA_MODEL?: string;
  /** PBKDF2 rounds for new passwords. Stored per user, so lowering it is safe. */
  PASSWORD_ITERATIONS?: string;
  /** Idle session lifetime in days (default 14, capped at 30). */
  SESSION_TTL_DAYS?: string;
};
