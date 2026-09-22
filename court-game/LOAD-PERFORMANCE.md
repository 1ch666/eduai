# WebGL 載入優化 — 2026-09-22

## 可重現的大小比較

基準：部署 commit `bdcdf7f` 的 play/Build；優化後：Unity 6000.3.24f1 正式 Release 成品。
單位 bytes，MB 使用十進位。優化後下載的是 Gzip `.unityweb`，loader 會解壓。

| 資源 | 優化前未壓縮檔 | 優化後下載檔 | 優化後解壓 |
| --- | ---: | ---: | ---: |
| .data | 18,702,452 | 1,252,702 | 4,027,543 |
| .wasm | 16,189,274 | 4,842,544 | 15,727,412 |
| framework.js | 403,992 | 84,487 | 403,034 |
| loader.js | 26,983 | 47,867 | 47,867 |
| 遊戲資源總計 | **35,322,701** | **6,227,600** | **20,205,856** |

遊戲檔案下載負載減少 **82.37%**；另有 HTML 6,133 bytes，合計 **6,233,733 bytes**。
此為完整檔案大小，不是冷快取網路 trace：未包含 HTTP/TLS 標頭，也未扣除瀏覽器快取或 CDN 額外傳輸壓縮。不能把 82.37% 直接當成啟動時間降幅；WASM 編譯和 Unity 初始化仍需 CPU 時間。

檢查指令（倉庫根目錄）：

```sh
node --test court-game/tools/test-template.mjs
node court-game/tools/check-build.mjs play
```

`check-build.mjs` 驗證 HTML 指向的資源、Gzip 完整性及解壓後 WASM magic；加 `--brotli` 可做離線壓縮估算，但不是 Unity Brotli build 或網路實測。

## 壓縮選擇的證據

先用微型 `encoding-probe.js.br` 在同一 GitHub Pages 網站測試（commit `34ba9b5`）。2026-09-21 23:18 UTC，無 Accept-Encoding 及帶 `Accept-Encoding: br` 兩種 HEAD 請求皆 HTTP 200、Content-Length 43、Content-Type application/octet-stream，**沒有 Content-Encoding: br**。測試檔在正式發布時移除。

因此不用依賴原生 Brotli 解碼；依使用者要求採 Gzip + Decompression Fallback。Unity 的說明：
https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/webgl/building-distribution/deploying

不需要更換付費主機、不改 GitHub Pages 標頭；本機沒有壓縮標頭的預覽服務也能成功啟動。

## 具體變更

- 進頁立刻 append loader 並 createUnityInstance 一次；不用先按載入。
- 保留封面、真實進度 0–99%，instance 完全初始化後才顯示 100%／開始遊戲。
- 開始按鈕只隱藏 cover、啟用現有 Player；不重新建立或下載 Unity。
- 未開始前 Player 不吃遊戲輸入；開始後重新顯示指引。
- Release／BuildOptions.None，關閉 Development、Script Debugging、Profiler、Deep Profiling、WebGL debug symbols。
- Strip Engine Code、Managed Stripping Medium。暫不採 High：目前收益主要來自資源和傳輸壓縮，不為微小收益冒進階 stripping 相容性風險。
- 字型 16,435,884 → 218,004 bytes；原字型移至 tools/fonts 保留 OFL 與來源，遊戲只帶靜態子集。
- 移除未使用 Visual Studio IDE、UnityWebRequest 套件宣告；其 test-framework／NUnit／jsonserialize 間接依賴隨之移除。自訂真 Unity 測試不依賴 NUnit。
- 移除沒有 SpriteRenderer／材質使用的 Always Included `Sprites/Default`；保留 Standard、UI 和字型 shader。沒有任意清空所有 built-in shaders。
- 修正 fallback 重新拖曳的游標起點，避免前次游標位置造成視角跳動。

## 測試邊界

- 模板 5 項測試、字集覆蓋、真正 Unity 場景／流程／Play、Release build 均通過。
- Play 啟動仍出現 UnityEditor.Search.SearchDatabase 內部 ArgumentOutOfRangeException；遊戲測試通過。這是既有 Editor 問題，不宣稱整個 Editor 零例外。
- Chrome DevTools MCP 未配置，因此依 web-perf 技能停止該 trace 稽核；沒有捏造 Lighthouse、Core Web Vitals 或冷啟動秒數。
- 奶蛙模型由使用者稍後提供；本次不含新角色。保留現有可完成／重玩的案件，未新增手機觸控。

Docker、線上及瀏覽器最後驗證紀錄見 STATUS.md。
