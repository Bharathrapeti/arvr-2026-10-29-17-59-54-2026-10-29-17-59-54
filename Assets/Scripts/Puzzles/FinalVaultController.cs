using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Doors;

namespace AREscapeRoom.Puzzles
{
    public class FinalVaultController : MonoBehaviour, IInteractable
    {
        [SerializeField] private ProgressionDoor finalDoor;
        [SerializeField] private bool submitted;

        public bool CanInteract => !submitted;
        public string Prompt => "[E] Activate Pedestal";

        public void Bind(ProgressionDoor door)
        {
            finalDoor = door;
        }

        public void Interact(GameObject instigator)
        {
            if (submitted)
                return;

            GameManager gm = GameManager.Instance;

            if (gm == null)
                return;

            if (!gm.Meets(DoorRequirement.RequiresFinalValidation))
            {
                string msg = gm.MissingRequirementMessage(
                    DoorRequirement.RequiresFinalValidation
                );

                Debug.Log($"[FinalVault] {msg}");

                if (AREscapeRoom.UI.HUDController.Instance != null)
                {
                    AREscapeRoom.UI.HUDController.Instance.SetPrompt(msg);
                }

                return;
            }

            submitted = true;

            if (finalDoor != null)
                finalDoor.OpenNow();

            gm.TriggerVictory();
        }
    }
}