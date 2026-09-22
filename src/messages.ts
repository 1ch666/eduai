// Shared study notes board. Anonymous posting still works; a signed-in visitor
// simply gets their display name stored alongside the note.
import { DurableObject } from "cloudflare:workers";
import { networkKeyFor, readJsonObject, type Responder } from "./http";
import { csrfTokenMatches, resolveSession } from "./session";
import type { AppEnv } from "./env";

export const MESSAGE_ROOM_NAME = "civic-notes-main";

export type MessageAuthor = { id: string; displayName: string };

export type Message = {
  id: string;
  text: string;
  createdAt: string;
  author: MessageAuthor | null;
};

type StoredMessage = {
  id: string;
  text: string;
  created_at: string;
  author_id: string | null;
  author_name: string | null;
};

export type AddMessageResult =
  | { message: Message; error?: never }
  | { message?: never; error: "INVALID_MESSAGE" | "INVALID_CLIENT" | "INVALID_ID" | "RATE_LIMIT" };

function toMessage(row: StoredMessage): Message {
  return {
    id: row.id,
    text: row.text,
    createdAt: row.created_at,
    author: row.author_id && row.author_name ? { id: row.author_id, displayName: row.author_name } : null
  };
}

export class MessageRoom extends DurableObject<AppEnv> {
  constructor(ctx: DurableObjectState, env: AppEnv) {
    super(ctx, env);
    ctx.blockConcurrencyWhile(async () => {
      this.ctx.storage.sql.exec(`
        CREATE TABLE IF NOT EXISTS messages (
          id TEXT PRIMARY KEY,
          text TEXT NOT NULL CHECK(length(text) BETWEEN 1 AND 500),
          created_at TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS messages_created_at ON messages(created_at);
        CREATE TABLE IF NOT EXISTS rate_limits (
          client_id TEXT PRIMARY KEY,
          window_start INTEGER NOT NULL,
          message_count INTEGER NOT NULL
        );
      `);
      // The board shipped before accounts existed, so widen the live table.
      const columns = this.ctx.storage.sql
        .exec<{ name: string }>("PRAGMA table_info(messages)")
        .toArray()
        .map(row => row.name);
      if (!columns.includes("author_id")) this.ctx.storage.sql.exec("ALTER TABLE messages ADD COLUMN author_id TEXT");
      if (!columns.includes("author_name")) this.ctx.storage.sql.exec("ALTER TABLE messages ADD COLUMN author_name TEXT");
    });
  }

  getMessages(): Message[] {
    return this.ctx.storage.sql
      .exec<StoredMessage>(
        "SELECT id, text, created_at, author_id, author_name FROM messages ORDER BY created_at DESC LIMIT 100"
      )
      .toArray()
      .reverse()
      .map(toMessage);
  }

  addMessage(
    text: string,
    clientId: string,
    messageId: string,
    networkKey: string,
    author: MessageAuthor | null = null
  ): AddMessageResult {
    const normalizedText = text.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]/g, "").trim();
    if (!normalizedText || normalizedText.length > 500) return { error: "INVALID_MESSAGE" };
    if (!/^[a-zA-Z0-9-]{16,80}$/.test(clientId)) return { error: "INVALID_CLIENT" };
    if (!/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(messageId)) return { error: "INVALID_ID" };

    const existing = this.ctx.storage.sql
      .exec<StoredMessage>("SELECT id, text, created_at, author_id, author_name FROM messages WHERE id = ?", messageId)
      .toArray()[0];
    if (existing) return { message: toMessage(existing) };

    const now = Date.now();
    const windowStart = Math.floor(now / 60_000) * 60_000;
    const rateKeys = [[`client:${clientId}`, 10] as const, [`network:${networkKey}`, 30] as const];
    for (const [key, limit] of rateKeys) {
      const rate = this.ctx.storage.sql
        .exec<{ window_start: number; message_count: number }>("SELECT window_start, message_count FROM rate_limits WHERE client_id = ?", key)
        .toArray()[0];
      if (rate?.window_start === windowStart && rate.message_count >= limit) return { error: "RATE_LIMIT" };
    }
    for (const [key] of rateKeys) {
      this.ctx.storage.sql.exec(
        `INSERT INTO rate_limits (client_id, window_start, message_count)
         VALUES (?, ?, 1)
         ON CONFLICT(client_id) DO UPDATE SET
           window_start = excluded.window_start,
           message_count = CASE WHEN rate_limits.window_start = excluded.window_start THEN rate_limits.message_count + 1 ELSE 1 END`,
        key,
        windowStart
      );
    }

    const message: Message = {
      id: messageId,
      text: normalizedText,
      createdAt: new Date(now).toISOString(),
      author: author && author.id && author.displayName ? { id: author.id, displayName: author.displayName.slice(0, 24) } : null
    };
    this.ctx.storage.sql.exec(
      "INSERT INTO messages (id, text, created_at, author_id, author_name) VALUES (?, ?, ?, ?, ?)",
      message.id,
      message.text,
      message.createdAt,
      message.author?.id ?? null,
      message.author?.displayName ?? null
    );
    this.ctx.storage.sql.exec(
      "DELETE FROM messages WHERE id NOT IN (SELECT id FROM messages ORDER BY created_at DESC LIMIT 2000)"
    );
    return { message };
  }

  allowAiRequest(clientId: string, networkKey: string, userId: string | null = null): boolean {
    if (!/^[a-zA-Z0-9-]{16,80}$/.test(clientId)) return false;
    const now = Date.now();
    const windowStart = Math.floor(now / 60_000) * 60_000;
    const rateKeys: (readonly [string, number])[] = [
      [`ai-client:${clientId}`, 4] as const,
      [`ai-network:${networkKey}`, 12] as const
    ];
    // Signed-in askers get their own bucket so one shared classroom IP is fairer.
    if (userId) rateKeys.push([`ai-user:${userId}`, 8] as const);
    for (const [key, limit] of rateKeys) {
      const rate = this.ctx.storage.sql
        .exec<{ window_start: number; message_count: number }>("SELECT window_start, message_count FROM rate_limits WHERE client_id = ?", key)
        .toArray()[0];
      if (rate?.window_start === windowStart && rate.message_count >= limit) return false;
    }
    for (const [key] of rateKeys) {
      this.ctx.storage.sql.exec(
        `INSERT INTO rate_limits (client_id, window_start, message_count)
         VALUES (?, ?, 1)
         ON CONFLICT(client_id) DO UPDATE SET
           window_start = excluded.window_start,
           message_count = CASE WHEN rate_limits.window_start = excluded.window_start THEN rate_limits.message_count + 1 ELSE 1 END`,
        key,
        windowStart
      );
    }
    return true;
  }
}

export function messageRoom(env: AppEnv): DurableObjectStub<MessageRoom> {
  // Wrangler generates the namespace without its RPC generic; narrow it here.
  return env.MESSAGE_ROOM.getByName(MESSAGE_ROOM_NAME) as DurableObjectStub<MessageRoom>;
}

export async function handleMessages(
  request: Request,
  env: AppEnv,
  respond: Responder,
  trustedOrigin: string | undefined
): Promise<Response> {
  const room = messageRoom(env);
  if (request.method === "GET") return respond({ messages: await room.getMessages() });
  if (request.method !== "POST") {
    return respond({ error: "此端點只接受 GET 或 POST" }, 405);
  }
  if (!trustedOrigin) return respond({ error: "拒絕未授權網站寫入" }, 403);

  const body = await readJsonObject(request, 4096, respond, "留言內容過長");
  if (body.error) return body.error;
  const candidate = body.value;
  if (typeof candidate.text !== "string" || typeof candidate.clientId !== "string" || typeof candidate.messageId !== "string") {
    return respond({ error: "留言格式錯誤" }, 400);
  }

  // Signed in? Then the note carries a name, and the cookie needs a CSRF token.
  const session = await resolveSession(request, env);
  if (session && !csrfTokenMatches(request, session)) return respond({ error: "請重新整理頁面後再送出" }, 403);
  const author = session ? { id: session.user.id, displayName: session.user.displayName } : null;

  const networkKey = await networkKeyFor(request, "civic-law-lab");
  const result = await room.addMessage(candidate.text, candidate.clientId, candidate.messageId, networkKey, author);
  if (result.error === "RATE_LIMIT") return respond({ error: "送出太頻繁，請稍後再試" }, 429);
  if (result.error) return respond({ error: "留言格式錯誤或超過 500 字" }, 400);
  return respond({ message: result.message }, 201);
}
