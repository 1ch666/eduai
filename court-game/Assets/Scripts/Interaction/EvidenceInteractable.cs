using UnityEngine;

namespace EduAI.Court
{
    public sealed class EvidenceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractionUI ui;
        public void Configure(InteractionUI hud) { ui = hud; }
        public string GetInteractionText() => "查看證物";
        public void Interact()
        { if (ui) ui.ShowMessage("證物 A\n監視器截圖\n取得時間：2026/09/21\n（原型假資料）"); }
    }
}
