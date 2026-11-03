using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Puzzles
{
    public class DrawerController : MonoBehaviour, IInteractable
    {
        [SerializeField] private float openDistance = 0.45f;
        [SerializeField] private float openSpeed = 4f;

        private Transform drawer1;
        private Transform drawer2;
        private Transform player;

        private Vector3 drawer1Open;
        private Vector3 drawer2Open;

        private bool drawer1Opened;
        private bool drawer2Opened;
        private bool keyPickedUp;

        private KeyPickup keyPickup;

        private bool IsMainDrawer =>
            gameObject.name == "Drawer_Empty";

        public bool CanInteract => true;

        public string Prompt => "";

        private void Start()
        {
            PlayerController pc =
                Object.FindFirstObjectByType<PlayerController>();

            if (pc != null)
                player = pc.transform;

            if (!IsMainDrawer)
                return;

            GameObject d1 =
                GameObject.Find("Drawer_Empty");

            GameObject d2 =
                GameObject.Find("Drawer_Key");

            if (d1 != null)
                drawer1 = d1.transform;

            if (d2 != null)
                drawer2 = d2.transform;

            if (drawer1 != null)
            {
                drawer1Open =
                    drawer1.localPosition +
                    drawer1.forward * openDistance;
            }

            if (drawer2 != null)
            {
                drawer2Open =
                    drawer2.localPosition +
                    drawer2.forward * openDistance;
            }

            FindKey();
        }

        private void Update()
        {
            if (!IsMainDrawer)
                return;

            if (drawer1Opened && drawer1 != null)
            {
                drawer1.localPosition =
                    Vector3.Lerp(
                        drawer1.localPosition,
                        drawer1Open,
                        openSpeed * Time.deltaTime
                    );
            }

            if (drawer2Opened && drawer2 != null)
            {
                drawer2.localPosition =
                    Vector3.Lerp(
                        drawer2.localPosition,
                        drawer2Open,
                        openSpeed * Time.deltaTime
                    );
            }
        }

        private void OnGUI()
        {
            if (!IsMainDrawer)
                return;

            if (player == null)
                return;

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameplayActive)
                return;

            float distance =
                Vector3.Distance(
                    player.position,
                    transform.position
                );

            if (distance > 3.0f)
                return;

            Vector3 direction =
                transform.position -
                player.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            direction.Normalize();

            float facing =
                Vector3.Dot(
                    player.forward,
                    direction
                );

            if (facing < 0.15f)
                return;

            float width = 320f;
            float height = 210f;

            float x =
                (Screen.width - width) * 0.5f;

            float y =
                Screen.height - height - 40f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height
                ),
                "DESK DRAWERS"
            );

            // ==========================================
            // KEY PICKUP BUTTON
            // ==========================================

            if (drawer2Opened && !keyPickedUp)
            {
                if (GUI.Button(
                    new Rect(
                        x + 25f,
                        y + 70f,
                        width - 50f,
                        65f
                    ),
                    "PICK UP KEY"
                ))
                {
                    PickUpKey();
                }

                return;
            }

            // ==========================================
            // DRAWER 1
            // ==========================================

            GUI.enabled = !drawer1Opened;

            if (GUI.Button(
                new Rect(
                    x + 25f,
                    y + 45f,
                    width - 50f,
                    55f
                ),
                drawer1Opened
                    ? "DRAWER 1 OPENED"
                    : "OPEN DRAWER 1"
            ))
            {
                OpenDrawer1();
            }

            // ==========================================
            // DRAWER 2
            // ==========================================

            GUI.enabled = !drawer2Opened;

            if (GUI.Button(
                new Rect(
                    x + 25f,
                    y + 115f,
                    width - 50f,
                    55f
                ),
                "OPEN DRAWER 2"
            ))
            {
                OpenDrawer2();
            }

            GUI.enabled = true;
        }

        private void OpenDrawer1()
        {
            drawer1Opened = true;

            Debug.Log(
                "[DrawerController] Drawer 1 opened."
            );
        }

        private void OpenDrawer2()
        {
            drawer2Opened = true;

            FindKey();

            Debug.Log(
                "[DrawerController] Drawer 2 opened."
            );
        }

        private void PickUpKey()
        {
            if (keyPickedUp)
                return;

            if (keyPickup == null)
            {
                FindKey();
            }

            keyPickedUp = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectKey();
            }

            if (keyPickup != null)
            {
                keyPickup.gameObject.SetActive(false);
            }

            Debug.Log(
                "[DrawerController] KEY PICKED UP!"
            );
        }

        private void FindKey()
        {
            if (keyPickup != null)
                return;

            KeyPickup[] keys =
                Object.FindObjectsByType<KeyPickup>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != null &&
                    keys[i].gameObject.name == "Key")
                {
                    keyPickup = keys[i];
                    return;
                }
            }

            if (keys.Length > 0)
                keyPickup = keys[0];
        }

        public void Interact(GameObject instigator)
        {
            // Interaction is handled by the popup.
        }

        public void Bind(
            bool hasKey,
            KeyPickup pickup
        )
        {
            keyPickup = pickup;

            if (keyPickup != null)
                keyPickup.gameObject.SetActive(false);
        }
    }
}