using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Puzzles;
using AREscapeRoom.UI;

namespace AREscapeRoom.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float interactDistance = 3.5f;
        [SerializeField] private Transform holdPoint;

        private IInteractable current;
        private ShapeObject carried;
        private Collider lastHitCollider;
        private IInteractable lastInteractable;

        public ShapeObject Carried => carried;

        public Transform HoldPoint => holdPoint;

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

            if (cameraTransform != null)
            {
                if (holdPoint == null)
                {
                    Transform existingHp = cameraTransform.Find("HoldPoint");
                    if (existingHp != null)
                    {
                        holdPoint = existingHp;
                    }
                    else
                    {
                        GameObject hp = new GameObject("HoldPoint");
                        hp.transform.SetParent(cameraTransform, false);
                        holdPoint = hp.transform;
                    }
                }
                holdPoint.localPosition = new Vector3(0.32f, -0.28f, 0.75f);
                holdPoint.localRotation = Quaternion.identity;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetPrompt("");
        }

        private void Update()
        {
            if (carried != null && holdPoint != null)
            {
                carried.FollowHoldPoint(holdPoint);

                // Allow dropping the carried shape with Right-Click or Q
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Q))
                {
                    DropCarried();
                    return;
                }
            }

            // Allow clicking interactables even when the cursor has been unlocked.
            if (KeypadUI.AnyVisible)
            {
                SetPrompt("");
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPointerVisible(false);
                }
                return;
            }

            if (HUDController.Instance != null)
            {
                HUDController.Instance.SetPointerVisible(true);
            }

            if (cameraTransform == null)
                return;

            current = RaycastInteractable();

            if (current != null && current.CanInteract)
            {
                SetPrompt(current.Prompt);

                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPointerHover(true);
                }

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                {
                    current.Interact(gameObject);
                }
            }
            else
            {
                SetPrompt("");

                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPointerHover(false);
                }
            }
        }

        public void SetCarried(ShapeObject shape)
        {
            carried = shape;
        }

        public void DropCarried()
        {
            if (carried == null) return;
            ShapeObject shapeToDrop = carried;
            carried = null;

            shapeToDrop.ReleaseCarry();

            GameObject room5 = GameObject.Find("Room_5");
            shapeToDrop.transform.SetParent(room5 != null ? room5.transform : null, true);

            shapeToDrop.SetPhysics(true);
            Debug.Log($"[PlayerInteraction] Dropped carried shape: {shapeToDrop.name}");
        }

        private IInteractable RaycastInteractable()
        {
            if (cameraTransform == null)
                return null;

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            // Raycast including triggers, but filtering out non-interactable triggers (like RoomZone)
            RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, ~0, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                lastHitCollider = null;
                lastInteractable = null;
                return null;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // Ignore player body colliders
                if (hit.collider.transform.IsChildOf(transform)) continue;

                // Ignore carried object colliders
                if (carried != null && (hit.collider.transform == carried.transform || hit.collider.transform.IsChildOf(carried.transform)))
                    continue;

                // Direct interactable check on the object pointed at by the cursor
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    // Pass through triggers that are not interactable themselves (e.g. RoomZone, proximity zones)
                    if (hit.collider.isTrigger)
                        continue;

                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }

                if (interactable != null)
                {
                    if (interactable.CanInteract)
                    {
                        lastHitCollider = hit.collider;
                        lastInteractable = interactable;
                        return interactable;
                    }
                    else
                    {
                        // Object hit is interactable but cannot be interacted with right now
                        lastHitCollider = hit.collider;
                        lastInteractable = null;
                        return null;
                    }
                }

                // If cursor hits a solid surface (wall, floor, table, etc.) that is not interactable,
                // line of sight is blocked and cursor is pointing at that surface. Stop here.
                lastHitCollider = hit.collider;
                lastInteractable = null;
                return null;
            }

            lastHitCollider = null;
            lastInteractable = null;
            return null;
        }

        private static void SetPrompt(string text)
        {
            if (HUDController.Instance != null)
            {
                HUDController.Instance.SetPrompt(text);
            }
        }
    }
}
