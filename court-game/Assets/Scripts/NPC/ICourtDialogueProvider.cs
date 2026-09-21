using System;
using System.Collections;

namespace EduAI.Court
{
    // Future adapter: UnityWebRequest -> Worker POST /api/ai/ask.
    // No endpoint is called by this prototype. Never put an API key in a build.
    public interface ICourtDialogueProvider
    {
        IEnumerator Ask(string question, Action<string> onReply, Action<string> onError);
    }
}
