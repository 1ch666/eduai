# 進度檢查點 — 2026-09-21

本次優先保留並推送簡單版原始碼，避免進度遺失。這不是已完成遊戲。

已建立：分檔 C# 第一人稱控制、Raycast／E 互動、提示 UI、NPC、證物、開庭按鈕與高亮、1–4 選項，以及 Editor 場景產生器；Dockerfile、Compose、Nginx 設定與後端 session 交接說明。

已查證：Git 和 winget 可用；Visual Studio 2022 Build Tools 已有；Unity Hub 3.21.3 已由 winget 安裝為 MSIX。未找到 Unity Editor、.NET SDK、Docker CLI。

尚未完成／未驗證：Unity 編譯、Courtroom.unity 實際產生、Editor Play、WebGL build、Docker build／容器遊玩。Docker 設定需先取得真實 WebGL 輸出，不能直接拿 C# 啟動遊戲。

字型：場景產生器需要 Assets/UI/NotoSansCJKtc-Regular.otf。若此檢查點尚未包含，請下載 https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/OTF/TraditionalChinese/NotoSansCJKtc-Regular.otf 並同時保留 https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/LICENSE 。不要將下載中的不完整字型拿來匯入。

下一步：安裝 Unity 6000.3.24f1 及 Web／Windows 支援，登入啟用符合資格的免費授權，開啟專案後執行 EduAI/Create Courtroom Prototype。完成 HANDOFF.md 驗收後才產生 WebGL 與 Docker image。Session、AI 串接留給後端人員。

無前端金鑰，無新付費服務，未更動既有網站或 API。
