using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.UI;

namespace AREscapeRoom.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.0f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float upDownLookLimit = 80.0f;

        private CharacterController characterController;
        private Vector3 velocity;
        private float verticalRotation;
        private bool isControlEnabled = true;
        private bool skipLookDelta;

        public bool IsControlEnabled => isControlEnabled;

        public void AssignLookCamera(Transform cam)
        {
            cameraTransform = cam;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraTransform = cam.transform;
            }

            if (cameraTransform != null)
            {
                float pitch = cameraTransform.localEulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                verticalRotation = Mathf.Clamp(pitch, -upDownLookLimit, upDownLookLimit);
            }

            LockCursor();
        }

        private void Update()
        {
            // ESCAPE MUST BE CHECKED FIRST
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursor();
                return;
            }

            if (!isControlEnabled)
                return;

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameplayActive)
                return;

            // Don't move/look while keypad is open
            if (KeypadUI.AnyVisible)
                return;

            // If mouse is unlocked, click inside game window to lock cursor
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    LockCursor();
                }
                return;
            }

            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            // Discard delta spikes when locking or regaining focus
            if (skipLookDelta)
            {
                skipLookDelta = false;
                return;
            }

            float mouseX =
                Input.GetAxis("Mouse X") *
                mouseSensitivity *
                100f *
                Time.deltaTime;

            float mouseY =
                Input.GetAxis("Mouse Y") *
                mouseSensitivity *
                100f *
                Time.deltaTime;

            // Protect against sudden large mouse jumps
            if (Mathf.Abs(mouseX) > 40f || Mathf.Abs(mouseY) > 40f)
            {
                return;
            }

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -upDownLookLimit, upDownLookLimit);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            }

            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            if (characterController == null)
                return;

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 move =
                transform.right * moveX +
                transform.forward * moveZ;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            float speed =
                Input.GetKey(KeyCode.LeftShift)
                    ? runSpeed
                    : walkSpeed;

            characterController.Move(
                move * speed * Time.deltaTime
            );

            if (characterController.isGrounded &&
                velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;

            characterController.Move(
                velocity * Time.deltaTime
            );
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            skipLookDelta = true;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        public void SetControlEnabled(bool enabled)
        {
            isControlEnabled = enabled;

            if (!enabled)
                velocity = Vector3.zero;
        }
    }
}
