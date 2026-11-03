using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Doors;
using AREscapeRoom.UI;

namespace AREscapeRoom.Puzzles
{
    public class CipherPuzzle : MonoBehaviour, IInteractable
    {
        [SerializeField] private KeypadUI keypadUI;
        [SerializeField] private ProgressionDoor exitDoor;
        [SerializeField] private string passwordOverride = "4729";

        public bool CanInteract
        {
            get
            {
                GameManager gm = GameManager.Instance;

                return gm != null &&
                       gm.IsGameplayActive &&
                       !gm.CipherPuzzleCompleted;
            }
        }

        public string Prompt =>
            CanInteract ? "LEFT CLICK TO USE NUMPAD" : "";

        public void Bind(
            KeypadUI ui,
            ProgressionDoor door)
        {
            keypadUI = ui;
            exitDoor = door;
        }

        public void Interact(GameObject instigator)
        {
            OpenKeypad();
        }

        public void OpenKeypad()
        {
            if (keypadUI == null)
            {
                keypadUI =
                    Object.FindFirstObjectByType<KeypadUI>(
                        FindObjectsInactive.Include
                    );
            }

            if (keypadUI == null)
            {
                Debug.LogWarning(
                    "[CipherPuzzle] KeypadUI missing."
                );

                return;
            }

            keypadUI.ShowKeypad(OnSubmit);
        }

        private void OnSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                ShowIncorrect();
                return;
            }

            string entered =
                input.Trim();

            GameManager gm = GameManager.Instance;

            // Room 2 password fallback logic: override -> GameManager -> default 4729
            string expected = "4729";
            if (!string.IsNullOrEmpty(passwordOverride))
            {
                expected = passwordOverride.Trim();
            }
            else if (gm != null && !string.IsNullOrEmpty(gm.CipherPassword))
            {
                expected = gm.CipherPassword.Trim();
            }

            Debug.Log(
                $"[CipherPuzzle] Entered: {entered} | Expected: {expected}"
            );

            // Check the password directly first.
            bool correct =
                string.Equals(
                    entered,
                    expected,
                    System.StringComparison.Ordinal
                );

            if (!correct)
            {
                ShowIncorrect();
                return;
            }

            // Password is correct.
            if (gm != null)
            {
                // Mark Room 2 cipher as completed.
                bool managerAccepted =
                    gm.TrySubmitCipher(expected);

                Debug.Log(
                    $"[CipherPuzzle] GameManager accepted password: {managerAccepted}"
                );
            }

            if (keypadUI != null)
            {
                keypadUI.ShowSuccess(
                    "ACCESS GRANTED"
                );
            }

            if (exitDoor == null)
            {
                ProgressionDoor[] doors = Object.FindObjectsByType<ProgressionDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (ProgressionDoor pd in doors)
                {
                    if (pd != null && (pd.gameObject.name.Contains("R2_R3") || pd.gameObject.name.Contains("Door_10_White")))
                    {
                        exitDoor = pd;
                        break;
                    }
                }
            }

            // Open the Room 2 -> Room 3 door.
            if (exitDoor != null)
            {
                exitDoor.OpenNow();

                Debug.Log(
                    "[CipherPuzzle] Correct password. Room 2 completed. Door to Room 3 opened."
                );
            }
            else
            {
                Debug.LogWarning(
                    "[CipherPuzzle] Exit door reference is missing."
                );
            }
        }

        private void ShowIncorrect()
        {
            if (keypadUI != null)
            {
                keypadUI.ShowError(
                    "INCORRECT CODE"
                );
            }

            Debug.Log(
                "[CipherPuzzle] Incorrect password. Expected 4729."
            );
        }
    }
}