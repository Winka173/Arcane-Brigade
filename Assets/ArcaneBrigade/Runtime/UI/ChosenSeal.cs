using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The one look of "this is the chosen one" across the game (requirement 3/11/2026: glowing rings and turning dots read
    /// as cheap, and the picked piece grew and pushed its neighbours): a small wax seal pinned at the chosen thing's top-right corner,
    /// rocking a little, over a magic circle that turns behind it. Nothing moves or resizes. It marks only what is picked, never what
    /// the pointer passes over or what the traveller stands beside (requirement 4/11/2026: it showed all over the shop). A pick is
    /// drawn at once, after the piece's own art, so a popup or card opened later covers it; only the keyboard or pad focus is drawn
    /// last, over everything.</summary>
    public static partial class ArcadeSkin
    {
        private struct ChosenAsk { public Vector2 Min, Max; }
        private static readonly List<ChosenAsk> chosen = new List<ChosenAsk>();

        /// <summary>Marks a picked rectangle (in the current GUI space) now: call it after the piece's own art.</summary>
        public static void Chosen(Rect r)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || r.width <= 0 || r.height <= 0) return;
            DrawSeal(r, Screen.height / 1080f / Mathf.Max(.01f, GUI.matrix.lossyScale.y));
        }

        /// <summary>Marks the keyboard or pad focus (in the current GUI space); drawn after every other GUI this frame.</summary>
        public static void ChosenLater(Rect r)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || r.width <= 0 || r.height <= 0) return;
            ChosenSealLayer.Ensure();
            chosen.Add(new ChosenAsk { Min = GUIUtility.GUIToScreenPoint(r.min), Max = GUIUtility.GUIToScreenPoint(r.max) });
        }

        /// <summary>Draws the seals asked for so far (in the current clip, so a scroll view's own are cut at its edge).</summary>
        public static void ChosenFlush()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || chosen.Count == 0) return;
            Matrix4x4 keep = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;
            float unit = Screen.height / 1080f;
            foreach (ChosenAsk ask in chosen)
            {
                Vector2 a = GUIUtility.ScreenToGUIPoint(ask.Min), b = GUIUtility.ScreenToGUIPoint(ask.Max);
                Rect r = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
                DrawSeal(r, unit);
            }
            chosen.Clear();
            GUI.matrix = keep;
        }

        /// <summary>Draws the seal now, in the current GUI space (for a list in a scroll view: call it after the piece's own art).</summary>
        public static void ChosenNow(Rect r)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || r.width <= 0 || r.height <= 0) return;
            float scale = Mathf.Max(.01f, GUI.matrix.lossyScale.y);
            DrawSeal(r, Screen.height / 1080f / scale, .46f); // inside a scroll view the seal sits within the corner, so the view's edge never cuts it
        }

        /// <summary>Turns what is drawn next about a point of the current GUI space, inside any clip or scroll view (the turn is
        /// made in local coordinates, before the GUI matrix).</summary>
        private static void Spin(Matrix4x4 keep, float degrees, Vector2 pivot) =>
            GUI.matrix = keep * Matrix4x4.TRS(pivot, Quaternion.Euler(0, 0, degrees), Vector3.one) * Matrix4x4.TRS(-pivot, Quaternion.identity, Vector3.one);

        private static void DrawSeal(Rect r, float unit, float inset = .2f)
        {
            bool still = PresentationSettings.ReducedMotion;
            float t = Time.unscaledTime;
            float size = Mathf.Clamp(Mathf.Min(r.width, r.height) * .36f, 34 * unit, 62 * unit);
            Vector2 at = new Vector2(r.xMax - size * inset, r.y + size * inset);
            Sprite ring = KitSprite("Info/SigilRingOuter"), wax = KitSprite("Info/SigilSealWax");
            Matrix4x4 keep = GUI.matrix;
            if (ring != null && !Crafted)
            {
                float ringSize = size * 1.75f;
                Spin(keep, still ? 0 : t * 40f, at);
                GUI.DrawTexture(new Rect(at.x - ringSize * .5f, at.y - ringSize * .5f, ringSize, ringSize), ring.texture, ScaleMode.ScaleToFit);
                GUI.matrix = keep;
            }
            if (wax != null && !Crafted)
            {
                Spin(keep, still ? 0 : Mathf.Sin(t * 2.2f) * 8f, at);
                GUI.DrawTexture(new Rect(at.x - size * .5f, at.y - size * .5f, size, size), wax.texture, ScaleMode.ScaleToFit);
                GUI.matrix = keep;
            }
            else Seal(at, size, "✓");
        }
    }

    /// <summary>Draws the chosen seals nobody flushed, after every other GUI (its depth is the lowest, so it paints last).</summary>
    public sealed class ChosenSealLayer : MonoBehaviour
    {
        private static ChosenSealLayer instance;
        public static void Ensure()
        {
            if (instance != null) return;
            var go = new GameObject("ChosenSealLayer"); DontDestroyOnLoad(go); instance = go.AddComponent<ChosenSealLayer>();
        }
        private void OnGUI()
        {
            GUI.depth = -1000;
            ArcadeSkin.ChosenFlush();
        }
    }
}
