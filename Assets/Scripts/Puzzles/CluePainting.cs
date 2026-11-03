using UnityEngine;
using UnityEngine.UI;
using AREscapeRoom.Core;

namespace AREscapeRoom.Puzzles
{
    public class CluePainting : MonoBehaviour, IInteractable
    {
        [SerializeField] private string clueTitle = "Painting";
        [SerializeField] private string clueText = "Look closer...";
        [SerializeField] private float displaySeconds = 2.5f;

        private static Text overlay;
        private static float hideAt;

        public bool CanInteract => true;
        public string Prompt => $"[E] Inspect {clueTitle}";

        public void Bind(string title, string text)
        {
            clueTitle = title;
            clueText = text;
        }

        public void Interact(GameObject instigator)
        {
            EnsureOverlay();

            if (overlay != null)
            {
                overlay.text = clueText;
                overlay.transform.parent.gameObject.SetActive(true);
                hideAt = Time.unscaledTime + displaySeconds;
            }
        }

        private void Update()
        {
            if (overlay != null &&
                overlay.transform.parent.gameObject.activeSelf &&
                Time.unscaledTime >= hideAt)
            {
                overlay.transform.parent.gameObject.SetActive(false);
            }
        }

        private static void EnsureOverlay()
        {
            if (overlay != null)
                return;

            AREscapeRoom.UI.HUDController hud =
                AREscapeRoom.UI.HUDController.Instance;

            Transform parent = hud != null ? hud.transform : null;

            if (parent == null)
                return;

            GameObject panel = AREscapeRoom.UI.UiFactory.Panel(
                parent,
                "ClueOverlay",
                new Vector2(0.25f, 0.35f),
                new Vector2(0.75f, 0.55f),
                Vector2.zero,
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.7f)
            );

            overlay = AREscapeRoom.UI.UiFactory.Label(
                panel.transform,
                "ClueText",
                "",
                Vector2.zero,
                Vector2.one,
                28,
                TextAnchor.MiddleCenter
            );

            panel.SetActive(false);
        }
    }
}