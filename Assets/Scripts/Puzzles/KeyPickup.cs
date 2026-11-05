using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.UI;

namespace AREscapeRoom.Puzzles
{
    public class KeyPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool collected;

        public bool Collected => collected;

        public bool CanInteract
        {
            get
            {
                if (collected) return false;
                if (GameManager.Instance == null || !GameManager.Instance.IsGameplayActive) return false;

                DeskTableWhiteDrawerController drawer = GetComponentInParent<DeskTableWhiteDrawerController>();
                if (drawer != null && !drawer.IsOpen)
                {
                    return false;
                }

                return true;
            }
        }

        public string Prompt => "[E] TAKE STYLIZED KEY";

        public void Interact(GameObject instigator)
        {
            if (collected)
                return;

            if (GameManager.Instance == null)
                return;

            collected = true;

            GameManager.Instance.CollectKey();

            Debug.Log(
                "[KeyPickup] STYLIZED KEY COLLECTED - Room 1 Exit Unlocked!"
            );

            if (HUDController.Instance != null)
            {
                HUDController.Instance.SetPrompt("Stylized Key Collected! Exit Door Unlocked.");
            }

            // Interface controls for Room 1: clear hint button and update objective text
            if (EscapeRoomManager.Instance != null)
            {
                EscapeRoomManager.Instance.OnKeyDiscovered();
            }

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.OnKeyDiscovered();
            }

            gameObject.SetActive(false);
        }
    }
}
