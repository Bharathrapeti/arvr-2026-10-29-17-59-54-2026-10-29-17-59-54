using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Puzzles
{
    public class ColorSequencePuzzle : MonoBehaviour
    {
        [SerializeField] private string[] sequence = { "Red", "Green", "Yellow", "Blue" };

        private int index;
        private bool complete;

        public int Progress => index;
        public int Total => sequence != null ? sequence.Length : 0;

        public void Configure(string[] colors)
        {
            if (colors != null && colors.Length > 0)
            {
                sequence = colors;
            }

            index = 0;
            complete = false;
        }

        public bool Press(string colorName)
        {
            if (complete)
                return true;

            GameManager gm = GameManager.Instance;

            if (gm != null)
            {
                if (!gm.IsGameplayActive)
                    return false;

                if (gm.ColorSequence != null &&
                    gm.ColorSequence.Length > 0)
                {
                    sequence = gm.ColorSequence;
                }
            }

            if (sequence == null || sequence.Length == 0)
                return false;

            string expected = sequence[index];

            if (string.Equals(
                colorName,
                expected,
                System.StringComparison.OrdinalIgnoreCase))
            {
                index++;

                Debug.Log(
                    $"[ColorSequence] Correct {colorName}. {index}/{sequence.Length}"
                );

                if (index >= sequence.Length)
                {
                    complete = true;
                    gm?.CompleteColorPuzzle();
                }

                return true;
            }

            index = 0;

            Debug.Log(
                $"[ColorSequence] Incorrect {colorName}. Reset 0/{sequence.Length}"
            );

            return false;
        }
    }
}