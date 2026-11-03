using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AREscapeRoom.Doors
{
    public class DoorController : MonoBehaviour
    {
        public enum DoorType
        {
            HingeRotation,
            SlidingHorizontal
        }

        public enum DoorState
        {
            Closed,
            Opening,
            Open,
            Closing,
            Locked
        }

        [Header("Door Configuration")]
        [SerializeField] private string doorName = "Door";
        [SerializeField] private DoorType doorType = DoorType.SlidingHorizontal;
        [SerializeField] private Transform doorTransform;
        [SerializeField] private Transform secondaryTransform;

        [Header("Hinge Settings")]
        [SerializeField] private float closedYRotation = 0f;
        [SerializeField] private float openYRotation = 90f;
        [SerializeField] private float secondaryClosedYRotation = 0f;
        [SerializeField] private float secondaryOpenYRotation = -90f;

        [Header("Sliding Settings")]
        [SerializeField] private Vector3 closedLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 openLocalPosition = new Vector3(-4f, 0f, 0f);
        [SerializeField] private Vector3 secondaryClosedPos = Vector3.zero;
        [SerializeField] private Vector3 secondaryOpenPos = new Vector3(1.5f, 0f, 0f);

        [Header("Animation Settings")]
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private bool startsLocked = false;
        [SerializeField] private bool startsOpen = false;

        [Header("Events")]
        public UnityEvent OnDoorOpened;
        public UnityEvent OnDoorClosed;
        public UnityEvent OnDoorLocked;
        public UnityEvent OnDoorUnlocked;
        public UnityEvent OnDoorDenied;

        private DoorState currentState = DoorState.Closed;
        private bool isLocked;
        private Coroutine activeAnimationRoutine;
        private Collider[] doorColliders;

        public string DoorName => doorName;
        public DoorState CurrentState => currentState;
        public bool IsLocked => isLocked;
        public bool IsOpen => currentState == DoorState.Open;

        public void ConfigureSliding(
            Vector3 closedPos,
            Vector3 openPos,
            bool locked,
            string name)
        {
            doorType = DoorType.SlidingHorizontal;
            closedLocalPosition = closedPos;
            openLocalPosition = openPos;
            startsLocked = locked;
            doorName = name;

            if (doorTransform == null)
                doorTransform = transform;

            secondaryTransform = null;
        }

        public void ConfigureHinge(
            Transform rotatingTransform,
            float closedY,
            float openY,
            bool locked,
            string name)
        {
            doorType = DoorType.HingeRotation;
            doorTransform = rotatingTransform != null ? rotatingTransform : transform;
            closedYRotation = closedY;
            openYRotation = openY;
            startsLocked = locked;
            doorName = name;
            secondaryTransform = null;
            if (locked)
            {
                isLocked = true;
                currentState = DoorState.Locked;
                ApplyStateImmediate(0f);
            }
            RefreshPassageColliders();
            SetPassageOpen(!locked && startsOpen);
        }

        public void ConfigureDoubleHinge(
            Transform primaryTransform,
            Transform secondaryLeaf,
            float closedY,
            float openPrimaryY,
            float openSecondaryY,
            bool locked,
            string name)
        {
            doorType = DoorType.HingeRotation;
            doorTransform = primaryTransform != null ? primaryTransform : transform;
            secondaryTransform = secondaryLeaf;
            closedYRotation = closedY;
            openYRotation = openPrimaryY;
            secondaryClosedYRotation = closedY;
            secondaryOpenYRotation = openSecondaryY;
            startsLocked = locked;
            doorName = name;
            if (locked)
            {
                isLocked = true;
                currentState = DoorState.Locked;
                ApplyStateImmediate(0f);
            }
            RefreshPassageColliders();
            SetPassageOpen(!locked && startsOpen);
        }

        private void Awake()
        {
            if (doorTransform == null || doorTransform == transform)
            {
                Transform childDoor = transform.Find("Door");
                if (childDoor != null)
                {
                    doorTransform = childDoor;
                    doorType = DoorType.HingeRotation;
                    closedYRotation = 0f;
                    openYRotation = 90f;
                }
                else
                {
                    doorTransform = transform;
                }
            }

            if (secondaryTransform == doorTransform)
                secondaryTransform = null;

            if (doorTransform != null && doorTransform != transform)
            {
                // Disable conflicting third-party Door component on child door leaf
                MonoBehaviour[] mbs = doorTransform.GetComponents<MonoBehaviour>();
                for (int i = 0; i < mbs.Length; i++)
                {
                    if (mbs[i] != null && mbs[i].GetType().Name == "Door")
                    {
                        mbs[i].enabled = false;
                    }
                }
            }

            if (closedLocalPosition == Vector3.zero && doorTransform != null)
                closedLocalPosition = doorTransform.localPosition;

            bool isExit = name.Contains("Exit") || doorName.Contains("Exit");
            bool isEntrance = !isExit && (doorName == "Entrance" || name == "EntranceDoor_Sliding" || name == "Door_5_Brown" || name.Contains("Entrance"));

            if (isExit)
            {
                doorName = "Room 1 Exit";
                doorType = DoorType.HingeRotation;
                closedYRotation = 0f;
                openYRotation = 90f;
                startsLocked = true;
                isLocked = true;
                startsOpen = false;

                // Ensure exit door does not have accidental MallEntranceProximity
                MallEntranceProximity prox = GetComponent<MallEntranceProximity>();
                if (prox != null)
                {
                    Destroy(prox);
                }

                // Ensure solid blocking BoxCollider on root ExitDoor_Keypad to physically stop the player
                BoxCollider box = GetComponent<BoxCollider>();
                if (box == null)
                    box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1.1f, 2.15f, 0.4f);
                box.center = new Vector3(0f, 1.075f, 0f);
                box.isTrigger = false;
                box.enabled = true;
            }
            else if (isEntrance)
            {
                doorName = "Entrance";
                doorType = DoorType.HingeRotation;
                closedYRotation = 0f;
                openYRotation = 90f;

                MallEntranceProximity prox = GetComponent<MallEntranceProximity>();
                if (prox == null)
                    prox = gameObject.AddComponent<MallEntranceProximity>();
                prox.Bind(this);

                GameObject sliding = GameObject.Find("EntranceDoor_Sliding");
                if (sliding != null && sliding != gameObject)
                {
                    sliding.SetActive(false);
                }
            }

            RefreshPassageColliders();
        }

        private void Start()
        {
            if (doorTransform == null)
                doorTransform = transform;

            if (secondaryTransform == doorTransform)
                secondaryTransform = null;

            bool isExit = name.Contains("Exit") || doorName.Contains("Exit");
            bool isEntrance = !isExit && (doorName == "Entrance" || name == "EntranceDoor_Sliding" || name == "Door_5_Brown" || name.Contains("Entrance"));

            if (isEntrance)
            {
                EntranceTrapTrigger trap = Object.FindFirstObjectByType<EntranceTrapTrigger>();
                if (trap != null)
                {
                    trap.Bind(this);
                }
            }
            else if (isExit)
            {
                isLocked = true;
                startsLocked = true;
                currentState = DoorState.Locked;
                ApplyStateImmediate(0f);
            }

            if (!isExit)
            {
                isLocked = startsLocked;

                if (startsOpen)
                {
                    currentState = DoorState.Open;
                    ApplyStateImmediate(1f);
                }
                else
                {
                    currentState = isLocked
                        ? DoorState.Locked
                        : DoorState.Closed;

                    ApplyStateImmediate(0f);
                }
            }

            RefreshPassageColliders();
            SetPassageOpen(currentState == DoorState.Open);
        }

        public void OpenDoor()
        {
            if (isLocked)
            {
                Debug.Log(
                    $"[DoorController] Cannot open '{doorName}': Door is locked!"
                );

                OnDoorDenied?.Invoke();
                return;
            }

            if (currentState == DoorState.Open ||
                currentState == DoorState.Opening)
            {
                return;
            }

            if (activeAnimationRoutine != null)
                StopCoroutine(activeAnimationRoutine);

            activeAnimationRoutine = StartCoroutine(
                AnimateDoor(
                    1f,
                    DoorState.Open,
                    OnDoorOpened
                )
            );
        }

        public void CloseDoor()
        {
            if (currentState == DoorState.Closed ||
                currentState == DoorState.Closing)
            {
                return;
            }

            if (activeAnimationRoutine != null)
                StopCoroutine(activeAnimationRoutine);

            activeAnimationRoutine = StartCoroutine(
                AnimateDoor(
                    0f,
                    DoorState.Closed,
                    OnDoorClosed
                )
            );
        }

        public void LockDoor()
        {
            isLocked = true;

            if (currentState == DoorState.Open ||
                currentState == DoorState.Opening)
            {
                CloseDoor();
                return;
            }

            if (currentState != DoorState.Closing)
                currentState = DoorState.Locked;

            SetPassageOpen(false);

            Debug.Log(
                $"[DoorController] '{doorName}' is now LOCKED."
            );

            OnDoorLocked?.Invoke();
        }

        public void UnlockDoor()
        {
            isLocked = false;

            if (currentState == DoorState.Locked)
                currentState = DoorState.Closed;

            Debug.Log(
                $"[DoorController] '{doorName}' is now UNLOCKED."
            );

            OnDoorUnlocked?.Invoke();
        }

        public void ToggleDoor()
        {
            if (IsOpen)
                CloseDoor();
            else
                OpenDoor();
        }

        private void ApplyStateImmediate(float progress)
        {
            if (doorTransform == null)
                doorTransform = transform;

            if (doorType == DoorType.HingeRotation)
            {
                float targetAngle = Mathf.Lerp(
                    closedYRotation,
                    openYRotation,
                    progress
                );

                doorTransform.localRotation =
                    Quaternion.Euler(0f, targetAngle, 0f);

                if (secondaryTransform != null && secondaryTransform != doorTransform)
                {
                    float targetSecAngle = Mathf.Lerp(
                        secondaryClosedYRotation,
                        secondaryOpenYRotation,
                        progress
                    );

                    secondaryTransform.localRotation =
                        Quaternion.Euler(0f, targetSecAngle, 0f);
                }
            }
            else
            {
                doorTransform.localPosition =
                    Vector3.Lerp(
                        closedLocalPosition,
                        openLocalPosition,
                        progress
                    );

                if (secondaryTransform != null &&
                    secondaryTransform != doorTransform)
                {
                    secondaryTransform.localPosition =
                        Vector3.Lerp(
                            secondaryClosedPos,
                            secondaryOpenPos,
                            progress
                        );
                }
            }

            SetPassageOpen(progress >= 0.45f);
        }

        private void RefreshPassageColliders()
        {
            System.Collections.Generic.List<Collider> cols = new System.Collections.Generic.List<Collider>();

            if (doorTransform != null)
            {
                Collider[] leafCols = doorTransform.GetComponents<Collider>();
                for (int i = 0; i < leafCols.Length; i++)
                {
                    if (leafCols[i] != null && !cols.Contains(leafCols[i]))
                        cols.Add(leafCols[i]);
                }
            }

            if (secondaryTransform != null && secondaryTransform != doorTransform)
            {
                Collider[] secCols = secondaryTransform.GetComponents<Collider>();
                for (int i = 0; i < secCols.Length; i++)
                {
                    if (secCols[i] != null && !cols.Contains(secCols[i]))
                        cols.Add(secCols[i]);
                }
            }

            Collider[] rootCols = GetComponents<Collider>();
            for (int i = 0; i < rootCols.Length; i++)
            {
                if (rootCols[i] != null && !cols.Contains(rootCols[i]))
                    cols.Add(rootCols[i]);
            }

            doorColliders = cols.ToArray();
        }

        private void SetPassageOpen(bool open)
        {
            if (doorColliders == null || doorColliders.Length == 0)
            {
                RefreshPassageColliders();
            }

            for (int i = 0; i < doorColliders.Length; i++)
            {
                if (doorColliders[i] != null)
                    doorColliders[i].enabled = !open;
            }
        }

        private IEnumerator AnimateDoor(
            float targetProgress,
            DoorState endState,
            UnityEvent completionEvent)
        {
            currentState =
                targetProgress > 0.5f
                    ? DoorState.Opening
                    : DoorState.Closing;

            if (targetProgress > 0.5f)
                SetPassageOpen(true);

            Vector3 startPos = doorTransform.localPosition;

            Vector3 targetPos = Vector3.Lerp(
                closedLocalPosition,
                openLocalPosition,
                targetProgress
            );

            Vector3 startSecPos =
                secondaryTransform != null
                    ? secondaryTransform.localPosition
                    : Vector3.zero;

            Vector3 targetSecPos =
                Vector3.Lerp(
                    secondaryClosedPos,
                    secondaryOpenPos,
                    targetProgress
                );

            Quaternion startRot =
                doorTransform.localRotation;

            Quaternion targetRot =
                Quaternion.Euler(
                    0f,
                    Mathf.Lerp(
                        closedYRotation,
                        openYRotation,
                        targetProgress
                    ),
                    0f
                );

            Quaternion startSecRot =
                secondaryTransform != null
                    ? secondaryTransform.localRotation
                    : Quaternion.identity;

            Quaternion targetSecRot =
                secondaryTransform != null
                    ? Quaternion.Euler(
                        0f,
                        Mathf.Lerp(
                            secondaryClosedYRotation,
                            secondaryOpenYRotation,
                            targetProgress
                        ),
                        0f
                    )
                    : Quaternion.identity;

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;

                float eased = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(t)
                );

                if (doorType == DoorType.SlidingHorizontal)
                {
                    doorTransform.localPosition =
                        Vector3.Lerp(
                            startPos,
                            targetPos,
                            eased
                        );

                    if (secondaryTransform != null &&
                        secondaryTransform != doorTransform)
                    {
                        secondaryTransform.localPosition =
                            Vector3.Lerp(
                                startSecPos,
                                targetSecPos,
                                eased
                            );
                    }
                }
                else
                {
                    doorTransform.localRotation =
                        Quaternion.Slerp(
                            startRot,
                            targetRot,
                            eased
                        );

                    if (secondaryTransform != null &&
                        secondaryTransform != doorTransform)
                    {
                        secondaryTransform.localRotation =
                            Quaternion.Slerp(
                                startSecRot,
                                targetSecRot,
                                eased
                            );
                    }
                }

                yield return null;
            }

            ApplyStateImmediate(targetProgress);

            currentState =
                isLocked
                    ? DoorState.Locked
                    : endState;

            activeAnimationRoutine = null;

            completionEvent?.Invoke();
        }
    }
}