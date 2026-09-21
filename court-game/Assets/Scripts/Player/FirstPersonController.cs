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
        public bool IsCaptured => Cursor.lockState == CursorLockMode.Locked;

        public void Configure(Camera camera) { viewCamera = camera; }
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (!viewCamera) viewCamera = GetComponentInChildren<Camera>();
        }
        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Capture(false); // 網頁必須由玩家點擊後要求鎖定指標。
#else
            Capture(true);
#endif
        }
        public void Capture(bool capture)
        {
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
        }
        private void OnApplicationFocus(bool focused) { if (!focused) Capture(false); }
        private void OnDisable() { Capture(false); }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { Capture(false); return; }
            // Browsers require a fresh user gesture to recapture the pointer.
            if (!IsCaptured)
            {
                if (Input.GetMouseButtonDown(0)) Capture(true);
                return;
            }
            if (!viewCamera || !controller) return;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, -85f, 85f);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.Rotate(0, Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0);
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
