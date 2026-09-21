using System;
using System.Collections;

namespace EduAI.Court
{
    // Future adapter: UnityWebRequest -> Worker POST /api/ai/ask.
    // No endpoint is called by this prototype. Never put an API key in a build.
    public interface ICourtDialogueProvider
    {
        // TODO(後端)：實作 UnityWebRequest 的非阻塞請求，加入逾時與錯誤回饋。
        // /api/ai/ask 的格式需先與後端確認；session 驗證與 AI key 都留在伺服器。
        // WebGL 的 HttpOnly cookie 由瀏覽器管理，切勿讓腳本讀出或寫進 URL。
        IEnumerator Ask(string question, Action<string> onReply, Action<string> onError);
    }
}
