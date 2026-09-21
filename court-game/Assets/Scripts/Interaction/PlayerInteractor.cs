using UnityEngine;

namespace EduAI.Court
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private InteractionUI ui;
        [SerializeField] private ChoiceSystem choices;
        [SerializeField] private float distance = 3f;
        [SerializeField] private LayerMask layers = ~0;
        private IInteractable target;
        private MonoBehaviour targetComponent;
        public void Configure(Camera camera, InteractionUI hud, ChoiceSystem choiceSystem)
        { viewCamera = camera; ui = hud; choices = choiceSystem; }
        private void Update()
        {
            if (!viewCamera || !ui) return;
            IInteractable next = null;
            MonoBehaviour component = null;
            if (Cursor.lockState == CursorLockMode.Locked && (!choices || !choices.IsOpen))
            {
                Ray ray = viewCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
                if (Physics.Raycast(ray, out RaycastHit hit, distance, layers, QueryTriggerInteraction.Ignore))
                {
                    foreach (MonoBehaviour candidate in hit.collider.GetComponentsInParent<MonoBehaviour>())
                        if (candidate is IInteractable interactable && candidate.isActiveAndEnabled)
                        { next = interactable; component = candidate; break; }
                }
            }
            if (!ReferenceEquals(next, target))
            {
                ClearTarget(); target = next; targetComponent = component;
                if (target is IFocusFeedback focus) focus.SetFocused(true);
            }
            ui.SetPrompt(targetComponent ? "[E] " + target.GetInteractionText() : "");
            if (targetComponent && Input.GetKeyDown(KeyCode.E)) target.Interact();
        }
        private void ClearTarget()
        {
            if (targetComponent && target is IFocusFeedback focus) focus.SetFocused(false);
            target = null; targetComponent = null;
            if (ui) ui.SetPrompt("");
        }
        private void OnDisable() { ClearTarget(); }
    }
}
