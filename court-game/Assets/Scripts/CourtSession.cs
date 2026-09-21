using UnityEngine;

namespace EduAI.Court
{
    public sealed class CourtSession : MonoBehaviour
    {
        // 接手入口：这里只管理法庭流程；移動、射線、UI 與未來後端請維持分層。
        // TODO(後端)：案件進度由伺服器驗證，不能把前端選項索引當成可信結果。
        [SerializeField] private InteractionUI ui;
        [SerializeField] private ChoiceSystem choices;
        public void Configure(InteractionUI hud, ChoiceSystem choiceSystem)
        { ui = hud; choices = choiceSystem; }
        public void BeginHearing()
        {
            if (!ui || !choices) return;
            ui.ShowMessage("法官：現在開始模擬法庭程序。");
            choices.ShowChoices("提出證物", "詢問證人", "查看案件", "結束發言", HandleChoice);
        }
        private void HandleChoice(int index)
        {
            // 目前為假資料回饋。後端接入時，請由此呼叫獨立服務，不在 Update 發 HTTP。
            string[] results = { "提出證物：請先查看桌上的證物 A。", "詢問證人：請走近證人席並按 E。",
                "查看案件：本案為操作測試，不代表真實法律判決。", "法官：本次發言結束。" };
            ui.ShowMessage(results[index]); Debug.Log(results[index]);
        }
    }
}
