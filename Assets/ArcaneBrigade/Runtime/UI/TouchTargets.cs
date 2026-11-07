using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every on-screen button goes through here. On the phone layout a button answers a tap anywhere within a finger's
    /// reach (8 mm) of its drawn shape, so small painted buttons stay as drawn but are easy to hit (requirement 3/11/2026: mobile is
    /// released first; most buttons were 2-4 mm on a phone). A tap on a button's own shape always goes to that button; a tap beside
    /// buttons goes to the nearest one.</summary>
    public static class TouchTargets
    {
        private static List<Rect> drawn = new List<Rect>(), previous = new List<Rect>();
        private static int frame = -1;

        /// <summary>A finger's reach in screen pixels: 8 mm where the screen knows its density, else 11.5% of its height (8 mm on a
        /// 6.5" phone held sideways).</summary>
        public static float ReachPixels => Application.isMobilePlatform && Screen.dpi > 0 ? 8f / 25.4f * Screen.dpi : Screen.height * 0.115f;

        public static bool Button(Rect bounds, string caption, GUIStyle style) => Button(bounds, LocalizedGUI.Content(caption), style);
        public static bool Button(Rect bounds, string caption) => Button(bounds, LocalizedGUI.Content(caption), GUI.skin.button);
        public static bool Button(Rect bounds, GUIContent content, GUIStyle style)
        {
            if (!RunUIHints.Phone) return LocalizedGUI.Button(bounds, content, style);
            Rect screen = ToScreen(bounds);
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                if (frame != Time.frameCount) { frame = Time.frameCount; (previous, drawn) = (drawn, previous); drawn.Clear(); }
                if (GUI.enabled) drawn.Add(screen);
                return LocalizedGUI.Button(bounds, content, style);
            }
            if (e.type != EventType.MouseDown && e.type != EventType.MouseUp || !GUI.enabled || bounds.Contains(e.mousePosition)) return LocalizedGUI.Button(bounds, content, style);
            Vector2 point = GUI.matrix.MultiplyPoint3x4(e.mousePosition);
            if (!Nearest(point, out Rect target) || Vector2.Distance(target.center, screen.center) > 6f) return LocalizedGUI.Button(bounds, content, style);
            Vector2 keep = e.mousePosition;
            e.mousePosition = bounds.center;
            bool clicked = LocalizedGUI.Button(bounds, content, style);
            if (e.type != EventType.Used) e.mousePosition = keep;
            return clicked;
        }

        /// <summary>The button last drawn nearest the point within reach, or none when the point is on a button's own shape (that
        /// button takes it as usual) or out of reach of all of them.</summary>
        private static bool Nearest(Vector2 point, out Rect target)
        {
            target = default;
            float reach = ReachPixels, best = float.MaxValue;
            foreach (Rect r in previous)
            {
                if (r.Contains(point)) return false;
                float grow = Mathf.Max(0, reach - Mathf.Min(r.width, r.height)) * 0.5f;
                float dx = Mathf.Max(r.xMin - point.x, 0, point.x - r.xMax), dy = Mathf.Max(r.yMin - point.y, 0, point.y - r.yMax);
                if (dx > grow || dy > grow) continue;
                float distance = dx * dx + dy * dy;
                if (distance < best) { best = distance; target = r; }
            }
            return best < float.MaxValue;
        }

        private static Rect ToScreen(Rect r)
        {
            Vector2 a = GUI.matrix.MultiplyPoint3x4(r.min), b = GUI.matrix.MultiplyPoint3x4(r.max);
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
    }
}
