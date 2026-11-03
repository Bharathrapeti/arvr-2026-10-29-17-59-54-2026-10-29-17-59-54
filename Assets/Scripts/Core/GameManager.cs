using System;
using UnityEngine;

namespace AREscapeRoom.Core
{
    public enum RoomId
    {
        Mall = 0,
        Room1 = 1,
        Room2 = 2,
        Room3 = 3,
        Room4 = 4,
        Room5 = 5,
        Room6 = 6
    }

    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action OnStateChanged;
        public static event Action OnTimerChanged;
        public static event Action OnGameWon;
        public static event Action OnGameOver;

        [Header("Timer")]
        [SerializeField] private float startingTimeSeconds = 600f;

        [Header("Puzzle Config")]
        [SerializeField] private string cipherPassword = "4729";
        [SerializeField] private int totalGems = 5;

        [SerializeField] private string[] colorSequence =
        {
            "Red",
            "Green",
            "Yellow",
            "Blue"
        };

        [Header("State")]
        [SerializeField] private RoomId currentRoom = RoomId.Mall;

        [SerializeField] private bool hasKey;
        [SerializeField] private int gemsCollected;

        [SerializeField] private bool cipherPuzzleCompleted;
        [SerializeField] private bool colorPuzzleCompleted;
        [SerializeField] private bool shapePuzzleCompleted;

        [SerializeField] private bool room1Completed;
        [SerializeField] private bool room2Completed;
        [SerializeField] private bool room3Completed;
        [SerializeField] private bool room4Completed;
        [SerializeField] private bool room5Completed;

        [SerializeField] private bool gameWon;
        [SerializeField] private bool gameOver;

        [SerializeField] private float remainingTime;

        private bool timerRunning;

        public RoomId CurrentRoom => currentRoom;

        public bool HasKey => hasKey;

        public int GemsCollected => gemsCollected;

        public int TotalGems => totalGems;

        public bool CipherPuzzleCompleted =>
            cipherPuzzleCompleted;

        public bool ColorPuzzleCompleted =>
            colorPuzzleCompleted;

        public bool ShapePuzzleCompleted =>
            shapePuzzleCompleted;

        public bool Room1Completed =>
            room1Completed;

        public bool Room2Completed =>
            room2Completed;

        public bool Room3Completed =>
            room3Completed;

        public bool Room4Completed =>
            room4Completed;

        public bool Room5Completed =>
            room5Completed;

        public bool GameWon =>
            gameWon;

        public bool GameOver =>
            gameOver;

        public float RemainingTime =>
            remainingTime;

        public string CipherPassword =>
            cipherPassword;

        public string[] ColorSequence =>
            colorSequence;

        public bool IsGameplayActive =>
            !gameWon && !gameOver;

        public bool IsPlayable =>
            IsGameplayActive;

        public string TimerDisplay
        {
            get
            {
                int t =
                    Mathf.Max(
                        0,
                        Mathf.CeilToInt(remainingTime)
                    );

                int minutes = t / 60;
                int seconds = t % 60;

                return $"{minutes:00}:{seconds:00}";
            }
        }

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            if (string.IsNullOrEmpty(cipherPassword))
            {
                cipherPassword = "4729";
            }

            remainingTime =
                startingTimeSeconds;
        }

        private void Start()
        {
            timerRunning = true;

            NotifyState();
            NotifyTimer();
        }

        private void Update()
        {
            if (!timerRunning ||
                gameWon ||
                gameOver)
            {
                return;
            }

            remainingTime -=
                Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                TriggerGameOver();
            }

            NotifyTimer();
        }

        public void Configure(
            float timeSeconds,
            string password,
            int gemCount)
        {
            if (timeSeconds > 0f)
            {
                startingTimeSeconds =
                    timeSeconds;
            }

            /*
             * Always use the supplied password.
             * If anything invalid is supplied,
             * fall back to 4729.
             */
            if (!string.IsNullOrWhiteSpace(password))
            {
                cipherPassword =
                    password.Trim();
            }
            else
            {
                cipherPassword = "4729";
            }

            if (gemCount > 0)
            {
                totalGems = gemCount;
            }

            if (!timerRunning &&
                remainingTime <= 0f)
            {
                remainingTime =
                    startingTimeSeconds;
            }

            Debug.Log(
                $"[GameManager] Password configured: {cipherPassword}"
            );
        }

        public void SetRoom(RoomId room)
        {
            if (currentRoom == room)
            {
                return;
            }

            int next = (int)room;
            int current = (int)currentRoom;

            // Automatically catch up currentRoom if room 2 puzzle is already done
            if (room2Completed && currentRoom < RoomId.Room2)
            {
                currentRoom = RoomId.Room2;
                current = (int)currentRoom;
            }

            /*
             * Do not allow skipping rooms unless the preceding room puzzle is complete.
             */
            if (next > current + 1)
            {
                bool allowedByProgress = false;
                if (room == RoomId.Room3 && room2Completed) allowedByProgress = true;

                if (!allowedByProgress)
                {
                    Debug.Log(
                        $"[GameManager] Blocked skip from {currentRoom} to {room}"
                    );
                    return;
                }
            }

            currentRoom = room;

            Debug.Log(
                $"[GameManager] Current room: {currentRoom}"
            );

            NotifyState();
        }

        public void CollectKey()
        {
            if (hasKey ||
                !IsGameplayActive)
            {
                return;
            }

            hasKey = true;
            room1Completed = true;

            Debug.Log(
                "[GameManager] Key collected. KEY 1/1"
            );

            NotifyState();
        }

        public void CollectGem()
        {
            if (!IsGameplayActive ||
                gemsCollected >= totalGems)
            {
                return;
            }

            gemsCollected++;

            Debug.Log(
                $"[GameManager] Gem collected. GEMS {gemsCollected}/{totalGems}"
            );

            if (gemsCollected >= totalGems)
            {
                room3Completed = true;
            }

            NotifyState();
        }

        public bool TrySubmitCipher(string input)
        {
            if (!IsGameplayActive)
            {
                return false;
            }

            /*
             * Clean the keypad input.
             *
             * This prevents accidental spaces,
             * line breaks, or other invisible
             * characters from making 4729 fail.
             */
            string entered =
                string.IsNullOrWhiteSpace(input)
                    ? ""
                    : input.Trim();

            string expected =
                string.IsNullOrWhiteSpace(cipherPassword)
                    ? "4729"
                    : cipherPassword.Trim();

            /*
             * Force the correct Room 2 password.
             */
            if (string.IsNullOrEmpty(expected))
            {
                expected = "4729";
            }

            bool correct =
                string.Equals(
                    entered,
                    expected,
                    StringComparison.Ordinal
                );

            Debug.Log(
                $"[GameManager] Cipher entered='{entered}' expected='{expected}' correct={correct}"
            );

            if (correct)
            {
                cipherPuzzleCompleted = true;
                room2Completed = true;
                if (currentRoom < RoomId.Room2)
                {
                    currentRoom = RoomId.Room2;
                }

                Debug.Log(
                    "[GameManager] Cipher puzzle complete. ROOM 2 COMPLETE."
                );

                NotifyState();
            }
            else
            {
                Debug.Log(
                    "[GameManager] Incorrect cipher code."
                );
            }

            return correct;
        }

        public void CompleteColorPuzzle()
        {
            if (!IsGameplayActive ||
                colorPuzzleCompleted)
            {
                return;
            }

            colorPuzzleCompleted = true;
            room4Completed = true;

            Debug.Log(
                "[GameManager] Color sequence complete."
            );

            NotifyState();
        }

        public void CompleteShapePuzzle()
        {
            if (!IsGameplayActive ||
                shapePuzzleCompleted)
            {
                return;
            }

            shapePuzzleCompleted = true;
            room5Completed = true;

            Debug.Log(
                "[GameManager] Shape puzzle complete."
            );

            NotifyState();
        }

        public void NotifyShapePlaced()
        {
            CompleteShapePuzzle();
        }

        public void NotifyDoorOpened()
        {
            NotifyState();
        }

        public bool Meets(
            DoorRequirement requirement)
        {
            switch (requirement)
            {
                case DoorRequirement.None:
                    return true;

                case DoorRequirement.RequiresKey:
                    return hasKey;

                case DoorRequirement.RequiresPassword:
                    return cipherPuzzleCompleted;

                case DoorRequirement.RequiresGems:
                    return gemsCollected >= totalGems;

                case DoorRequirement.RequiresColorSequence:
                    return colorPuzzleCompleted;

                case DoorRequirement.RequiresShapes:
                    return shapePuzzleCompleted;

                case DoorRequirement.RequiresFinalValidation:

                    return hasKey &&
                           gemsCollected >= totalGems &&
                           cipherPuzzleCompleted &&
                           colorPuzzleCompleted &&
                           shapePuzzleCompleted &&
                           room1Completed;

                default:
                    return false;
            }
        }

        public string GetLockedReason(
            DoorRequirement requirement)
        {
            return MissingRequirementMessage(
                requirement
            );
        }

        public string MissingRequirementMessage(
            DoorRequirement requirement)
        {
            switch (requirement)
            {
                case DoorRequirement.RequiresKey:

                    return hasKey
                        ? ""
                        : "Key Required";

                case DoorRequirement.RequiresPassword:

                    return cipherPuzzleCompleted
                        ? ""
                        : "Password Required";

                case DoorRequirement.RequiresGems:

                    return gemsCollected >= totalGems
                        ? ""
                        : $"{totalGems} Gems Required";

                case DoorRequirement.RequiresColorSequence:

                    return colorPuzzleCompleted
                        ? ""
                        : "Color Sequence Incomplete";

                case DoorRequirement.RequiresShapes:

                    return shapePuzzleCompleted
                        ? ""
                        : "Place All 4 Shapes";

                case DoorRequirement.RequiresFinalValidation:

                    if (!hasKey)
                        return "Key Required";

                    if (gemsCollected < totalGems)
                        return $"{totalGems} Gems Required";

                    if (!cipherPuzzleCompleted)
                        return "Cipher Puzzle Incomplete";

                    if (!colorPuzzleCompleted)
                        return "Color Puzzle Incomplete";

                    if (!shapePuzzleCompleted)
                        return "Shape Puzzle Incomplete";

                    return "";

                default:
                    return "Locked";
            }
        }

        public void TriggerVictory()
        {
            if (gameWon ||
                gameOver)
            {
                return;
            }

            gameWon = true;
            timerRunning = false;

            Debug.Log(
                "[GameManager] YOU ESCAPED!"
            );

            NotifyState();

            OnGameWon?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (gameWon ||
                gameOver)
            {
                return;
            }

            gameOver = true;
            timerRunning = false;
            remainingTime = 0f;

            Debug.Log(
                "[GameManager] TIME UP / GAME OVER"
            );

            NotifyTimer();
            NotifyState();

            OnGameOver?.Invoke();
        }

        public void SaveGame()
        {
            PlayerPrefs.SetInt(
                "er_room",
                (int)currentRoom
            );

            PlayerPrefs.SetInt(
                "er_key",
                hasKey ? 1 : 0
            );

            PlayerPrefs.SetInt(
                "er_gems",
                gemsCollected
            );

            PlayerPrefs.SetInt(
                "er_cipher",
                cipherPuzzleCompleted ? 1 : 0
            );

            PlayerPrefs.SetInt(
                "er_color",
                colorPuzzleCompleted ? 1 : 0
            );

            PlayerPrefs.SetInt(
                "er_shape",
                shapePuzzleCompleted ? 1 : 0
            );

            PlayerPrefs.SetFloat(
                "er_time",
                remainingTime
            );

            PlayerPrefs.Save();

            Debug.Log(
                "[GameManager] Game saved."
            );
        }

        public void LoadGame()
        {
            currentRoom =
                (RoomId)PlayerPrefs.GetInt(
                    "er_room",
                    0
                );

            hasKey =
                PlayerPrefs.GetInt(
                    "er_key",
                    0
                ) == 1;

            gemsCollected =
                PlayerPrefs.GetInt(
                    "er_gems",
                    0
                );

            cipherPuzzleCompleted =
                PlayerPrefs.GetInt(
                    "er_cipher",
                    0
                ) == 1;

            colorPuzzleCompleted =
                PlayerPrefs.GetInt(
                    "er_color",
                    0
                ) == 1;

            shapePuzzleCompleted =
                PlayerPrefs.GetInt(
                    "er_shape",
                    0
                ) == 1;

            remainingTime =
                PlayerPrefs.GetFloat(
                    "er_time",
                    startingTimeSeconds
                );

            room1Completed =
                hasKey;

            room2Completed =
                cipherPuzzleCompleted;

            room3Completed =
                gemsCollected >= totalGems;

            room4Completed =
                colorPuzzleCompleted;

            room5Completed =
                shapePuzzleCompleted;

            NotifyState();
            NotifyTimer();

            Debug.Log(
                "[GameManager] Game loaded."
            );
        }

        public void ResetGame()
        {
            currentRoom =
                RoomId.Mall;

            hasKey = false;
            gemsCollected = 0;

            cipherPuzzleCompleted = false;
            colorPuzzleCompleted = false;
            shapePuzzleCompleted = false;

            room1Completed = false;
            room2Completed = false;
            room3Completed = false;
            room4Completed = false;
            room5Completed = false;

            gameWon = false;
            gameOver = false;

            remainingTime =
                startingTimeSeconds;

            timerRunning = true;

            /*
             * Always restore the correct password.
             */
            cipherPassword = "4729";

            PlayerPrefs.DeleteKey("er_room");
            PlayerPrefs.DeleteKey("er_key");
            PlayerPrefs.DeleteKey("er_gems");
            PlayerPrefs.DeleteKey("er_cipher");
            PlayerPrefs.DeleteKey("er_color");
            PlayerPrefs.DeleteKey("er_shape");
            PlayerPrefs.DeleteKey("er_time");

            PlayerPrefs.Save();

            NotifyState();
            NotifyTimer();

            Debug.Log(
                "[GameManager] Game reset. Password = 4729"
            );
        }

        private void NotifyState()
        {
            OnStateChanged?.Invoke();
        }

        private void NotifyTimer()
        {
            OnTimerChanged?.Invoke();
        }
    }
}