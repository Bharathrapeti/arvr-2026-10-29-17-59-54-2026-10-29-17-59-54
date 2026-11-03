using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Puzzles;
using AREscapeRoom.UI;

namespace AREscapeRoom.Doors
{
    public class ProgressionDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private DoorController door;
        [SerializeField] private DoorRequirement requirement =
            DoorRequirement.RequiresKey;

        [SerializeField] private bool autoOpenWhenMet;
        [SerializeField] private int leadsToRoom = 2;
        [SerializeField] private CipherPuzzle cipherPuzzle;

        private bool opened;

        public bool CanInteract
        {
            get
            {
                if (opened)
                    return false;

                if (door != null && door.IsOpen)
                    return false;

                return true;
            }
        }

        public string Prompt
        {
            get
            {
                GameManager gm = GameManager.Instance;

                if (gm == null)
                    return "[E] Door";

                if (requirement == DoorRequirement.RequiresPassword)
                {
                    if (!gm.CipherPuzzleCompleted)
                        return "[E] Use Keypad";

                    return "[E] Open Door";
                }

                if (!gm.Meets(requirement))
                {
                    string missing =
                        gm.MissingRequirementMessage(requirement);

                    return string.IsNullOrEmpty(missing)
                        ? "[E] Locked"
                        : "[E] " + missing;
                }

                return "[E] Open Door";
            }
        }

        public void Bind(
            DoorController doorController,
            DoorRequirement req,
            bool autoOpen,
            int nextRoom)
        {
            door = doorController;
            requirement = req;
            autoOpenWhenMet = autoOpen;
            leadsToRoom = nextRoom;
        }

        public void BindCipher(CipherPuzzle puzzle)
        {
            cipherPuzzle = puzzle;
        }

        private void Awake()
        {
            if (door == null)
                door = GetComponent<DoorController>();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += TryAutoOpen;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= TryAutoOpen;
        }

        private void Start()
        {
            if (door == null)
                door = GetComponent<DoorController>();

            if (door != null && !opened)
                door.LockDoor();

            TryAutoOpen();
        }

        public void Interact(GameObject instigator)
        {
            GameManager gm = GameManager.Instance;

            if (gm == null || !gm.IsGameplayActive)
                return;

            if (requirement == DoorRequirement.RequiresPassword &&
                !gm.CipherPuzzleCompleted)
            {
                if (cipherPuzzle != null)
                {
                    cipherPuzzle.OpenKeypad();
                }

                return;
            }

            if (!gm.Meets(requirement))
            {
                string missing =
                    gm.MissingRequirementMessage(requirement);

                if (HUDController.Instance != null)
                {
                    HUDController.Instance.SetPrompt(
                        string.IsNullOrEmpty(missing)
                            ? "[E] Locked"
                            : "[E] " + missing
                    );
                }

                return;
            }

            OpenNow();
        }

        public void OpenNow()
        {
            if (opened)
                return;

            opened = true;

            if (door != null)
            {
                door.UnlockDoor();
                door.OpenDoor();
            }

            if (GameManager.Instance != null)
                GameManager.Instance.NotifyDoorOpened();
        }

        private void TryAutoOpen()
        {
            if (opened ||
                !autoOpenWhenMet ||
                GameManager.Instance == null ||
                door == null)
                return;

            if (GameManager.Instance.Meets(requirement))
                OpenNow();
        }
    }
}