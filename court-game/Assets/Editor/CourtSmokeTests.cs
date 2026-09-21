using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EduAI.Court.Editor
{
    // 無付費或額外 test 套件；在真正 Unity Editor 裡執行。
    // 這些是場景及流程測試，不取代 Play 的碰撞與瀏覽器指標鎖定實測。
    public static class CourtSmokeTests
    {
        [MenuItem("EduAI/Run Court Smoke Tests")]
        public static void Run()
        {
            CourtProjectBuilder.ValidateScene();
            try
            {
                var choices = UnityEngine.Object.FindFirstObjectByType<ChoiceSystem>();
                var session = UnityEngine.Object.FindFirstObjectByType<CourtSession>();
                var evidence = UnityEngine.Object.FindFirstObjectByType<EvidenceInteractable>();
                var button = UnityEngine.Object.FindFirstObjectByType<CourtButton>();
                Require(choices && session && evidence && button, "Missing gameplay component");
                Require(button.OnPressed.GetPersistentEventCount() == 1, "Start button listener");
                int callbacks = 0;
                choices.ShowChoices("A", "B", "C", "D", index => { Require(index == 2, "Selected index"); callbacks++; });
                choices.Choose(-1); choices.Choose(4);
                Require(choices.IsOpen && callbacks == 0, "Invalid choice must be ignored");
                choices.Choose(2); choices.Choose(2);
                Require(!choices.IsOpen && callbacks == 1, "Selection closes and invokes once");

                session.BeginHearing();
                Require(choices.IsOpen, "Opening choices");
                choices.Choose(3);
                evidence.Interact();
                session.HearWitness();
                session.BeginHearing();
                choices.Choose(0);
                Require(Message().Contains("再想一想"), "Incorrect answer feedback");
                session.BeginHearing(); choices.Choose(2);
                Require(Message().Contains("本輪完成"), "Complete after correct answer");
                session.BeginHearing(); choices.Choose(0);
                Require(choices.IsOpen && Message().Contains("消失的平板"), "Restart resets progress");
                Debug.Log("COURT_SMOKE_TESTS_PASSED");
            }
            finally
            {
                // 測試變更只留在記憶體，不儲存到交付場景。
                EditorSceneManager.OpenScene("Assets/Scenes/Courtroom.unity");
            }
        }
        private static string Message()
        {
            var ui = UnityEngine.Object.FindFirstObjectByType<InteractionUI>();
            var reference = new SerializedObject(ui).FindProperty("message");
            return ((Text)reference.objectReferenceValue).text;
        }
        private static void Require(bool condition, string reason)
        { if (!condition) throw new InvalidOperationException("Court smoke test failed: " + reason); }
    }
}
