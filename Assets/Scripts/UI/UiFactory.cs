using UnityEngine;
using UnityEngine.UI;

namespace AREscapeRoom.UI
{
    public static class UiFactory
    {
        public static Font Font
        {
            get
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Text AddText(GameObject go, string value, int size, TextAnchor anchor, Color color)
        {
            Text t = go.GetComponent<Text>();
            if (t == null) t = go.AddComponent<Text>();
            t.text = value;
            t.font = Font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Image AddImage(GameObject go, Color color)
        {
            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            AddImage(go, color);
            return go;
        }

        public static Text Label(Transform parent, string name, string value, Vector2 anchorMin, Vector2 anchorMax, int size, TextAnchor align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return AddText(go, value, size, align, Color.white);
        }
    }
}
