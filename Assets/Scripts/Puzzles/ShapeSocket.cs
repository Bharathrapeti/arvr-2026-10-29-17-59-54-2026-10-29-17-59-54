using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Puzzles
{
    public class ShapeSocket : MonoBehaviour, IInteractable
    {
        [SerializeField] private ShapeType acceptedType = ShapeType.Square;
        [SerializeField] private ShapePlacementPuzzle puzzle;
        [SerializeField] private bool filled;

        public bool Filled => filled;
        public bool CanInteract
        {
            get
            {
                if (filled) return false;
                var player = Object.FindFirstObjectByType<PlayerInteraction>();
                // Sockets only prompt and accept interaction when player is holding a shape
                if (player == null || player.Carried == null) return false;
                return true;
            }
        }
        public ShapeType AcceptedType => acceptedType;

        public string Prompt
        {
            get
            {
                if (filled) return "";
                var player = Object.FindFirstObjectByType<PlayerInteraction>();
                if (player != null && player.Carried != null)
                {
                    if (player.Carried.Type == acceptedType)
                    {
                        switch (acceptedType)
                        {
                            case ShapeType.Triangle: return "[E] Place Traingle Shape";
                            case ShapeType.Star: return "[E] Place Star Shape";
                            case ShapeType.Circle: return "[E] Place Circle Shape";
                            case ShapeType.Square: return "[E] Place Square Shape";
                            default: return $"[E] Place {acceptedType} Shape";
                        }
                    }
                    else
                    {
                        string shapeName = acceptedType == ShapeType.Triangle ? "Traingle" : acceptedType.ToString();
                        return $"Cannot Place: Only {shapeName} Shape fits here";
                    }
                }
                return "";
            }
        }

        private void Awake()
        {
            DetectSocketType();

            if (puzzle == null)
            {
                puzzle = GetComponentInParent<ShapePlacementPuzzle>();
                if (puzzle == null)
                    puzzle = Object.FindFirstObjectByType<ShapePlacementPuzzle>();
            }
        }

        public void DetectSocketType()
        {
            string lower = gameObject.name.ToLower();
            if (lower.Contains("triang") || lower.Contains("traing") || lower.Contains("tri"))
                acceptedType = ShapeType.Triangle;
            else if (lower.Contains("circle") || lower.Contains("circ"))
                acceptedType = ShapeType.Circle;
            else if (lower.Contains("star"))
                acceptedType = ShapeType.Star;
            else if (lower.Contains("square") || lower.Contains("sq"))
                acceptedType = ShapeType.Square;
        }

        public void Bind(ShapeType type, ShapePlacementPuzzle owner)
        {
            acceptedType = type;
            puzzle = owner;
        }

        public void Interact(GameObject instigator)
        {
            if (filled)
                return;

            PlayerInteraction interaction = instigator.GetComponent<PlayerInteraction>();
            if (interaction == null)
            {
                interaction = instigator.GetComponentInParent<PlayerInteraction>();
            }

            if (interaction == null || interaction.Carried == null)
            {
                Debug.Log($"[ShapeSocket] Cannot place into {acceptedType} socket: Player is not holding any shape.");
                return;
            }

            ShapeObject shape = interaction.Carried;

            if (shape.Type != acceptedType)
            {
                Debug.LogWarning($"[ShapeSocket] {shape.Type} does not fit {acceptedType} socket.");
                return;
            }

            shape.ReleaseCarry();
            interaction.SetCarried(null);
            shape.PlaceInSocket(transform);

            filled = true;

            if (puzzle == null)
            {
                puzzle = GetComponentInParent<ShapePlacementPuzzle>();
                if (puzzle == null)
                    puzzle = Object.FindFirstObjectByType<ShapePlacementPuzzle>();
            }

            if (puzzle != null)
            {
                puzzle.NotifyPlaced(acceptedType);
            }
            else
            {
                // Fallback direct notification to GameManager
                if (GameManager.Instance != null)
                {
                    Debug.Log($"[ShapeSocket] Placed {acceptedType}. Fallback notifying GameManager.");
                    GameManager.Instance.CompleteShapePuzzle();
                }
            }
        }
    }
}
