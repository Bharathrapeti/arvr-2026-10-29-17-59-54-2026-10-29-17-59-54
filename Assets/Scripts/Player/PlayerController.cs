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
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float upDownLookLimit = 60.0f;

        private CharacterController characterController;
        private Vector3 velocity;
        private float verticalRotation;
        private bool isControlEnabled = true;
        private int skipLookFrames = 10;

        public bool IsControlEnabled => isControlEnabled;

        public void AssignLookCamera(Transform cam)
        {
            cameraTransform = cam;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            EnsureSolidFloorCollider();
        }

        private void Start()
        {
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraTransform = cam.transform;
            }

            if (cameraTransform == null)
            {
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj == null) camObj = GameObject.Find("Main Camera");
                if (camObj != null) cameraTransform = camObj.transform;
            }

            // In game: place camera and player firmly in front of AR_Escape_Room_Structure
            PlaceInFrontOfEscapeRoom();

            velocity = Vector3.zero;
            skipLookFrames = 10;
            LockCursor();
        }

        public void PlaceInFrontOfEscapeRoom()
        {
            GameObject door = GameObject.Find("Door_5_Brown");
            if (door == null) door = GameObject.Find("Door_5_Room");
            GameObject room = GameObject.Find("AR_Escape_Room_Structure");

            Vector3 targetPos;
            if (door != null)
            {
                // In front of the entrance door (2.8m back along Z, on floor at y = 0.05)
                targetPos = new Vector3(door.transform.position.x, 0.05f, door.transform.position.z - 2.8f);
            }
            else if (room != null)
            {
                // In front of the room structure (front wall is at z = -5 local)
                targetPos = new Vector3(room.transform.position.x + 0.1f, 0.05f, room.transform.position.z - 8.4f);
            }
            else
            {
                targetPos = new Vector3(0.109f, 0.05f, -28.459f);
            }

            if (characterController != null)
                characterController.enabled = false;

            transform.position = targetPos;
            transform.rotation = Quaternion.identity;

            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0f, 1.6f, 0f);
                cameraTransform.localRotation = Quaternion.identity;
            }

            verticalRotation = 0f;

            if (characterController != null)
                characterController.enabled = true;

            Debug.Log($"[PlayerController] In game: camera placed in front of AR_Escape_Room_Structure at {targetPos}");
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

            // If mouse is unlocked, lock it when user clicks back into the game window
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
            // Discard initial recentering frames after locking cursor or regaining focus
            if (skipLookFrames > 0)
            {
                skipLookFrames--;
                Input.GetAxis("Mouse X");
                Input.GetAxis("Mouse Y");
                return;
            }

            float dt = Mathf.Min(Time.deltaTime, 0.05f);

            float rawX = Input.GetAxis("Mouse X");
            float rawY = Input.GetAxis("Mouse Y");

            // Cap raw axis deltas to protect against sudden mouse focus spikes
            rawX = Mathf.Clamp(rawX, -2f, 2f);
            rawY = Mathf.Clamp(rawY, -2f, 2f);

            float mouseX = rawX * mouseSensitivity * 60f * dt;
            float mouseY = rawY * mouseSensitivity * 60f * dt;

            // Clamp max rotation per single frame to 3 degrees to eliminate any spinning or sudden pitch
            mouseX = Mathf.Clamp(mouseX, -3f, 3f);
            mouseY = Mathf.Clamp(mouseY, -3f, 3f);

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

            float dt = Mathf.Min(Time.deltaTime, 0.05f);

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            Vector3 horizontalMove = move * speed;

            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }
            else
            {
                velocity.y += gravity * dt;
            }

                        Vector3 totalMove = (horizontalMove + velocity) * dt;
            characterController.Move(totalMove);

            // Extreme emergency fallback: recover player only if they fall below the world floor (-5m)
            if (transform.position.y < -5.0f)
            {
                characterController.enabled = false;
                transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
                characterController.enabled = true;
                velocity = Vector3.zero;
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            skipLookFrames = 10;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            skipLookFrames = 10;
        }

        private void ToggleCursor()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
                Debug.Log("[PlayerController] Mouse unlocked.");
            }
            else
            {
                LockCursor();
                Debug.Log("[PlayerController] Mouse locked.");
            }
        }

        public void SetControlEnabled(bool enabled)
        {
            isControlEnabled = enabled;

            if (!enabled)
                velocity = Vector3.zero;
        }

        private void EnsureSolidFloorCollider()
        {
            GameObject ground = GameObject.Find("SolidGround");
            if (ground == null)
            {
                ground = new GameObject("SolidGround");
                ground.transform.position = new Vector3(0f, -0.01f, 0f);
            }

            BoxCollider bc = ground.GetComponent<BoxCollider>();
            if (bc == null)
            {
                bc = ground.AddComponent<BoxCollider>();
            }

            // Solid 2-meter thick floor slab: top surface at y = 0.0f
            bc.center = new Vector3(0f, -1.0f, -25.0f);
            bc.size = new Vector3(200f, 2.0f, 200f);
            bc.isTrigger = false;
            bc.enabled = true;
        }
    }
}