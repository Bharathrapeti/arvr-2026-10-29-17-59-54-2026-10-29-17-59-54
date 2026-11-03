using UnityEngine;

namespace AREscapeRoom.Doors
{
    public class MallEntranceProximity : MonoBehaviour
    {
        [Header("Door Reference")]
        [SerializeField] private DoorController mallSlidingDoor;

        [Header("Distance Settings")]
        [SerializeField] private float openDistance = 6.0f;
        [SerializeField] private float closeDistance = 9.0f;

        private Transform playerTransform;

        public void Bind(DoorController door)
        {
            mallSlidingDoor = door;
        }

        private void Start()
        {
            if (mallSlidingDoor == null)
            {
                mallSlidingDoor = GetComponent<DoorController>();
            }
            FindPlayer();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            if (mallSlidingDoor == null || mallSlidingDoor.IsLocked) return;

            float dist = Vector3.Distance(mallSlidingDoor.transform.position, playerTransform.position);

            if (dist <= openDistance &&
                !mallSlidingDoor.IsOpen &&
                mallSlidingDoor.CurrentState != DoorController.DoorState.Opening)
            {
                mallSlidingDoor.OpenDoor();
            }
            else if (dist > closeDistance &&
                     mallSlidingDoor.IsOpen &&
                     mallSlidingDoor.CurrentState != DoorController.DoorState.Closing)
            {
                mallSlidingDoor.CloseDoor();
            }
        }

        private void FindPlayer()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                playerTransform = p.transform;
            }
        }
    }
}
