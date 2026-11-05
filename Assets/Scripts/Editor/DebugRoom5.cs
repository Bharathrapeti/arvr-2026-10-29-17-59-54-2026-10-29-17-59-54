using UnityEngine;
using UnityEditor;

namespace AREscapeRoom.Editor
{
    public static class DebugRoom5
    {
        [InitializeOnLoadMethod]
        public static void InspectRoom5()
        {
            GameObject room5 = GameObject.Find("Room_5");
            if (room5 == null)
            {
                Debug.Log("[DebugRoom5] Room_5 not found!");
                return;
            }

            Debug.Log($"[DebugRoom5] Room_5 found! Child count: {room5.transform.childCount}");
            for (int i = 0; i < room5.transform.childCount; i++)
            {
                Transform child = room5.transform.GetChild(i);
                var comps = child.GetComponents<Component>();
                string compNames = "";
                foreach (var c in comps)
                {
                    if (c != null) compNames += c.GetType().Name + ", ";
                }
                Debug.Log($"[DebugRoom5] Child {i}: '{child.name}', activeSelf={child.gameObject.activeSelf}, pos={child.localPosition}, comps=[{compNames}]");
            }
        }
    }
}
