using UnityEngine;
using UnityEngine.Events;

namespace AREscapeRoom.Core
{
    public enum RoomState
    {
        MallExploration,
        TrappedInRoom,
        Escaped
    }

    public class EscapeRoomManager : MonoBehaviour
    {
        public static EscapeRoomManager Instance { get; private set; }

        [Header("State Settings")]
        [SerializeField] private RoomState currentState = RoomState.MallExploration;

        [Header("Events")]
        public UnityEvent OnRoomEntered;
        public UnityEvent OnDoorTrapped;
        public UnityEvent OnPasswordFailed;
        public UnityEvent OnPasswordSuccess;
        public UnityEvent OnEscaped;

        public RoomState CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetState(RoomState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[EscapeRoomManager] Room State Changed to: {currentState}");

            switch (currentState)
            {
                case RoomState.TrappedInRoom:
                    OnDoorTrapped?.Invoke();
                    break;
                case RoomState.Escaped:
                    OnEscaped?.Invoke();
                    break;
            }
        }

        public void NotifyRoomEntered()
        {
            OnRoomEntered?.Invoke();
            SetState(RoomState.TrappedInRoom);
        }

        public void NotifyPasswordAttempt(bool isCorrect)
        {
            if (isCorrect)
            {
                Debug.Log("[EscapeRoomManager] Password correct! Exit door unlocking...");
                OnPasswordSuccess?.Invoke();
                SetState(RoomState.Escaped);
            }
            else
            {
                Debug.Log("[EscapeRoomManager] Password incorrect! Try again.");
                OnPasswordFailed?.Invoke();
            }
        }
    }
}
