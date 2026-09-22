# 字型來源與重建

`NotoSansCJKtc-Regular.otf` 是原始 Noto Sans CJK TC，來源：
https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/TraditionalChinese/NotoSansCJKtc-Regular.otf

原始 SHA256：`dce08bd4fd91aa8aa76ed8fea4b694c2dfb8550f67871e326843212ddbeb88b4`
SIL OFL 1.1，授權見同目錄 Noto-LICENSE.txt。保留完整來源在 Assets 外，不納入遊戲下載。

遊戲使用修改後的 **EduAI Court Subset** 字型，保留原本 Unity GUID。
在倉庫根目錄建立 Python venv、安裝免費 `fonttools==4.60.1` 後執行：

```powershell
python -m venv .court-tools
./.court-tools/Scripts/python.exe -m pip install fonttools==4.60.1
./.court-tools/Scripts/python.exe court-game/tools/subset-font.py
./.court-tools/Scripts/python.exe court-game/tools/subset-font.py --check
```

新增 C#／場景／Prefab 文字後必須重跑，否則新的中文字可能缺字。
未來接 AI 或任意使用者文字時不能只用這個靜態字集，應恢復完整字型或設計按語系載入。
