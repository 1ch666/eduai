# EduAI Court — 簡單版交接

獨立 Unity 6.3 LTS 原型，不修改 eduai 網站或既有 AI API。使用 Primitive 和 Unity UI，不依賴付費資產。

## 本次範圍

第一人稱 WASD、滑鼠轉向、跳躍、碰撞、中央準星、3 公尺 Raycast、E 互動、開庭按鈕、五位 NPC、證物 A 與鍵盤 1–4 選項。場景由 `Assets/Editor/CourtProjectBuilder.cs` 建立，互動邏輯分檔。無武器、無正式案件審理、無 AI、無 session 實作。

## Unity

版本：6000.3.24f1（Unity 6.3 LTS）。需要 Windows／Web Build Support；使用免費授權前由使用者確認資格並在 Hub 登入啟用。不得把授權檔或金鑰加入 Git。

1. 用 Hub 開啟此資料夾，等待套件匯入。
2. 執行 `EduAI > Create Courtroom Prototype`，產生 `Assets/Scenes/Courtroom.unity`。為避免覆寫場景，有同名檔案時會停止。
3. 打開 Courtroom 並 Play；依下列清單實測。
4. 可透過 batch 呼叫 `EduAI.Court.Editor.CourtProjectBuilder.ValidateScene` 與 `EduAI.Court.Editor.CourtProjectBuilder.BuildWebGL`。

WebGL 以未壓縮模式輸出 `Builds/WebGL`，便於一般靜態伺服器正確提供 WASM，不依賴特定 CDN 壓縮標頭。瀏覽器可能需要第一次點擊才鎖定滑鼠；Esc 解鎖後可再次點擊恢復。這是鍵鼠原型，不宣稱支援手機觸控。

## Docker

**最新變更：使用者已同意先上線空白頁。** Docker 現在預設提供倉庫 `play/` 的相同空白入口，不代表 Unity 已編譯。從 `court-game/` 執行 `docker compose up --build -d`；從倉庫根目錄執行 `docker build -f court-game/Dockerfile -t eduai-court:prototype .`。本機尚無 Docker，但 GitHub Actions 已實際 build/run 通過，結果與下載位置見 STATUS.md。

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

開啟 http://localhost:8080/ 。這個映像只有已部署的空白入口，不包含可玩的 Unity 場景。

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
- 開庭／法官顯示四選項；1–4 只在選项開啟時作用，選後關閉；其他 NPC 及證物顯示假資料。
- WebGL 編譯並以 HTTP 開啟測試；Docker build、healthz、WASM MIME type 與遊玩測試。

這份清單是驗收要求，不代表上述測試已完成；實際執行狀態另見 STATUS.md。
