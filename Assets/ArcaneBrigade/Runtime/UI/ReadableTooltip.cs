using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Full text for truncated labels: hover on desktop, tap to read on touch. No native input control.</summary>
    public sealed class ReadableTooltip : MonoBehaviour
    {
        private static string message;
        private static Rect anchor;
        private static int seenFrame = -10;
        private static UnityEngine.SceneManagement.Scene scene; // the scene the tip was seen in; a new scene drops it
        private static float pinnedUntil;
        private GUIStyle copy;
        private readonly GUIContent content = new GUIContent();
        private string measured;
        private float measuredWidth, measuredHeight;
        private int measuredSize;
        public static string Message => message;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (FindAnyObjectByType<ReadableTooltip>() != null) return;
            var go = new GameObject("Readable UI tooltips"); DontDestroyOnLoad(go); go.AddComponent<ReadableTooltip>();
        }
        public static void Observe(Rect bounds, GUIContent shown)
        {
            if (string.IsNullOrEmpty(shown?.tooltip) || !bounds.Contains(Event.current.mousePosition)) return;
            if (ScrollPanel.Inside && !ScrollPanel.Visible.Contains(Event.current.mousePosition)) return;
            bool tap = RunUIHints.Phone && Event.current.type == EventType.MouseUp;
            if (RunUIHints.Phone && !tap) return;
            if (Event.current.type != EventType.Repaint && !tap) return;
            Vector2 a = GUIUtility.GUIToScreenPoint(bounds.min), b = GUIUtility.GUIToScreenPoint(bounds.max);
            message = shown.tooltip; anchor = new Rect(a, b - a); seenFrame = Time.frameCount;
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (tap) pinnedUntil = Time.unscaledTime + 4;
        }
        public static void Clear() { message = null; pinnedUntil = 0; seenFrame = -10; }
        private void OnGUI()
        {
            if (scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene()) { Clear(); return; }
            if (string.IsNullOrEmpty(message) || Time.frameCount - seenFrame > 1 && Time.unscaledTime >= pinnedUntil) return;
            GUI.depth = -10000;
            Matrix4x4 previous = GUI.matrix; Color tint = GUI.color;
            GUI.matrix = Matrix4x4.identity; GUI.color = Color.white;
            copy ??= new GUIStyle { font = ArcadeTypography.Body, wordWrap = true, alignment = TextAnchor.UpperLeft, padding = new RectOffset(14, 14, 10, 10) };
            copy.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 720f * 22), 16, 32); copy.normal.textColor = ArcadeSkin.Ink;
            float width = Mathf.Min(Screen.width - 24, 620 * Screen.height / 720f);
            if (measured != message || measuredWidth != width || measuredSize != copy.fontSize)
            {
                measured = message; measuredWidth = width; measuredSize = copy.fontSize; content.text = message;
                measuredHeight = Mathf.Min(Screen.height - 24, copy.CalcHeight(content, width));
            }
            Rect box = new Rect(Mathf.Clamp(anchor.center.x - width * .5f, 12, Screen.width - width - 12),
                Mathf.Clamp(anchor.yMax + 10, 12, Screen.height - measuredHeight - 12), width, measuredHeight);
            if (box.Overlaps(anchor) && anchor.yMin > measuredHeight + 20) box.y = anchor.yMin - measuredHeight - 10;
            GUI.DrawTexture(new Rect(box.x - 2, box.y - 2, box.width + 4, box.height + 4), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, ArcadeSkin.Ink, 0, 0);
            GUI.DrawTexture(box, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, ArcadeSkin.Paper, 0, 0);
            GUI.Label(box, content, copy);
            GUI.matrix = previous; GUI.color = tint;
        }
    }
}
