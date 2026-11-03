using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Doors
{
    public class EntranceTrapTrigger : MonoBehaviour
    {
        [Header("Door Reference")]
        [SerializeField] private DoorController entranceDoor;

        [Header("Settings")]
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private float approachDistance = 6.0f;
        [SerializeField] private float closeDistance = 10.0f;
        [SerializeField] private bool lockAfterEntering = true;

        private Transform playerTransform;
        private bool hasTrapped = false;
        private bool playerIsInside = false;

        public void Bind(DoorController door)
        {
            entranceDoor = door;
        }

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (hasTrapped || entranceDoor == null) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float distToDoor = Vector3.Distance(playerTransform.position, entranceDoor.transform.position);

            if (!playerIsInside && distToDoor <= approachDistance)
            {
                if (!entranceDoor.IsOpen &&
                    entranceDoor.CurrentState != DoorController.DoorState.Opening &&
                    !entranceDoor.IsLocked)
                {
                    entranceDoor.UnlockDoor();
                    entranceDoor.OpenDoor();
                }
            }
            else if (!playerIsInside && distToDoor > closeDistance)
            {
                if (entranceDoor.IsOpen && entranceDoor.CurrentState != DoorController.DoorState.Closing)
                {
                    entranceDoor.CloseDoor();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryTrap(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!hasTrapped)
            {
                TryTrap(other);
            }
        }

        private void TryTrap(Collider other)
        {
            if (hasTrapped || entranceDoor == null) return;
            if (other == null) return;

            bool isPlayer = other.CompareTag(targetTag) ||
                            (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(targetTag)) ||
                            other.GetComponentInParent<AREscapeRoom.Player.PlayerController>() != null;

            if (!isPlayer) return;

            playerIsInside = true;

            if (!lockAfterEntering) return;

            hasTrapped = true;
            Debug.Log("[EntranceTrapTrigger] Player entered the room. Closing and locking the entrance door.");

            entranceDoor.CloseDoor();
            entranceDoor.LockDoor();

            if (EscapeRoomManager.Instance != null)
            {
                EscapeRoomManager.Instance.NotifyRoomEntered();
            }
        }

        private void EnsureTriggerCollider()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(6f, 4f, 2f);
                col = box;
            }
            col.isTrigger = true;
        }

        private void FindPlayer()
        {
            GameObject p = GameObject.FindGameObjectWithTag(targetTag);
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }
    }
}
