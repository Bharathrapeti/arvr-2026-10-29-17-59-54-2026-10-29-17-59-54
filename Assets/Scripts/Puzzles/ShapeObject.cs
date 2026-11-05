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

        public ShapeType Type
        {
            get
            {
                ShapeObject rootShape = GetRootShape();
                if (rootShape != null && rootShape != this)
                    return rootShape.Type;
                return shapeType;
            }
        }

        public bool Placed => placed;

        public bool CanInteract
        {
            get
            {
                ShapeObject rootShape = GetRootShape();
                if (rootShape != null && rootShape != this)
                    return rootShape.CanInteract;

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
                ShapeObject rootShape = GetRootShape();
                if (rootShape != null && rootShape != this)
                    return rootShape.Prompt;

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

        private ShapeObject GetRootShape()
        {
            if (transform.parent == null) return null;
            return transform.parent.GetComponentInParent<ShapeObject>();
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
            // 1. Get MeshFilter on this GameObject or any child to determine local mesh bounds
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) mf = GetComponentInChildren<MeshFilter>(true);

            Vector3 localCenter = Vector3.zero;
            Vector3 localSize = new Vector3(0.9f, 0.9f, 0.9f);

            if (mf != null && mf.sharedMesh != null)
            {
                localCenter = mf.gameObject == gameObject
                    ? mf.sharedMesh.bounds.center
                    : transform.InverseTransformPoint(mf.transform.TransformPoint(mf.sharedMesh.bounds.center));

                Vector3 sz = mf.sharedMesh.bounds.size;
                localSize = new Vector3(
                    Mathf.Max(sz.x * 1.5f, 0.9f),
                    Mathf.Max(sz.y * 1.5f, 0.7f),
                    Mathf.Max(sz.z * 1.5f, 0.9f)
                );
            }

            // 2. Ensure solid BoxCollider on this GameObject
            BoxCollider rootBox = GetComponent<BoxCollider>();
            if (rootBox == null) rootBox = gameObject.AddComponent<BoxCollider>();
            rootBox.center = localCenter;
            rootBox.size = localSize;
            rootBox.enabled = true;
            rootBox.isTrigger = false;

            // 3. Keep MeshCollider enabled as convex for precise collision
            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc == null && mf != null && mf.sharedMesh != null)
            {
                mc = gameObject.AddComponent<MeshCollider>();
            }
            if (mc != null)
            {
                if (mf != null && mf.sharedMesh != null)
                {
                    mc.sharedMesh = mf.sharedMesh;
                }
                mc.convex = true;
                mc.enabled = true;
                mc.isTrigger = false;
            }

            // 4. Ensure Rigidbody is kinematic
            body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            // 5. Also ensure any child MeshFilter has a BoxCollider and child ShapeObject forwarder
            MeshFilter[] childMfs = GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < childMfs.Length; i++)
            {
                var cmf = childMfs[i];
                if (cmf == null || cmf.gameObject == gameObject) continue;

                BoxCollider childBox = cmf.gameObject.GetComponent<BoxCollider>();
                if (childBox == null) childBox = cmf.gameObject.AddComponent<BoxCollider>();
                if (cmf.sharedMesh != null)
                {
                    childBox.center = cmf.sharedMesh.bounds.center;
                    Vector3 sz = cmf.sharedMesh.bounds.size;
                    childBox.size = new Vector3(
                        Mathf.Max(sz.x * 1.4f, 0.8f),
                        Mathf.Max(sz.y * 1.4f, 0.6f),
                        Mathf.Max(sz.z * 1.4f, 0.8f)
                    );
                }
                else
                {
                    childBox.center = Vector3.zero;
                    childBox.size = new Vector3(0.8f, 0.8f, 0.8f);
                }
                childBox.enabled = true;
                childBox.isTrigger = false;

                ShapeObject childShape = cmf.gameObject.GetComponent<ShapeObject>();
                if (childShape == null) childShape = cmf.gameObject.AddComponent<ShapeObject>();
                childShape.Bind(shapeType);
            }

            colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = true;
                    colliders[i].isTrigger = false;
                }
            }
        }

        public void Interact(GameObject instigator)
        {
            // If this component is on a child, delegate to parent ShapeObject
            ShapeObject rootShape = GetRootShape();
            if (rootShape != null && rootShape != this)
            {
                rootShape.Interact(instigator);
                return;
            }

            if (placed || held)
                return;

            PlayerInteraction interaction = instigator.GetComponent<PlayerInteraction>();
            if (interaction == null)
                interaction = instigator.GetComponentInParent<PlayerInteraction>();

            if (interaction == null || interaction.Carried != null)
                return;

            held = true;
            SetPhysics(false);
            interaction.SetCarried(this);

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
                if (z >= -25.0f && z <= -15.0f)
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
