using UnityEngine;

namespace AREscapeRoom.Core
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        string Prompt { get; }
        void Interact(GameObject instigator);
    }
}