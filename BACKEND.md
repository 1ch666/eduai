# 後端與資料庫（MVP）

這份文件接續 `court-game/HANDOFF.md` 的「後端／session 邊界」。那份文件把登入、session、資料庫與 AI 留給後端；這裡記錄目前實際做到哪裡、怎麼設定，以及哪些還沒驗證。

全部使用既有的免費服務：Cloudflare Workers + Durable Objects（SQLite），沒有新增付費服務、沒有新的第三方平台，也沒有把任何金鑰寫進程式或 Git。

## 這次新增的功能

| 功能 | 端點 | 狀態 |
| --- | --- | --- |
| 註冊 / 登入 / 登出 | `POST /api/auth/register`、`/login`、`/logout` | 本機驗證通過 |
| 查詢登入狀態與 CSRF token | `GET /api/auth/session` | 本機驗證通過 |
| 學習進度雲端同步 | `GET`／`PUT /api/progress` | 本機驗證通過 |
| 共用留言標示作者 | `GET`／`POST /api/messages` | 本機驗證通過 |
| 公民助教問答（原有） | `POST /api/ai/ask`（`mode: "civics"`） | 介面未變 |
| 法庭 NPC 對話（新增） | `POST /api/ai/ask`（`mode: "court"`） | 需要 `OLLAMA_API_KEY` 才能實測 |

前端 `index.html` 右上角新增「登入 / 註冊」按鈕與表單，登入後公民小學堂與生活法律問答的作答紀錄會自動上傳。

## 資料庫

兩個 Durable Object，各自內建 SQLite：

- `MESSAGE_ROOM`（既有）：`messages`、`rate_limits`。本次新增 `messages.author_id`、`messages.author_name` 兩欄，啟動時以 `PRAGMA table_info` 檢查後才 `ALTER TABLE`，不會動到既有留言。
- `ACCOUNT_STORE`（新增，migration `v2`）：
  - `users`：`id`、`username`（唯一）、`display_name`、`password_hash`、`password_salt`、`password_iterations`、`created_at`、`last_login_at`
  - `sessions`：`token_hash`（主鍵）、`user_id`、`csrf_token`、`created_at`、`expires_at`、`last_seen_at`
  - `progress`：`user_id` + `scope` 為主鍵，`payload` 存 JSON 字串，`self_reported` 固定為 1
  - `auth_limits`：登入與註冊的五分鐘限流計數

沒有存 email、電話或真實姓名。顯示名稱由使用者自己填，會出現在公開留言區。

## 安全設計

- 密碼：PBKDF2-SHA256，每人獨立 16 bytes salt，預設 100,000 次疊代。每位使用者的疊代次數存在自己的資料列，之後調整設定不會讓舊帳號失效。
- Session：32 bytes 亂數，資料庫只存 SHA-256 雜湊，所以即使匯出資料庫也無法重放。Cookie 為 `HttpOnly`、`Path=/`，HTTPS 時再加上 `Secure`、`SameSite=None`、`Partitioned`，名稱使用 `__Host-` 前綴。瀏覽器端的 JavaScript 讀不到 token，也沒有寫進 localStorage。
- 閒置 14 天（可調，最多 30 天）自動失效，絕對上限 60 天；每個帳號最多保留 10 個有效 session。
- CSRF：所有會改資料的請求都要帶 `X-CSRF-Token`，值來自 `GET /api/auth/session`，只有信任來源讀得到。另外仍保留原本的 Origin 白名單檢查。
- 限流：登入每帳號 10 次／5 分鐘、每網段 30 次／5 分鐘；註冊每網段 5 次／5 分鐘；AI 提問維持原本的每裝置與每網段限制，登入者另有每人 8 次／分鐘的額度。IP 只以加鹽雜湊後的前 32 字元當作分桶依據，不會存原始位址。
- 查無帳號時仍會做一次 PBKDF2，避免用回應時間推測帳號是否存在。
- **前端送上來的進度一律是自述資料**：`/api/progress` 存進去時固定標記 `self_reported`，回傳時也帶 `selfReported: true`。它是個人練習紀錄，不能當成成績或權限依據。

## 設定步驟

需要你本人操作的只有 Cloudflare 登入與金鑰設定；AI 不會、也不應該代為輸入帳密。

```sh
npm install
npx wrangler login                    # 用你自己的 Cloudflare 帳號
npx wrangler secret put OLLAMA_API_KEY # 只存在伺服器端
npm run deploy                        # 第一次部署會套用 migration v2 建立 AccountStore
```

可選的 `vars`（寫在 `wrangler.jsonc`，不是秘密）：

- `PASSWORD_ITERATIONS`：預設 100000，允許 20000–400000。
- `SESSION_TTL_DAYS`：預設 14，最多 30。

本機開發與測試：

```sh
npm run dev                  # wrangler dev，預設 http://127.0.0.1:8787
node scripts/check-api.mjs   # 對著上面那個網址跑 13 項 API 檢查
```

## 已驗證

在這台機器上實際執行並通過：

- `npx tsc --noEmit`：無錯誤。
- `npm run build:assets` 與 `node scripts/check-frontend.mjs`：通過。
- `npx wrangler deploy --dry-run`：打包成功，列出 `MESSAGE_ROOM`、`ACCOUNT_STORE`、`ASSETS` 三個綁定。
- `npx wrangler dev` + `node scripts/check-api.mjs`：13 項通過，包含註冊格式檢查、重複帳號 409、cookie 解析、缺 CSRF 被擋 403、進度存取與重新登入後仍在、未登入讀進度 401、外部來源寫入 403、登出後舊 cookie 失效、密碼錯誤 401。

## 尚未驗證 / 尚未完成

- **還沒部署到正式環境**，線上 `https://civic-law-lab-212.yichengc869.workers.dev` 仍是舊版本；要等你執行 `npm run deploy` 才會有登入功能。
- 沒有 `OLLAMA_API_KEY` 就無法實測 `/api/ai/ask`；法庭模式的提示詞只做過格式驗證，沒有實際向 Ollama 送出過。
- `court-game/Assets/Scripts/NPC/WorkerDialogueProvider.cs` 是 `ICourtDialogueProvider` 的實作，**沒有接進 Courtroom.unity，也還沒用 Unity Editor 編譯過**。要在場景使用必須重新 build WebGL 並重測，`play/` 目前的成品不含這個檔案。
- 沒有在真實手機或多種瀏覽器上測過登入流程。
- Workers 免費方案每次呼叫的 CPU 限制較嚴，PBKDF2 100,000 次在本機約 30–40 毫秒。若部署後看到 CPU 超限錯誤，把 `PASSWORD_ITERATIONS` 調低（例如 50000）再部署即可，已註冊的帳號不受影響。這點還沒在正式環境實測。

## 已知限制

- **第三方 Cookie**：GitHub Pages 的頁面要跨站呼叫 Worker，session cookie 用 `SameSite=None; Partitioned`（CHIPS）。完全封鎖第三方 cookie 的瀏覽器設定下會登入失敗；此時請改用 Worker 自己的網址開啟網站（同源，不受影響）。
- 帳號沒有 email，也就沒有「忘記密碼」。密碼遺失只能重新註冊一個帳號。
- 所有帳號資料集中在一個 Durable Object，寫入會排隊。對一個班級的使用量沒問題，要擴大再分片或改用 D1。
- 沒有教師端、沒有班級、沒有成績匯出，也沒有管理後台。
- 留言區與進度都沒有內容審核；留言仍請勿填個資。
- Docker 版（`court-game/docker/nginx.conf`）的 `/api/` 仍刻意回 501。要讓容器版也能登入，需要由部署者把該區塊改成指向 Worker 的同源反向代理。
