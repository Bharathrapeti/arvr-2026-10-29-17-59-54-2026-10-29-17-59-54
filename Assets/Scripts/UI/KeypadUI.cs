using System;
using UnityEngine;
using UnityEngine.UI;

namespace AREscapeRoom.UI
{
    public class KeypadUI : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private GameObject keypadCanvas;
        [SerializeField] private Text displayText;
        [SerializeField] private Text statusText;
        [SerializeField] private Image displayBackground;

        [Header("Keypad Settings")]
        [SerializeField] private int maxPasswordLength = 6;
        [SerializeField] private bool maskPassword = false;

        [Header("Feedback Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color successColor = new Color(0.3f, 1f, 0.4f);

        private string currentInput = "";
        private Action<string> onSubmitCallback;
        private static KeypadUI activeInstance;

        public static bool AnyVisible => activeInstance != null && activeInstance.IsKeypadVisible;

        public bool IsKeypadVisible
        {
            get
            {
                if (keypadCanvas != null) return keypadCanvas.activeSelf;
                return gameObject.activeSelf;
            }
        }

        private void Awake()
        {
            activeInstance = this;
            ApplyLegacyFont();
            BindButtonListeners();
        }

        private void ApplyLegacyFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) return;

            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].font == null || texts[i].font.name == "Arial")
                {
                    texts[i].font = font;
                }
            }
        }

        private void Start()
        {
            HideKeypad();
        }

        private void Update()
        {
            if (!IsKeypadVisible) return;

            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    OnPressDigit(i.ToString());
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnPressSubmit();
            }

            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
            {
                // Backspace logic: remove last character
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    UpdateDisplay();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideKeypad();
            }
        }

        private bool isSubmitting = false;

        public void BindButtonListeners()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn == null) continue;
                string btnName = btn.gameObject.name;
                btn.onClick.RemoveAllListeners();

                // Ensure child text components don't swallow pointer events
                Text[] childTexts = btn.GetComponentsInChildren<Text>(true);
                foreach (Text t in childTexts)
                {
                    if (t != null) t.raycastTarget = false;
                }

                if (btnName == "Btn_CLR" || btnName.Equals("CLR", StringComparison.OrdinalIgnoreCase))
                {
                    btn.onClick.AddListener(OnPressClear);
                }
                else if (btnName == "Btn_ENT" || btnName.Equals("ENT", StringComparison.OrdinalIgnoreCase) || btnName.Equals("Enter", StringComparison.OrdinalIgnoreCase))
                {
                    btn.onClick.AddListener(OnPressSubmit);
                }
                else if (btnName == "Btn_Close" || btnName.Equals("Close", StringComparison.OrdinalIgnoreCase) || btnName.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    btn.onClick.AddListener(HideKeypad);
                }
                else if (btnName.StartsWith("Btn_"))
                {
                    string digit = btnName.Substring(4);
                    btn.onClick.AddListener(() => OnPressDigit(digit));
                }
                else if (childTexts.Length > 0 && int.TryParse(childTexts[0].text, out int digitVal))
                {
                    string digit = digitVal.ToString();
                    btn.onClick.AddListener(() => OnPressDigit(digit));
                }
            }
        }

        public void ShowKeypad(Action<string> onSubmit)
        {
            onSubmitCallback = onSubmit;
            currentInput = "";
            isSubmitting = false;

            if (keypadCanvas != null)
            {
                keypadCanvas.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            BindButtonListeners();
            ResetKeypadDisplay();

            // Unlock and reveal mouse cursor for UI clicking
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HideKeypad()
        {
            isSubmitting = false;
            if (keypadCanvas != null)
            {
                keypadCanvas.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnPressDigit(string digit)
        {
            if (currentInput.Length >= maxPasswordLength) return;

            currentInput += digit;
            Debug.Log($"[KeypadUI] Pressed digit: '{digit}'. Current Input: '{currentInput}'");
            UpdateDisplay();
        }

        public void OnPressClear()
        {
            currentInput = "";
            Debug.Log("[KeypadUI] Cleared password input.");
            ResetKeypadDisplay();
        }

        public void OnPressSubmit()
        {
            if (isSubmitting) return;

            if (string.IsNullOrEmpty(currentInput))
            {
                SetStatus("PLEASE ENTER A PASSWORD", errorColor);
                return;
            }

            isSubmitting = true;
            Debug.Log($"[KeypadUI] Submitting password: '{currentInput}'");
            onSubmitCallback?.Invoke(currentInput);
            Invoke(nameof(ResetSubmitFlag), 0.3f);
        }

        private void ResetSubmitFlag()
        {
            isSubmitting = false;
        }

        public void ShowError(string message = "INCORRECT PASSWORD! TRY AGAIN")
        {
            SetStatus(message, errorColor);
            currentInput = "";
            UpdateDisplay();
        }

        public void ShowSuccess(string message = "ACCESS GRANTED! DOOR UNLOCKED")
        {
            SetStatus(message, successColor);
            Invoke(nameof(HideKeypad), 1.5f);
        }

        private void UpdateDisplay()
        {
            if (displayText != null)
            {
                if (string.IsNullOrEmpty(currentInput))
                {
                    displayText.text = "____";
                }
                else if (maskPassword)
                {
                    displayText.text = new string('*', currentInput.Length);
                }
                else
                {
                    displayText.text = currentInput;
                }
            }
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusText != null)
            {
                statusText.text = msg;
                statusText.color = color;
            }
            if (displayBackground != null)
            {
                displayBackground.color = color;
            }
        }

        private void ResetKeypadDisplay()
        {
            SetStatus("ENTER PASSWORD", normalColor);
            UpdateDisplay();
        }
    }
}
