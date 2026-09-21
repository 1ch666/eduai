using System;
using UnityEngine;
using UnityEngine.UI;

namespace EduAI.Court
{
    public sealed class ChoiceSystem : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text options;
        private Action<int> onSelected;
        public bool IsOpen { get; private set; }
        public event Action<int> Selected;
        public void Configure(GameObject choicePanel, Text choiceText)
        { panel = choicePanel; options = choiceText; Close(); }
        public void ShowChoices(string option1, string option2, string option3, string option4,
            Action<int> callback = null)
        {
            if (!panel || !options) { Debug.LogError("Choice UI is not configured.", this); return; }
            options.text = $"[1] {option1}\n[2] {option2}\n[3] {option3}\n[4] {option4}";
            onSelected = callback; IsOpen = true; panel.SetActive(true);
        }
        public void Choose(int index)
        {
            if (!IsOpen || index < 0 || index > 3) return;
            Action<int> callback = onSelected;
            Close(); callback?.Invoke(index); Selected?.Invoke(index);
        }
        public void Close() { IsOpen = false; onSelected = null; if (panel) panel.SetActive(false); }
        private void Update()
        {
            if (!IsOpen || Cursor.lockState != CursorLockMode.Locked) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) Choose(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) Choose(3);
        }
    }
}
