// Harmless temporary Pages header probe, not referenced by the game.
// Remove play/encoding-probe.js.br after recording Content-Encoding response.
import { brotliCompressSync } from 'node:zlib';
import { writeFile } from 'node:fs/promises';
await writeFile(new URL('../../play/encoding-probe.js.br', import.meta.url), brotliCompressSync(Buffer.from('/* EduAI native Brotli header probe */\n')));
