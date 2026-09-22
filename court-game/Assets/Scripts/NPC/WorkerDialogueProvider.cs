using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace EduAI.Court
{
    /// <summary>
    /// Talks to the Cloudflare Worker at POST /api/ai/ask with mode "court".
    /// The Worker holds the Ollama key; nothing secret is shipped in the build.
    /// Not wired into Courtroom.unity: drop it on an NPC and call Ask when the
    /// scene is ready for live dialogue, then rebuild and retest WebGL.
    /// </summary>
    public sealed class WorkerDialogueProvider : MonoBehaviour, ICourtDialogueProvider
    {
        [Tooltip("Same-origin /api/ai/ask when the game is served by the Worker; otherwise the full Worker URL.")]
        [SerializeField] private string endpoint = "https://civic-law-lab-212.yichengc869.workers.dev/api/ai/ask";
        [SerializeField] private string npcName = "書記官";
        [SerializeField] private string npcRole = "虛構教學法庭的書記官";
        [SerializeField] private string caseTitle = "消失的平板（虛構練習案件）";
        [SerializeField] private int timeoutSeconds = 30;

        private const string ClientIdKey = "eduai.court.clientId";

        [Serializable]
        private sealed class NpcPayload
        {
            public string name;
            public string role;
        }

        [Serializable]
        private sealed class AskPayload
        {
            public string question;
            public string clientId;
            public string requestId;
            public string mode;
            public string caseTitle;
            public NpcPayload npc;
        }

        [Serializable]
        private sealed class AskResponse
        {
            public string answer;
            public string error;
        }

        /// <summary>Stable per-device id for the Worker rate limiter. Not a login.</summary>
        private static string ClientId()
        {
            string stored = PlayerPrefs.GetString(ClientIdKey, string.Empty);
            if (string.IsNullOrEmpty(stored))
            {
                stored = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(ClientIdKey, stored);
                PlayerPrefs.Save();
            }
            return stored;
        }

        public IEnumerator Ask(string question, Action<string> onReply, Action<string> onError)
        {
            string trimmed = string.IsNullOrEmpty(question) ? string.Empty : question.Trim();
            if (trimmed.Length == 0 || trimmed.Length > 400)
            {
                if (onError != null) onError("問題需為 1 至 400 字。");
                yield break;
            }

            AskPayload payload = new AskPayload
            {
                question = trimmed,
                clientId = ClientId(),
                requestId = Guid.NewGuid().ToString(),
                mode = "court",
                caseTitle = caseTitle,
                npc = new NpcPayload { name = npcName, role = npcRole }
            };

            byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = Mathf.Clamp(timeoutSeconds, 5, 60);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    string detail = ReadError(request.downloadHandler != null ? request.downloadHandler.text : null);
                    if (onError != null) onError(detail ?? "目前無法連線到助教服務，請稍後再試。");
                    yield break;
                }

                AskResponse parsed = null;
                try
                {
                    parsed = JsonUtility.FromJson<AskResponse>(request.downloadHandler.text);
                }
                catch (ArgumentException)
                {
                    parsed = null;
                }

                if (parsed == null || string.IsNullOrEmpty(parsed.answer))
                {
                    if (onError != null) onError(parsed != null && !string.IsNullOrEmpty(parsed.error) ? parsed.error : "助教沒有傳回內容。");
                    yield break;
                }
                if (onReply != null) onReply(parsed.answer);
            }
        }

        /// <summary>Surfaces the Worker's own Chinese error text when it sent one.</summary>
        private static string ReadError(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) return null;
            try
            {
                AskResponse parsed = JsonUtility.FromJson<AskResponse>(responseText);
                return parsed != null && !string.IsNullOrEmpty(parsed.error) ? parsed.error : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
