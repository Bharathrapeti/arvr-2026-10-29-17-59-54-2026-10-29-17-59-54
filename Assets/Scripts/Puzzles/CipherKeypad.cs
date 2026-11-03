using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.UI;

namespace AREscapeRoom.Puzzles
{
    public class CipherKeypad : MonoBehaviour, IInteractable
    {
        [SerializeField] private KeypadUI keypadUI;

        public string Prompt
        {
            get
            {
                if (GameManager.Instance != null &&
                    GameManager.Instance.CipherPuzzleCompleted)
                {
                    return "";
                }

                return "[E] Use Keypad";
            }
        }

        public bool CanInteract
        {
            get
            {
                return GameManager.Instance != null &&
                       GameManager.Instance.IsPlayable &&
                       !GameManager.Instance.CipherPuzzleCompleted;
            }
        }

        public void Bind(KeypadUI ui)
        {
            keypadUI = ui;
        }

        public void Interact(GameObject instigator)
        {
            if (keypadUI == null)
            {
                keypadUI = FindFirstObjectByType<KeypadUI>();
            }

            if (keypadUI == null)
            {
                Debug.LogWarning("[CipherKeypad] KeypadUI missing.");
                return;
            }

            keypadUI.ShowKeypad(OnSubmit);
        }

        private void OnSubmit(string value)
        {
            if (GameManager.Instance == null)
                return;

            bool ok = GameManager.Instance.TrySubmitCipher(value);

            if (ok)
            {
                keypadUI.ShowSuccess("ACCESS GRANTED");
            }
            else
            {
                keypadUI.ShowError("Incorrect Code");
            }
        }
    }
}