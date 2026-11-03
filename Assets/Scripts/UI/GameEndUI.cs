using UnityEngine;
using UnityEngine.UI;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.UI
{
    public class GameEndUI : MonoBehaviour
    {
        private GameObject winPanel;
        private GameObject losePanel;
        private Text winTimeText;

        private void OnEnable()
        {
            GameManager.OnGameWon += ShowWin;
            GameManager.OnGameOver += ShowLose;
        }

        private void OnDisable()
        {
            GameManager.OnGameWon -= ShowWin;
            GameManager.OnGameOver -= ShowLose;
        }

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                gameObject.AddComponent<GraphicRaycaster>();
            }

            winPanel = UiFactory.Panel(transform, "VictoryPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.75f));
            UiFactory.Label(winPanel.transform, "Title", "YOU ESCAPED!", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f), 64, TextAnchor.MiddleCenter);
            UiFactory.Label(winPanel.transform, "Sub", "Congratulations!", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.55f), 32, TextAnchor.MiddleCenter);
            winTimeText = UiFactory.Label(winPanel.transform, "Time", "Time Remaining: 00:00", new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.42f), 26, TextAnchor.MiddleCenter);
            winPanel.SetActive(false);

            losePanel = UiFactory.Panel(transform, "GameOverPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.15f, 0f, 0f, 0.8f));
            UiFactory.Label(losePanel.transform, "Title", "TIME UP", new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.7f), 64, TextAnchor.MiddleCenter);
            UiFactory.Label(losePanel.transform, "Sub", "Game Over", new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.5f), 32, TextAnchor.MiddleCenter);
            losePanel.SetActive(false);
        }

        private void ShowWin()
        {
            if (winPanel != null)
            {
                if (GameManager.Instance != null && winTimeText != null)
                {
                    winTimeText.text = $"Time Remaining: {GameManager.Instance.TimerDisplay}";
                }
                winPanel.SetActive(true);
            }
            UnlockCursorAndFreeze();
        }

        private void ShowLose()
        {
            if (losePanel != null) losePanel.SetActive(true);
            UnlockCursorAndFreeze();
        }

        private static void UnlockCursorAndFreeze()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetControlEnabled(false);
        }
    }
}
