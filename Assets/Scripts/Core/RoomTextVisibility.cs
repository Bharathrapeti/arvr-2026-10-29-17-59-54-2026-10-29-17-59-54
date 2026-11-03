using UnityEngine;

namespace AREscapeRoom.Core
{
    public class RoomTextVisibility : MonoBehaviour
    {
        private Renderer textRenderer;
        private RoomId cachedRoom = RoomId.Mall;
        private bool roomResolved = false;

        private void Awake()
        {
            textRenderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += Refresh;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= Refresh;
        }

        private void Start()
        {
            ResolveRoom();
            Refresh();
        }

        private void ResolveRoom()
        {
            if (roomResolved) return;
            cachedRoom = GetRoomFromWorldPosition(transform.position);
            roomResolved = true;
        }

        public void SetAssignedRoom(RoomId room)
        {
            cachedRoom = room;
            roomResolved = true;
            Refresh();
        }

        private void Refresh()
        {
            if (textRenderer == null)
            {
                textRenderer = GetComponent<Renderer>();
            }

            if (textRenderer == null)
                return;

            GameManager gm = GameManager.Instance;
            if (gm == null)
                return;

            if (!roomResolved)
            {
                ResolveRoom();
            }

            bool show = (cachedRoom == gm.CurrentRoom);
            textRenderer.enabled = show;
        }

        private RoomId GetRoomFromWorldPosition(Vector3 worldPosition)
        {
            // Room 1
            Transform room1 = GameObject.Find("AR_Escape_Room_Structure")?.transform;
            if (room1 != null && GetRoomBounds(room1).Contains(worldPosition))
                return RoomId.Room1;

            // Rooms 2-6
            for (int r = 2; r <= 6; r++)
            {
                Transform room = GameObject.Find($"Room_{r}")?.transform;
                if (room != null && GetRoomBounds(room).Contains(worldPosition))
                    return (RoomId)(r - 1);
            }

            return RoomId.Mall;
        }

        private Bounds GetRoomBounds(Transform room)
        {
            Vector3 center = room.position;
            center.y += 2.5f;
            return new Bounds(center, new Vector3(10f, 5f, 10f));
        }
    }
}
