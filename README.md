# eduai

公民法律研究室：結合模擬法庭、公民小學堂、筆記精華與 Ollama 公民知識問答的互動式學習網站。

- 後端、帳號、資料庫與 AI 串接：`BACKEND.md`
- Unity 3D 法庭遊戲：`court-game/START-HERE.md`（新手）、`court-game/STATUS.md`（實際驗證狀態）
- 搜尋與分享設定：`SEO.md`

常用指令：

```sh
npm install
npm run dev                  # 本機 Worker，預設 http://127.0.0.1:8787
npm run check                # 型別檢查與部署乾跑
node scripts/check-frontend.mjs
node scripts/check-api.mjs   # 需要先啟動 npm run dev
```
