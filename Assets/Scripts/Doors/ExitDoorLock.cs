using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.UI;

namespace AREscapeRoom.Doors
{
    public class ExitDoorLock : MonoBehaviour
    {
        [Header("Exit Door Configuration")]
        [SerializeField] private DoorController exitDoor;
        [SerializeField] private string correctPassword = "1234";

        [Header("Keypad UI")]
        [SerializeField] private KeypadUI keypadUI;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 4.0f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private Transform playerTransform;
        private bool unlocked;

        public string CorrectPassword => correctPassword;

        public void Bind(DoorController door, KeypadUI ui, string password)
        {
            exitDoor = door;
            keypadUI = ui;
            if (!string.IsNullOrEmpty(password))
            {
                correctPassword = password;
            }
        }

        private void Start()
        {
            if (exitDoor != null)
            {
                exitDoor.LockDoor();
            }

            FindPlayer();
        }

        private void Update()
        {
            if (unlocked) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            if (exitDoor == null || keypadUI == null) return;

            float dist = Vector3.Distance(GetDoorPosition(), playerTransform.position);
            bool inRange = dist <= interactionDistance && exitDoor.IsLocked;

            if (inRange)
            {
                if (!keypadUI.IsKeypadVisible)
                {
                    OpenKeypad();
                }
            }
            else if (keypadUI.IsKeypadVisible && dist > interactionDistance + 1.5f)
            {
                keypadUI.HideKeypad();
            }

            if (inRange && Input.GetKeyDown(interactKey) && !keypadUI.IsKeypadVisible)
            {
                OpenKeypad();
            }
        }

        public void OpenKeypad()
        {
            if (keypadUI != null)
            {
                keypadUI.ShowKeypad(OnSubmitPassword);
            }
            else
            {
                Debug.LogWarning("[ExitDoorLock] KeypadUI reference is missing!");
            }
        }

        private Vector3 GetDoorPosition()
        {
            return exitDoor != null ? exitDoor.transform.position : transform.position;
        }

        private void OnSubmitPassword(string inputPassword)
        {
            bool isCorrect = inputPassword == correctPassword;

            if (EscapeRoomManager.Instance != null)
            {
                EscapeRoomManager.Instance.NotifyPasswordAttempt(isCorrect);
            }

            if (isCorrect)
            {
                unlocked = true;
                Debug.Log($"[ExitDoorLock] Password '{inputPassword}' ACCEPTED! Unlocking mall door.");
                if (keypadUI != null) keypadUI.ShowSuccess("ACCESS GRANTED! ENTER THE MALL");
                if (exitDoor != null)
                {
                    exitDoor.UnlockDoor();
                    exitDoor.OpenDoor();
                }
            }
            else
            {
                Debug.Log($"[ExitDoorLock] Password '{inputPassword}' INCORRECT!");
                if (keypadUI != null) keypadUI.ShowError("INCORRECT PASSWORD! TRY AGAIN");
            }
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void OnGUI()
        {
            if (unlocked || playerTransform == null || exitDoor == null) return;
            float dist = Vector3.Distance(GetDoorPosition(), playerTransform.position);
            if (dist > interactionDistance) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 40, Screen.width, 40), "Mall door locked — enter password 1234", style);
        }
    }
}
