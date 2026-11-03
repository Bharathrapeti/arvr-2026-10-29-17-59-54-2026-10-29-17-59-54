using UnityEngine;
using AREscapeRoom.Core;

namespace AREscapeRoom.Core
{
    [RequireComponent(typeof(Collider))]
    public class RoomZone : MonoBehaviour
    {
        [SerializeField] private RoomId roomId = RoomId.Room1;

        public void Bind(RoomId id)
        {
            roomId = id;
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance == null) return;
            if (other.CompareTag("Player") || other.GetComponentInParent<AREscapeRoom.Player.PlayerController>() != null)
            {
                GameManager.Instance.SetRoom(roomId);
            }
        }
    }
}
