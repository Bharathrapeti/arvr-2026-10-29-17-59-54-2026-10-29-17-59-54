using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Puzzles
{
    public class OldSimpleDrawerController : MonoBehaviour, IInteractable
    {
        [Header("Drawer Transforms")]
        [SerializeField] private Transform drawerLeft;
        [SerializeField] private Transform drawerRight;

        [Header("Movement Settings")]
        [SerializeField] private float openDistance = 1.6f;
        [SerializeField] private float slideSpeed = 4f;

        [Header("Key Puzzle Integration")]
        [SerializeField] private KeyPickup keyPickup;
        [SerializeField] private Transform keyMountPoint;

        private Vector3 leftClosedPos;
        private Vector3 leftOpenPos;
        private Vector3 rightClosedPos;
        private Vector3 rightOpenPos;

        private bool leftOpen = false;
        private bool rightOpen = false;
        private bool initialized = false;

        private Transform player;

        public bool IsOpen => leftOpen || rightOpen;
        public bool IsLeftOpen => leftOpen;
        public bool IsRightOpen => rightOpen;

        public bool CanInteract => true;

        public string Prompt
        {
            get
            {
                if (IsOpen)
                    return "[E] Close Drawer";
                return "[E] Open Drawer";
            }
        }

        private void Awake()
        {
            InitTransforms();
        }

        private void Start()
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null)
                player = pc.transform;

            FindAndMountKey();
        }

        public void InitTransforms()
        {
            if (initialized) return;

            if (drawerLeft == null)
            {
                Transform t = transform.Find("Drawer_Left");
                if (t != null) drawerLeft = t;
            }

            if (drawerRight == null)
            {
                Transform t = transform.Find("Drawer_Right");
                if (t != null) drawerRight = t;
            }

            if (drawerLeft != null)
            {
                leftClosedPos = drawerLeft.localPosition;
                leftOpenPos = leftClosedPos + Vector3.forward * openDistance;
                EnsureDrawerLeaf(drawerLeft.gameObject, true);
            }

            if (drawerRight != null)
            {
                rightClosedPos = drawerRight.localPosition;
                rightOpenPos = rightClosedPos + Vector3.forward * openDistance;
                EnsureDrawerLeaf(drawerRight.gameObject, false);
            }

            initialized = true;
        }

        private void EnsureDrawerLeaf(GameObject go, bool isLeft)
        {
            BoxCollider box = go.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = go.AddComponent<BoxCollider>();
            }

            DrawerLeaf leaf = go.GetComponent<DrawerLeaf>();
            if (leaf == null)
            {
                leaf = go.AddComponent<DrawerLeaf>();
            }
            leaf.Bind(this, isLeft);
        }

        private void Update()
        {
            if (!initialized) return;

            if (drawerLeft != null)
            {
                Vector3 target = leftOpen ? leftOpenPos : leftClosedPos;
                drawerLeft.localPosition = Vector3.Lerp(drawerLeft.localPosition, target, slideSpeed * Time.deltaTime);
            }

            if (drawerRight != null)
            {
                Vector3 target = rightOpen ? rightOpenPos : rightClosedPos;
                drawerRight.localPosition = Vector3.Lerp(drawerRight.localPosition, target, slideSpeed * Time.deltaTime);
            }
        }

        public void Interact(GameObject instigator)
        {
            ToggleAll();
        }

        public void ToggleLeft()
        {
            leftOpen = !leftOpen;
            OnDrawerStateChanged();
        }

        public void ToggleRight()
        {
            rightOpen = !rightOpen;
            OnDrawerStateChanged();
        }

        public void ToggleAll()
        {
            if (IsOpen)
            {
                leftOpen = false;
                rightOpen = false;
            }
            else
            {
                leftOpen = true;
                rightOpen = true;
            }
            OnDrawerStateChanged();
        }

        public void OpenDrawer()
        {
            leftOpen = true;
            rightOpen = true;
            OnDrawerStateChanged();
        }

        public void CloseDrawer()
        {
            leftOpen = false;
            rightOpen = false;
            OnDrawerStateChanged();
        }

        private void OnDrawerStateChanged()
        {
            Debug.Log($"[OldSimpleDrawerController] Drawer state: Left={(leftOpen ? "OPEN" : "CLOSED")}, Right={(rightOpen ? "OPEN" : "CLOSED")}");

            if (IsOpen && keyPickup != null && !keyPickup.Collected)
            {
                keyPickup.gameObject.SetActive(true);
            }
        }

        public void FindAndMountKey()
        {
            if (keyPickup != null) return;

            KeyPickup[] keys = Object.FindObjectsByType<KeyPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != null && keys[i].gameObject.name.Contains("Key"))
                {
                    keyPickup = keys[i];
                    break;
                }
            }

            if (keyPickup != null)
            {
                // Mount key inside drawerLeft so it moves with the drawer, respecting manual editor transform
                Transform parentDrawer = drawerLeft != null ? drawerLeft : transform;
                keyPickup.transform.SetParent(parentDrawer, true);
                // Respect visual editor scene coordinates:
                // keyPickup.transform.localPosition = new Vector3(-0.9f, 1.55f, 0.5f);
                // keyPickup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                // keyPickup.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }
        }

        private void OnGUI()
        {
            if (player == null) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive) return;

            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > 3.0f) return;

            Vector3 dir = (transform.position - player.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            dir.Normalize();

            if (Vector3.Dot(player.forward, dir) < 0.2f) return;

            // Draw compact on-screen interactive control
            float w = 240f;
            float h = 130f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 35f;

            GUI.Box(new Rect(x, y, w, h), "WOODEN DRAWER");

            string btnText = IsOpen ? "CLOSE DRAWER" : "OPEN DRAWER";
            if (GUI.Button(new Rect(x + 20f, y + 35f, w - 40f, 40f), btnText))
            {
                ToggleAll();
            }

            if (keyPickup != null && !keyPickup.Collected && IsOpen)
            {
                if (GUI.Button(new Rect(x + 20f, y + 80f, w - 40f, 40f), "PICK UP KEY"))
                {
                    keyPickup.Interact(gameObject);
                }
            }
            else
            {
                string stateText = $"Left: {(leftOpen ? "Open" : "Closed")} | Right: {(rightOpen ? "Open" : "Closed")}";
                GUI.Label(new Rect(x + 20f, y + 85f, w - 40f, 30f), stateText);
            }
        }

        public class DrawerLeaf : MonoBehaviour, IInteractable
        {
            private OldSimpleDrawerController controller;
            private bool isLeft;

            public void Bind(OldSimpleDrawerController ctrl, bool left)
            {
                controller = ctrl;
                isLeft = left;
            }

            public bool CanInteract => controller != null;

            public string Prompt
            {
                get
                {
                    if (controller == null) return "";
                    bool open = isLeft ? controller.IsLeftOpen : controller.IsRightOpen;
                    string side = isLeft ? "Left" : "Right";
                    return open ? $"[E] Close {side} Drawer" : $"[E] Open {side} Drawer";
                }
            }

            public void Interact(GameObject instigator)
            {
                if (controller == null) return;
                if (isLeft) controller.ToggleLeft();
                else controller.ToggleRight();
            }
        }
    }
}
