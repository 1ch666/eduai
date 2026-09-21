# 進度檢查點 — 2026-09-21

最新範圍：使用者同意先部署空白遊戲頁面（play/index.html），模擬法庭新增「開始遊玩」連結。3D 製作留給朋友，需求已寫入該 HTML 註解；以下 Unity 未完成事項是後續交接項目，不代表空白入口失敗。Unity CLI 另已成功安裝，但 Editor 與實際 WebGL 編譯仍未完成。

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
