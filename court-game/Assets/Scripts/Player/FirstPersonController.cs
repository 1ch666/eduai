using UnityEngine;

namespace EduAI.Court
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -20f;
        private CharacterController controller;
        private float verticalSpeed;
        private float pitch;
        private bool dragLook;
        private bool dragLookActive;
        private bool wasDragging;
        private Vector3 previousDragPosition;
        private bool entered;
        public static FirstPersonController Active { get; private set; }
        public static bool InputActive => Active && Active.entered && Active.IsCaptured;
        public bool IsCaptured => dragLook ? dragLookActive : Cursor.lockState == CursorLockMode.Locked;

        public void Configure(Camera camera) { viewCamera = camera; }
        private void Awake()
        {
            Active = this;
            controller = GetComponent<CharacterController>();
            if (!viewCamera) viewCamera = GetComponentInChildren<Camera>();
        }
        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Capture(false); // 網頁必須由玩家點擊後要求鎖定指標。
#else
            entered = true;
            Capture(true);
#endif
        }
        // Called once by the Web cover after Unity initialization completes.
        // Background preload must not consume keyboard/mouse input or start the case.
        public void BeginGame()
        {
            if (entered) return;
            entered = true;
            Input.ResetInputAxes();
            if (dragLook) Capture(true);
            var hud = FindFirstObjectByType<InteractionUI>();
            if (hud) hud.ShowMessage("走到前方法官桌，對準「開庭」按 E。", 15);
        }
        public void Capture(bool capture)
        {
            dragLookActive = capture;
            Cursor.lockState = capture && !dragLook ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = dragLook || !capture;
        }
        // Web 模板只在瀏覽器拒絕 Pointer Lock 時呼叫；不繞過瀏覽器權限。
        public void EnableDragLook()
        {
            dragLook = true;
            Capture(entered);
            Input.ResetInputAxes();
            var hud = FindFirstObjectByType<InteractionUI>();
            if (hud) hud.ShowMessage("相容模式：按住滑鼠左鍵拖曳視角。\nWASD 移動、E 互動、1–4 作答不變。Esc 暫停。", 15);
        }
        private void OnApplicationFocus(bool focused) { if (!focused) Capture(false); }
        private void OnDisable() { Capture(false); if (Active == this) Active = null; }
        private void Update()
        {
            if (!entered) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { Capture(false); return; }
            // Browsers require a fresh user gesture to recapture the pointer.
            if (!IsCaptured)
            {
                if (Input.GetMouseButtonDown(0)) Capture(true);
                return;
            }
            if (!viewCamera || !controller) return;
            Vector2 look = Vector2.zero;
            if (!dragLook) look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            else
            {
                bool dragging = Input.GetMouseButton(0);
                // Reset the origin on each drag: moving the cursor before a
                // click must not cause a sudden camera jump in fallback mode.
                if (dragging && wasDragging) look = (Vector2)(Input.mousePosition - previousDragPosition) * .1f;
                previousDragPosition = Input.mousePosition;
                wasDragging = dragging;
            }
            if (look != Vector2.zero)
            {
                pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, -85f, 85f);
                viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
                transform.Rotate(0, look.x * mouseSensitivity, 0);
            }
            Vector2 input = Vector2.ClampMagnitude(new Vector2(
                (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)), 1);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * moveSpeed;
            velocity.y = verticalSpeed;
            CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0) verticalSpeed = 0;
        }
    }
}
