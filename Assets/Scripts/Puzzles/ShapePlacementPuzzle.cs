using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Puzzles
{
    public class ShapePlacementPuzzle : MonoBehaviour
    {
        [SerializeField] private int placedCount;
        [SerializeField] private int requiredCount = 4;

        public int PlacedCount => placedCount;

        public void NotifyPlaced(ShapeType type)
        {
            placedCount++;

            Debug.Log($"[ShapePuzzle] Placed {type}. {placedCount}/{requiredCount}");

            if (placedCount >= requiredCount && GameManager.Instance != null)
            {
                GameManager.Instance.CompleteShapePuzzle();
            }
        }
    }
}