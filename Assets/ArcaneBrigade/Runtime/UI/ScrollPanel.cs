using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The one scroll area every text panel uses (play-test 20/10/2026, item 19): mouse wheel and Page Up/Down while
    /// the pointer is over it, touch or mouse drag with a little inertia (a drag never clicks the button it started on), a thin
    /// thumb only when the content overflows, and a soft shade at a clipped edge so it is plain there is more to read.
    /// Drop-in for GUI.BeginScrollView / GUI.EndScrollView: <c>scroll = ScrollPanel.Begin(view, scroll, content); … ScrollPanel.End();</c></summary>
    public static class ScrollPanel
    {
        private sealed class Drag { public bool Down, Dragging; public Vector2 Start, Last; public float Velocity, LastTime; }
        private static readonly Dictionary<int, Drag> drags = new Dictionary<int, Drag>();
        private static readonly Stack<(Rect view, float max, float y, Color shade)> open = new Stack<(Rect, float, float, Color)>();
        private static Texture2D ramp;
        private const float DragSlop = 12, WheelStep = 42;
        public static readonly Color DefaultShade = new Color(0.02f, 0.02f, 0.04f, 0.55f);

        public static Vector2 Begin(Rect view, Vector2 scroll, Rect content) => Begin(view, scroll, content, DefaultShade);
        /// <summary>Inside an open scroll: the part of its content on screen (content coordinates), so long text draws only that.</summary>
        public static bool Inside => open.Count > 0;
        public static Rect Visible { get { var top = open.Peek(); return new Rect(0, top.Item3, top.Item1.width, top.Item1.height); } }
        public static Vector2 Begin(Rect view, Vector2 scroll, Rect content, Color shade)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (!drags.TryGetValue(id, out Drag drag)) drags[id] = drag = new Drag();
            float max = Mathf.Max(0, content.height - view.height);
            Event e = Event.current;
            bool over = view.Contains(e.mousePosition);
            if (max > 0)
            {
                switch (e.type)
                {
                    case EventType.ScrollWheel when over:
                        scroll.y += e.delta.y * WheelStep / 3f; drag.Velocity = 0; e.Use(); break;
                    case EventType.KeyDown when over && (e.keyCode == KeyCode.PageDown || e.keyCode == KeyCode.PageUp):
                        scroll.y += (e.keyCode == KeyCode.PageDown ? 1 : -1) * view.height * .8f; e.Use(); break;
                    case EventType.MouseDown when over:
                        drag.Down = true; drag.Dragging = false; drag.Start = drag.Last = e.mousePosition; drag.Velocity = 0; drag.LastTime = Time.unscaledTime; break;
                    case EventType.MouseDrag when drag.Down:
                        if (!drag.Dragging && Mathf.Abs(e.mousePosition.y - drag.Start.y) > DragSlop) { drag.Dragging = true; GUIUtility.hotControl = 0; }
                        if (drag.Dragging)
                        {
                            float step = drag.Last.y - e.mousePosition.y, dt = Mathf.Max(0.001f, Time.unscaledTime - drag.LastTime);
                            scroll.y += step; drag.Velocity = Mathf.Lerp(drag.Velocity, step / dt, 0.5f); drag.LastTime = Time.unscaledTime;
                            e.Use();
                        }
                        drag.Last = e.mousePosition; break;
                    case EventType.MouseUp when drag.Down:
                        drag.Down = false;
                        if (drag.Dragging) { drag.Dragging = false; e.Use(); } // the release after a drag is not a click
                        else drag.Velocity = 0;
                        break;
                    case EventType.Repaint when !drag.Down && Mathf.Abs(drag.Velocity) > 5:
                        float delta = Mathf.Min(0.05f, Time.unscaledDeltaTime);
                        scroll.y += drag.Velocity * delta; drag.Velocity *= PresentationSettings.ReducedMotion ? 0 : Mathf.Exp(-delta * 6f); break;
                }
            }
            else drag.Velocity = 0;
            scroll.x = 0; scroll.y = Mathf.Clamp(scroll.y, 0, max);
            open.Push((view, max, scroll.y, shade));
            return GUI.BeginScrollView(view, scroll, new Rect(0, 0, Mathf.Min(content.width, view.width), Mathf.Max(content.height, view.height)), false, false, GUIStyle.none, GUIStyle.none);
        }

        public static void End()
        {
            GUI.EndScrollView();
            if (open.Count == 0) return;
            var (view, max, y, shade) = open.Pop();
            if (max <= 0 || Event.current.type != EventType.Repaint) return;
            if (ramp == null)
            {
                ramp = new Texture2D(1, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int i = 0; i < 32; i++) ramp.SetPixel(0, i, new Color(1, 1, 1, Mathf.SmoothStep(0, 1, i / 31f)));
                ramp.Apply();
            }
            Color previous = GUI.color;
            float fade = Mathf.Min(36, view.height * .2f);
            // the shade sits over the clipped edge: strong at the cut, gone where the text is whole
            GUI.color = new Color(shade.r, shade.g, shade.b, shade.a * Mathf.Clamp01(y / 24f));
            if (y > 1) GUI.DrawTextureWithTexCoords(new Rect(view.x, view.y, view.width, fade), ramp, new Rect(0, 0, 1, 1));
            GUI.color = new Color(shade.r, shade.g, shade.b, shade.a * Mathf.Clamp01((max - y) / 24f));
            if (y < max - 1) GUI.DrawTextureWithTexCoords(new Rect(view.x, view.yMax - fade, view.width, fade), ramp, new Rect(0, 1, 1, -1));
            // the thumb: how much is shown and where
            float track = view.height - 8, thumb = Mathf.Max(28, track * view.height / (view.height + max));
            float top = view.y + 4 + (track - thumb) * (y / max);
            GUI.color = previous;
            // the painted rail and thumb from the UI kit (a walnut rail, a velvet lozenge in brass)
            ArcadeSkin.ScrollBar(new Rect(view.xMax - 13, view.y + 4, 10, track), new Rect(view.xMax - 15, top, 14, thumb));
        }
    }
}
