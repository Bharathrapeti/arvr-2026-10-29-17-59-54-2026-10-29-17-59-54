using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AREscapeRoom.Core;

namespace AREscapeRoom.UI
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Room 1 Objective & Hint")]
        public TextMeshProUGUI roomObjectiveText;
        public Button hintButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            FindUIReferencesIfNull();
        }

        private void Start()
        {
            FindUIReferencesIfNull();

            // Hide the hintButton completely by default on game launch
            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(false);
            }
        }

        public void FindUIReferencesIfNull()
        {
            if (roomObjectiveText == null)
            {
                GameObject obj = GameObject.Find("RoomObjectiveText");
                if (obj != null)
                {
                    roomObjectiveText = obj.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    TextMeshProUGUI[] texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
                    for (int i = 0; i < texts.Length; i++)
                    {
                        if (texts[i] != null && texts[i].gameObject.name == "RoomObjectiveText")
                        {
                            roomObjectiveText = texts[i];
                            break;
                        }
                    }
                }
            }

            if (hintButton == null)
            {
                GameObject obj = GameObject.Find("HintButton");
                if (obj != null)
                {
                    hintButton = obj.GetComponent<Button>();
                }
                else
                {
                    Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (buttons[i] != null && buttons[i].gameObject.name == "HintButton")
                        {
                            hintButton = buttons[i];
                            break;
                        }
                    }
                }
            }
        }

        public void SetupRoom1Objective()
        {
            FindUIReferencesIfNull();

            if (roomObjectiveText != null)
            {
                roomObjectiveText.text = "You are stuck in Room 1. Find the key hidden somewhere in this room.";
            }

            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(true);
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(OnHintButtonClicked);
            }
        }

        public void OnHintButtonClicked()
        {
            if (roomObjectiveText != null)
            {
                roomObjectiveText.text = "💡 Hint: Something useful may be hiding where darkness meets the wall.";
            }
        }

        public void OnKeyDiscovered()
        {
            FindUIReferencesIfNull();

            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(false);
            }

            if (roomObjectiveText != null)
            {
                roomObjectiveText.text = "Door Unlocked!";
            }
        }
    }
}
