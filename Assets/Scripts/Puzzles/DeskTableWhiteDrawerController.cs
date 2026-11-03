using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Puzzles
{
    public class DeskTableWhiteDrawerController : MonoBehaviour, IInteractable
    {
        [Header("Drawer Settings")]
        [SerializeField] private float openDistance = 0.46f;
        [SerializeField] private float slideSpeed = 5f;

        [Header("Key Puzzle")]
        [SerializeField] private KeyPickup keyPickup;

        private Vector3 closedLocalPos;
        private Vector3 openLocalPos;
        private bool isOpen;
        private bool initialized;

        public bool IsOpen => isOpen;

        public bool CanInteract => true;

        public string Prompt => isOpen ? "[E] Close Drawer" : "[E] Open Drawer";

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            Init();

            if (keyPickup == null)
            {
                keyPickup = GetComponentInChildren<KeyPickup>(true);
            }
        }

        public void Init()
        {
            if (initialized) return;

            closedLocalPos = transform.localPosition;
            openLocalPos = closedLocalPos + new Vector3(0f, 0f, openDistance);

            // Ensure collider only covers the drawer front/handle so it does NOT block
            // the interior tray or raycasts aiming at the Stylized Key inside.
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                col.center = new Vector3(0.01f, 0.11f, 0.02f);
                col.size = new Vector3(0.40f, 0.22f, 0.06f);
            }

            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            Vector3 target = isOpen ? openLocalPos : closedLocalPos;
            if (Vector3.Distance(transform.localPosition, target) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, target, slideSpeed * Time.deltaTime);
            }
        }

        public void Interact(GameObject instigator)
        {
            Toggle();
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            Debug.Log($"[DeskTableWhiteDrawerController] Drawer is now {(isOpen ? "OPEN" : "CLOSED")}.");
        }

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
        }

        public void BindKey(KeyPickup pickup)
        {
            keyPickup = pickup;
        }
    }
}
