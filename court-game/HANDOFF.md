# EduAI Court — 簡單版交接

獨立 Unity 6.3 LTS 原型，不修改 eduai 網站或既有 AI API。使用 Primitive 和 Unity UI，不依賴付費資產。

## 本次範圍

第一人稱 WASD、滑鼠轉向、跳躍、碰撞、中央準星、3 公尺 Raycast、E 互動、開庭按鈕、五位 NPC、證物 A 與鍵盤 1–4 選項。新增虛構「消失的平板」調查、答錯提示、完成／重玩流程。場景由 `Assets/Editor/CourtProjectBuilder.cs` 建立，互動邏輯分檔。無武器、無正式案件審理、無 AI、無 session 實作。新手先看 START-HERE.md，實際驗證狀態見 STATUS.md。

## Unity

版本：6000.3.24f1（Unity 6.3 LTS）。需要 Windows／Web Build Support；使用免費授權前由使用者確認資格並在 Hub 登入啟用。不得把授權檔或金鑰加入 Git。

1. 用 Hub 開啟此資料夾，等待套件匯入。
2. 執行 `EduAI > Create Courtroom Prototype`，產生 `Assets/Scenes/Courtroom.unity`。為避免覆寫場景，有同名檔案時會停止。
3. 打開 Courtroom 並 Play；依下列清單實測。
4. 可透過 batch 呼叫 `EduAI.Court.Editor.CourtProjectBuilder.ValidateScene` 與 `EduAI.Court.Editor.CourtProjectBuilder.BuildWebGL`。

WebGL 使用 **Release + Gzip + Decompression Fallback** 輸出 `Builds/WebGL`。`.unityweb` 由 Unity loader 解壓，不依賴伺服器 Content-Encoding；不要直接改副檔名或替壓縮檔設定 application/wasm。頁面進入後立即初始化一次、顯示進度，100% 才啟用「開始遊戲」，點擊只移除封面。Esc 解鎖後可再次點擊恢復；不支援 Pointer Lock 時按住左鍵拖曳。這是鍵鼠原型，不宣稱支援手機觸控。

`BuildWebGL` 強制關閉 Development、Script Debugging、Profiler／Deep Profiling、除錯符號，開啟 Strip Engine Code，Managed Stripping 使用 Medium。High 未啟用；未來增加反射／AI 套件後須重新測試。字型已裁成靜態文字子集，新增文字前閱讀 `tools/fonts/README.md` 並重建／檢查字集；原字型保留在 tools/fonts，不進入遊戲下載。

## Docker

**Docker 已改為預設提供倉庫 `play/` 的 WebGL 遊戲。** 每次遊戲部署後都必須重新執行 `Court Docker handoff`，確認產物與 GitHub Pages 使用相同 commit。從 `court-game/` 執行 `docker compose up --build -d`；從倉庫根目錄執行 `docker build -f court-game/Dockerfile -t eduai-court:prototype .`。工作流程會檢查首頁、WASM MIME、資料檔、healthz、API 501、非 root 與唯讀檔案系統。下載的映像要記錄來源 commit 和 SHA256；舊空白頁映像不可當作新版遊戲。

後續真實 WebGL 產出後，從倉庫根目錄改用：

```sh
docker build -f court-game/Dockerfile --build-arg WEB_ROOT=court-game/Builds/WebGL -t eduai-court:prototype .
docker save -o eduai-court-image.tar eduai-court:prototype
```

先完成 Unity WebGL build 才能交付可玩遊戲；空白入口不需要 Unity。倉庫另附手動執行的 `Court Docker handoff` 工作流程，僅在公開倉庫使用標準 runner 建置、檢查及匯出映像；下載檔保存 1 天，請及時保存。

下載 `eduai-court-preview.tar.gz` 後：

```sh
docker load -i eduai-court-preview.tar.gz
docker run --rm --read-only --tmpfs /tmp:size=32m,mode=1777 --cap-drop ALL --security-opt no-new-privileges:true -p 127.0.0.1:8080:8080 eduai-court:preview
```

開啟 http://localhost:8080/play/ （根路徑 / 也支援）。這個映像提供已提交的 WebGL 遊戲；它仍不包含後端 session 或 AI。Dockerfile 會拒絕缺少 loader 或 Gzip 資源損毀的半成品。`.unityweb` 保持原始壓縮位元組交給 loader，CI 逐檔比較容器與 play/ 是否一致。

Docker 提供靜態檔案，不能用來開啟 C# 原始碼遊玩。若缺少 index.html，建置會失敗。來源交接 zip 不是已建好的 Docker image；image tar 需完成 build/save 才會產生。替換為 Unity 輸出時，務必另外驗證 WASM 與實際遊玩。

服務預設只綁本機、非 root、唯讀檔案系統。後端人員自行在部署環境加 HTTPS 與網路入口；正式環境應固定 nginx image digest。無付費服務設定。

## 後端／session 邊界

- `ICourtDialogueProvider` 是未來回覆介面，原型目前不發送請求。
- 預留同源 `POST /api/ai/ask`；前端欄位、回覆格式與登入路由需由後端人員確認，不假設舊 API 支援 session。
- Nginx `/api/` 目前明確回 501；由後端人員改成自己的 upstream，不能把目前區塊當成已串接。
- Session ID 由伺服器產生、輪替與驗證，透過 HttpOnly、Secure、適當 SameSite Cookie 傳遞。狀態變更請求要驗證 Origin／CSRF；不把 session ID 放在 URL 或 localStorage。
- 後端負責登入、授權、CSRF、限流、輸入長度、AI 領域規則、逾時與錯誤處理。AI key 只能放伺服器秘密設定。
- GitHub Pages 無法執行 Docker 或後端；容器需另外的主機。不要為此自動開通付費資源。

## 待實測驗收

- Unity 匯入及 C# 編譯無錯誤；場景無 missing scripts／未接 references。
- Play：WASD、滑鼠、Space、撞牆／桌、Esc 解鎖／再捕捉。
- 準星固定中央；E 提示只在 3m 內、無牆壁遮擋且準星對準物件時出現；離開取消高亮。
- 開庭／法官顯示四選項；1–4 只在選项開啟時作用，選後關閉；其他 NPC 及證物顯示虛構教學資料。
- 查看證物並詢問證人後回法官處作答；答錯可再答，答對完成；重玩會清除本輪調查進度。
- WebGL 編譯並以 HTTP 開啟測試；Docker build、healthz、WASM MIME type 與遊玩測試。

這份清單是驗收要求，不代表上述測試已完成；實際執行狀態另見 STATUS.md。
