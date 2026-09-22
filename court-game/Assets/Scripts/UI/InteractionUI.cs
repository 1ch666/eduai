using UnityEngine;
using UnityEngine.UI;

namespace EduAI.Court
{
    public sealed class InteractionUI : MonoBehaviour
    {
        [SerializeField] private Text interactionPrompt;
        [SerializeField] private Text message;
        private float messageUntil;
        public void Configure(Text prompt, Text messageText)
        { interactionPrompt = prompt; message = messageText; SetPrompt(""); }
        public void SetPrompt(string text)
        {
            if (!interactionPrompt) return;
            interactionPrompt.text = text;
            interactionPrompt.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
        public void ShowMessage(string text, float seconds = 7)
        {
            if (!message) return;
            if (FirstPersonController.TouchEnabled)
            {
                message.gameObject.SetActive(false);
                TouchWebBridge.Message(text.Replace("對準後按 E", "直接輕點").Replace("對準「開庭」按 E", "輕點「開庭」"), seconds);
                return;
            }
            message.text = text; message.gameObject.SetActive(true);
            messageUntil = Time.unscaledTime + seconds;
        }
        private void Update()
        {
            if (message && message.gameObject.activeSelf && Time.unscaledTime > messageUntil)
                message.gameObject.SetActive(false);
        }
    }
}
