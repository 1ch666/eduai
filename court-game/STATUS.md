# 進度檢查點 — 2026-09-21

最新範圍：使用者要求繼續完成 3D 遊戲，真正遊戲部署後更新 Docker，並提供無經驗朋友可使用的 AI 接手文件。session 與 AI 仍留給後端。成功建置、實測後才替換 /play/ 空白入口。

## 本輪進度（原始碼已寫，Unity 尚未編譯）

- 虛構「消失的平板」：開庭、查看證物 A、詢問證人、回法官處作答、答錯提示、完成及重玩。
- 修正場景驗證誤報 UI 可選欄位；補齊 InputLegacy／TextRendering 模組；WebGL 點擊鎖定滑鼠，Esc 只解鎖。
- 加入 CourtSmokeTests：選項邊界、回呼一次、答錯／完成／重玩；尚未在 Unity 執行。
- WebGL 中文載入模板：進度、錯誤、返回學堂、鍵鼠限制。尚未部署。
- tools/build.ps1：建立缺少場景、流程測試、WebGL build；不覆寫已有場景。
- START-HERE.md 與「給朋友的AI.txt」：新手操作、檔案用途、部署後 Docker 更新、安全邊界。

已通過：模板 JavaScript 語法、manifest.json 解析、build.ps1 PowerShell 語法、git diff --check。這不等於 Unity 編譯或遊玩驗證。

## 最新環境更新：Editor 已安裝，需啟用 Unity 帳號授權

使用者回覆「允許」後，直接啟動已驗證的官方安裝檔，經正常 Windows UAC 安裝成功，結束碼 0。Unity CLI editors -i 已列出 6000.3.24f1，路徑為 `C:\Users\user\Documents\Codex\2026-09-01\new-chat\tools\UnityEditors\6000.3.24f1\Editor\Unity.exe`。

首次實際 batch 啟動專案結束碼 **198**，`Logs/create-scene.log` 明確顯示 `No valid Unity Editor license found. Please activate your license.` 尚未進入 C# 編譯，Courtroom.unity 未生成。需要使用者在 Unity Hub 登入自己的帳號、確認免費資格並啟用 Personal 授權；不要選付費方案或把帳密交給 AI。

Unity CLI 的模組清單為空，故改從官方版本頁提供的 Windows Web Build Support 下載連結下載相同版本模組；約 886.7 MiB，尚未下載／安裝完成。目標檔案在工作區 `tools/UnitySetup-WebGL-Support-for-Editor-6000.3.24f1.exe`。接手前先確認下載是否完成及數位簽章有效，不能執行不完整安裝檔。Editor 安裝成功不代表 WebGL 支援已就緒。

線上 /play/ 與 Docker 仍是原空白入口。啟用授權及裝好模組後才可繼續 build.ps1、實測、部署和重建 Docker。

## 先前安裝問題（已由直接執行官方安裝檔解決）

UnitySetup64-6000.3.24f1.exe 已下載（4,127,507,400 bytes）；數位簽章 Valid，Unity Technologies SF。首次 Program Files 安裝失敗；官方 --no-elevate 模式回 ELEVATION_REQUIRED。改用可寫入目錄仍回報：`The Windows elevation prompt was cancelled or timed out.` 未繞過授權或改安全設定。

下載檔：`C:\Users\user\AppData\Local\Packages\UnityTechnologies.UnityCLI_2vrhnee42bhxm\LocalCache\Roaming\UnityHub\downloads\UnitySetup64-6000.3.24f1.exe`。Unity CLI 安裝目錄已設為 `C:\Users\user\Documents\Codex\2026-09-01\new-chat\tools\UnityEditors`，此工具目錄不提交 Git。

需要使用者允許官方安裝程式的 Windows 管理員提示，安裝 Editor 6000.3.24f1 與 Web Build Support，在 Hub 登入並確認資格、啟用免費授權。license status 為 LICENSING_CLIENT_UNAVAILABLE，editors -i 仍無 Editor。

Courtroom.unity 尚未生成，無新 WebGL 成品、無新遊戲上線、無新版遊戲 Docker。安裝後繼續：build.ps1 → HANDOFF 實測 → 發布 play/ → 線上驗證 → 重建並測 Docker → 更新本檔。

## 先前已完成：空白入口與其容器

本次優先保留並推送簡單版原始碼，避免進度遺失。這不是已完成遊戲。

已建立：分檔 C# 第一人稱控制、Raycast／E 互動、提示 UI、NPC、證物、開庭按鈕與高亮、1–4 選項，以及 Editor 場景產生器；Dockerfile、Compose、Nginx 設定與後端 session 交接說明。

已查證：Git 和 winget 可用；Visual Studio 2022 Build Tools 已有；Unity Hub 3.21.3 已由 winget 安裝為 MSIX。未找到 Unity Editor、.NET SDK、Docker CLI。

已部署：https://1ch666.github.io/eduai/play/ 。主網站模擬法庭的「開始遊玩」已透過瀏覽器實際點擊，能開啟此空白入口，並明示「3D 場景製作中」。

Docker 已建置並測試成功：GitHub Actions https://github.com/1ch666/eduai/actions/runs/35588646331 ，來源 commit 09d6d65。驗證包含 Nginx 設定、HTTP 200／healthz、HTML 與 play/index.html 一致、/api/ 回 501、未知路徑 404、非 root 與唯讀檔案系統。輸出 artifact `eduai-court-docker` 內含 `eduai-court-preview.tar.gz` 及 SHA256SUMS；保存 1 天，可重新執行手動工作流程。這是空白入口的 Linux amd64 映像，不是 Unity 遊戲。

交由朋友後續完成／尚未驗證：Unity 編譯、Courtroom.unity 實際產生、Editor Play、WebGL build 與遊戲操作。不能直接拿 C# 啟動遊戲。

字型：已補入下載完成的 Assets/UI/NotoSansCJKtc-Regular.otf（16,435,884 bytes）與 Noto-LICENSE.txt。SHA256：dce08bd4fd91aa8aa76ed8fea4b694c2dfb8550f67871e326843212ddbeb88b4。來源為 notofonts/noto-cjk；尚需 Unity 匯入驗證。

接手：閱讀 AGENTS.md；CONTINUE.md 有可直接貼給 AI 的指令。後續以倉庫 court-game 為主要工作目錄，不要回頭拿外部 EduAI-Court 副本覆蓋較新的註解。

朋友接手下一步：安裝 Unity 6000.3.24f1 及 Web／Windows 支援，登入啟用符合資格的免費授權，開啟專案後執行 EduAI/Create Courtroom Prototype。完成 HANDOFF.md 遊戲驗收後，以真實 WebGL 取代空白入口並重建遊戲版 Docker image。Session、AI 串接留給後端人員。

無前端金鑰，無新付費服務；網站僅加入遊玩入口，未改動既有 API。使用者表示 GitHub 權限已自行交接，本次未新增其他平台共享權限。
