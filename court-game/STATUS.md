# 進度檢查點 — 2026-09-21

最新範圍：使用者同意先部署空白遊戲頁面（play/index.html），模擬法庭新增「開始遊玩」連結。3D 製作留給朋友，需求已寫入該 HTML 註解；以下 Unity 未完成事項是後續交接項目，不代表空白入口失敗。Unity CLI 另已成功安裝，但 Editor 與實際 WebGL 編譯仍未完成。

本次優先保留並推送簡單版原始碼，避免進度遺失。這不是已完成遊戲。

已建立：分檔 C# 第一人稱控制、Raycast／E 互動、提示 UI、NPC、證物、開庭按鈕與高亮、1–4 選項，以及 Editor 場景產生器；Dockerfile、Compose、Nginx 設定與後端 session 交接說明。

已查證：Git 和 winget 可用；Visual Studio 2022 Build Tools 已有；Unity Hub 3.21.3 已由 winget 安裝為 MSIX。未找到 Unity Editor、.NET SDK、Docker CLI。

尚未完成／未驗證：Unity 編譯、Courtroom.unity 實際產生、Editor Play、WebGL build、Docker build／容器遊玩。Docker 設定需先取得真實 WebGL 輸出，不能直接拿 C# 啟動遊戲。

字型：已補入下載完成的 Assets/UI/NotoSansCJKtc-Regular.otf（16,435,884 bytes）與 Noto-LICENSE.txt。SHA256：dce08bd4fd91aa8aa76ed8fea4b694c2dfb8550f67871e326843212ddbeb88b4。來源為 notofonts/noto-cjk；尚需 Unity 匯入驗證。

接手：閱讀 AGENTS.md；CONTINUE.md 有可直接貼給 AI 的指令。後續以倉庫 court-game 為主要工作目錄，不要回頭拿外部 EduAI-Court 副本覆蓋較新的註解。

下一步：安裝 Unity 6000.3.24f1 及 Web／Windows 支援，登入啟用符合資格的免費授權，開啟專案後執行 EduAI/Create Courtroom Prototype。完成 HANDOFF.md 驗收後才產生 WebGL 與 Docker image。Session、AI 串接留給後端人員。

無前端金鑰，無新付費服務，未更動既有網站或 API。
