using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Puzzles
{
    public class GemCollectible : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool collected;

        public bool CanInteract => !collected;
        public string Prompt => "[E] Collect Gem";

        public void Interact(GameObject instigator)
        {
            if (collected)
                return;

            collected = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectGem();
            }

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (collected)
                return;

            transform.Rotate(
                Vector3.up,
                60f * Time.deltaTime,
                Space.World
            );
        }
    }
}