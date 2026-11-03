using UnityEngine;
using AREscapeRoom.Core;
using AREscapeRoom.Player;

namespace AREscapeRoom.Puzzles
{
    public class ShapeObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private ShapeType shapeType = ShapeType.Square;
        [SerializeField] private bool placed;
        [SerializeField] private bool held;

        private Collider[] colliders;
        private Rigidbody body;

        public ShapeType Type => shapeType;
        public bool Placed => placed;
        public bool CanInteract
        {
            get
            {
                if (placed || held) return false;
                var player = Object.FindFirstObjectByType<PlayerInteraction>();
                if (player != null && player.Carried != null) return false;
                return true;
            }
        }

        public string Prompt
        {
            get
            {
                switch (shapeType)
                {
                    case ShapeType.Triangle: return "[E] Pick up Traingle Shape";
                    case ShapeType.Star: return "[E] Pick up Star Shape";
                    case ShapeType.Circle: return "[E] Pick up Circle Shape";
                    case ShapeType.Square: return "[E] Pick up Square Shape";
                    default: return $"[E] Pick up {shapeType} Shape";
                }
            }
        }

        public void Bind(ShapeType type)
        {
            shapeType = type;
        }

        private void Awake()
        {
            DetectShapeTypeFromName();
            EnsureCollidersAndPhysics();
        }

        public void DetectShapeTypeFromName()
        {
            string objName = gameObject.name.ToLower();
            if (objName.Contains("triang") || objName.Contains("traing") || objName.Contains("tri"))
                shapeType = ShapeType.Triangle;
            else if (objName.Contains("circle") || objName.Contains("circ"))
                shapeType = ShapeType.Circle;
            else if (objName.Contains("star"))
                shapeType = ShapeType.Star;
            else if (objName.Contains("square") || objName.Contains("sq"))
                shapeType = ShapeType.Square;
        }

        public void EnsureCollidersAndPhysics()
        {
            colliders = GetComponentsInChildren<Collider>(true);
            if (colliders == null || colliders.Length == 0)
            {
                var mf = GetComponentInChildren<MeshFilter>(true);
                if (mf != null && mf.sharedMesh != null)
                {
                    var mc = gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = true;
                    colliders = new Collider[] { mc };
                }
                else
                {
                    var bc = gameObject.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.6f, 0.15f, 0.6f);
                    colliders = new Collider[] { bc };
                }
            }
            else
            {
                // Ensure any existing MeshCollider is convex so physics / CharacterController behaves cleanly
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] is MeshCollider meshCol)
                    {
                        meshCol.convex = true;
                    }
                }
            }

            body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }
            body.isKinematic = true;
        }

        public void Interact(GameObject instigator)
        {
            if (placed || held)
                return;

            PlayerInteraction interaction = instigator.GetComponent<PlayerInteraction>();

            if (interaction == null)
                interaction = instigator.GetComponentInParent<PlayerInteraction>();

            if (interaction == null)
                return;

            if (interaction.Carried != null)
                return;

            held = true;
            SetPhysics(false);
            interaction.SetCarried(this);

            // Carry shape directly attached to user hands / hold point
            if (interaction.HoldPoint != null)
            {
                transform.SetParent(interaction.HoldPoint, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            }

            Debug.Log($"[ShapeObject] Picked up {shapeType}");
        }

        public static bool IsPlayerInRoom5()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentRoom == RoomId.Room5)
                return true;

            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                float z = player.transform.position.z;
                // Room 5 is located between z = 14f and z = 26f
                if (z >= 14f && z <= 26f)
                    return true;
            }

            return false;
        }

        public void FollowHoldPoint(Transform holdPoint)
        {
            if (!held || holdPoint == null)
                return;

            if (transform.parent != holdPoint)
            {
                transform.SetParent(holdPoint, false);
            }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void PlaceInSocket(Transform socket)
        {
            if (socket == null)
                return;

            held = false;
            placed = true;

            SetPhysics(false);

            transform.SetParent(socket, false);
            transform.localPosition = new Vector3(0f, 0f, -0.06f);
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        }

        public void ReleaseCarry()
        {
            held = false;
        }

        public void SetPhysics(bool enabled)
        {
            colliders = GetComponentsInChildren<Collider>(true);
            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                        colliders[i].enabled = enabled;
                }
            }

            if (body == null)
                body = GetComponent<Rigidbody>();

            if (body != null)
            {
                body.isKinematic = !enabled;
            }
        }
    }
}
