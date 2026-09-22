using System.Runtime.InteropServices;

namespace EduAI.Court
{
    // Local presentation only: no network, session, permissions or third-party SDK.
    public static class TouchWebBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void CourtTouchChoices(string a, string b, string c, string d);
        [DllImport("__Internal")] private static extern void CourtTouchClose();
        [DllImport("__Internal")] private static extern void CourtTouchMessage(string text, float seconds);
#endif
        public static void ShowChoices(string a, string b, string c, string d)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CourtTouchChoices(a, b, c, d);
#endif
        }
        public static void HideChoices()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CourtTouchClose();
#endif
        }
        public static void Message(string text, float seconds)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            CourtTouchMessage(text, seconds);
#endif
        }
    }
}
