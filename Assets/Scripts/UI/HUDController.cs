using UnityEngine;
using UnityEngine.UI;
using AREscapeRoom.Core;

namespace AREscapeRoom.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Text keyText;
        [SerializeField] private Text gemText;
        [SerializeField] private Text promptText;
        private GameObject promptPanel;

        [Header("Pointer / Reticle")]
        [SerializeField] private GameObject pointerRoot;
        [SerializeField] private Image pointerDot;
        [SerializeField] private Image pointerRing;

        public static HUDController Instance { get; private set; }
        public string CurrentPrompt => promptText != null ? promptText.text : "";

        private void Awake()
        {
            Instance = this;
            if (timerText == null)
            {
                BuildHud();
            }
            EnsurePointer();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += Refresh;
            GameManager.OnTimerChanged += RefreshTimer;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= Refresh;
            GameManager.OnTimerChanged -= RefreshTimer;
        }

        private void Start()
        {
            EnsurePointer();
            Refresh();
            RefreshTimer();
        }

        public void SetPrompt(string message)
        {
            if (promptText == null) return;
            bool show = !string.IsNullOrEmpty(message);
            promptText.text = message ?? "";
            if (promptPanel != null) promptPanel.SetActive(show);
            else promptText.gameObject.SetActive(show);
        }

        private void Refresh()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
            if (keyText != null) keyText.text = gm.HasKey ? "KEY  1/1" : "KEY  0/1";
            if (gemText != null) gemText.text = $"GEMS  {gm.GemsCollected}/{gm.TotalGems}";
        }

        private void RefreshTimer()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || timerText == null) return;
            timerText.text = $"TIME  {gm.TimerDisplay}";
        }

        private void BuildHud()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                gameObject.AddComponent<GraphicRaycaster>();
            }

            GameObject left = UiFactory.Panel(transform, "TimerPanel",
                new Vector2(0.02f, 0.88f), new Vector2(0.22f, 0.97f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
            timerText = UiFactory.Label(left.transform, "TimerText", "TIME  10:00",
                Vector2.zero, Vector2.one, 28, TextAnchor.MiddleCenter);

            GameObject right = UiFactory.Panel(transform, "InventoryPanel",
                new Vector2(0.78f, 0.82f), new Vector2(0.98f, 0.97f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
            keyText = UiFactory.Label(right.transform, "KeyText", "KEY  0/1",
                new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.95f), 22, TextAnchor.MiddleLeft);
            gemText = UiFactory.Label(right.transform, "GemText", "GEMS  0/5",
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.5f), 22, TextAnchor.MiddleLeft);

            promptPanel = UiFactory.Panel(transform, "PromptPanel",
                new Vector2(0.35f, 0.12f), new Vector2(0.65f, 0.2f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.45f));
            promptText = UiFactory.Label(promptPanel.transform, "PromptText", "",
                Vector2.zero, Vector2.one, 24, TextAnchor.MiddleCenter);
            promptPanel.SetActive(false);

            BuildPointer();
        }

        public void EnsurePointer()
        {
            if (pointerRoot != null) return;
            BuildPointer();
        }

        public void SetPointerHover(bool hovering)
        {
            if (pointerDot == null || pointerRing == null) return;

            if (hovering)
            {
                pointerDot.color = new Color(0.2f, 1f, 0.85f, 1f);
                pointerDot.rectTransform.sizeDelta = new Vector2(8f, 8f);
                pointerRing.color = new Color(0.2f, 1f, 0.85f, 0.9f);
                pointerRing.rectTransform.sizeDelta = new Vector2(28f, 28f);
            }
            else
            {
                pointerDot.color = new Color(1f, 1f, 1f, 0.85f);
                pointerDot.rectTransform.sizeDelta = new Vector2(6f, 6f);
                pointerRing.color = new Color(1f, 1f, 1f, 0.4f);
                pointerRing.rectTransform.sizeDelta = new Vector2(20f, 20f);
            }
        }

        public void SetPointerVisible(bool visible)
        {
            if (pointerRoot != null)
            {
                pointerRoot.SetActive(visible);
            }
        }

        private void BuildPointer()
        {
            if (pointerRoot != null) return;

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
            }

            pointerRoot = new GameObject("Pointer_Crosshair", typeof(RectTransform));
            pointerRoot.transform.SetParent(transform, false);
            RectTransform ptrRt = pointerRoot.GetComponent<RectTransform>();
            ptrRt.anchorMin = new Vector2(0.5f, 0.5f);
            ptrRt.anchorMax = new Vector2(0.5f, 0.5f);
            ptrRt.pivot = new Vector2(0.5f, 0.5f);
            ptrRt.anchoredPosition = Vector2.zero;
            ptrRt.sizeDelta = new Vector2(32f, 32f);

            // Outer targeting ring
            GameObject ringGo = new GameObject("PointerRing", typeof(RectTransform));
            ringGo.transform.SetParent(pointerRoot.transform, false);
            RectTransform ringRt = ringGo.GetComponent<RectTransform>();
            ringRt.anchorMin = new Vector2(0.5f, 0.5f);
            ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            ringRt.pivot = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = Vector2.zero;
            ringRt.sizeDelta = new Vector2(20f, 20f);
            pointerRing = ringGo.AddComponent<Image>();
            pointerRing.sprite = CreateRingSprite(32, 2.5f);
            pointerRing.color = new Color(1f, 1f, 1f, 0.4f);
            pointerRing.raycastTarget = false;

            // Center targeting dot
            GameObject dotGo = new GameObject("PointerDot", typeof(RectTransform));
            dotGo.transform.SetParent(pointerRoot.transform, false);
            RectTransform dotRt = dotGo.GetComponent<RectTransform>();
            dotRt.anchorMin = new Vector2(0.5f, 0.5f);
            dotRt.anchorMax = new Vector2(0.5f, 0.5f);
            dotRt.pivot = new Vector2(0.5f, 0.5f);
            dotRt.anchoredPosition = Vector2.zero;
            dotRt.sizeDelta = new Vector2(6f, 6f);
            pointerDot = dotGo.AddComponent<Image>();
            pointerDot.sprite = CreateCircleSprite(16);
            pointerDot.color = new Color(1f, 1f, 1f, 0.85f);
            pointerDot.raycastTarget = false;
        }

        private static Sprite CreateCircleSprite(int diameter)
        {
            Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float r = diameter * 0.5f;
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    float alpha = Mathf.Clamp01(r - d + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateRingSprite(int diameter, float thickness)
        {
            Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float outerR = diameter * 0.5f;
            float innerR = outerR - thickness;
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(outerR, outerR));
                    float outerAlpha = Mathf.Clamp01(outerR - d + 0.5f);
                    float innerAlpha = Mathf.Clamp01(d - innerR + 0.5f);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f));
        }
    }
}
