using UnityEngine;

namespace EduAI.Court
{
    public sealed class NPCInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "法官";
        [SerializeField] private bool isJudge;
        [SerializeField] private InteractionUI ui;
        [SerializeField] private CourtSession session;
        public void Configure(string npcName, bool judge, InteractionUI hud, CourtSession court)
        { displayName = npcName; isJudge = judge; ui = hud; session = court; }
        public string GetInteractionText() => "與" + displayName + "交談";
        public void Interact()
        {
            if (isJudge && session) session.BeginHearing();
            else if (ui) ui.ShowMessage(displayName + "：這是模擬法庭互動測試，尚未串接 AI。");
        }
    }
}
