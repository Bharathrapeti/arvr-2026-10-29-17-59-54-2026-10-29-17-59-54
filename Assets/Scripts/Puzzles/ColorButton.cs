using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Puzzles
{
    public class ColorButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private string colorName = "Red";
        [SerializeField] private ColorSequencePuzzle puzzle;
        [SerializeField] private Renderer buttonRenderer;
        [SerializeField] private Color displayColor = Color.red;

        private float cooldown;

        public bool CanInteract =>
            cooldown <= 0f &&
            (GameManager.Instance == null || GameManager.Instance.IsGameplayActive);

        public string Prompt => $"[E] Press {colorName}";

        public void Bind(string name, ColorSequencePuzzle owner, Color color)
        {
            colorName = name;
            puzzle = owner;
            displayColor = color;

            if (buttonRenderer == null)
                buttonRenderer = GetComponent<Renderer>();

            ApplyColor();
        }

        private void Awake()
        {
            if (buttonRenderer == null)
                buttonRenderer = GetComponent<Renderer>();

            ApplyColor();
        }

        private void Update()
        {
            if (cooldown > 0f)
                cooldown -= Time.deltaTime;
        }

        public void Interact(GameObject instigator)
        {
            if (!CanInteract || puzzle == null)
                return;

            cooldown = 0.2f;

            bool ok = puzzle.Press(colorName);

            if (buttonRenderer != null)
            {
                buttonRenderer.material.color =
                    ok ? displayColor : Color.white;

                Invoke(nameof(ApplyColor), 0.15f);
            }
        }

        private void ApplyColor()
        {
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = displayColor;
            }
        }
    }
}