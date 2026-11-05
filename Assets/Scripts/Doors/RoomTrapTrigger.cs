using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Doors
{
    public class RoomTrapTrigger : MonoBehaviour
    {
        [Header("Target Door")]
        [SerializeField] private DoorController doorToTrap;

        [Header("Settings")]
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private bool lockAfterEntering = true;
        [SerializeField] private RoomId roomEntered = RoomId.Room2;

        private bool hasTrapped = false;

        public void Bind(DoorController door, RoomId nextRoom = RoomId.Room2, bool lockDoor = true)
        {
            doorToTrap = door;
            roomEntered = nextRoom;
            lockAfterEntering = lockDoor;
        }

        private void Awake()
        {
            if (doorToTrap == null)
            {
                GameObject exitObj = GameObject.Find("ExitDoor_Keypad");
                if (exitObj != null)
                {
                    doorToTrap = exitObj.GetComponent<DoorController>();
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
            if (hasTrapped || doorToTrap == null || other == null) return;

            bool isPlayer = other.CompareTag(targetTag) ||
                            other.gameObject.name.Equals("Player", System.StringComparison.OrdinalIgnoreCase) ||
                            (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(targetTag)) ||
                            other.GetComponentInParent<PlayerController>() != null ||
                            other.GetComponentInParent<CharacterController>() != null;

            if (!isPlayer) return;

            // Only trigger if the door has been opened or is in the process of opening
            if (!doorToTrap.IsOpen && doorToTrap.CurrentState != DoorController.DoorState.Opening)
            {
                return;
            }

            hasTrapped = true;
            Debug.Log($"[RoomTrapTrigger] Player entered {roomEntered}. Closing and locking '{doorToTrap.DoorName}'.");

            doorToTrap.CloseDoor();
            if (lockAfterEntering)
            {
                doorToTrap.LockDoor();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetRoom(roomEntered);
            }

            if (EscapeRoomManager.Instance != null && roomEntered == RoomId.Room2)
            {
                EscapeRoomManager.Instance.NotifyRoomEntered();
            }
        }
    }
}
