using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A name tag over a shop prop that can be walked to and tapped (the ferry, the ringing telephone, the warm chair),
    /// in the stalls' tag shape, so the player sees it is something to use without hovering (requirement 31/10/2026). Drawn in
    /// the 1920x1080 viewport; it bobs gently. Greyed with its own words when the prop has nothing to offer now.</summary>
    public static class PropTag
    {
        private static GUIStyle style;
        public static void Draw(SpriteRenderer prop, string word, bool usable)
        {
            Camera cam = Camera.main;
            if (cam == null || prop == null || !prop.enabled || string.IsNullOrEmpty(word)) return;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(cam.pixelRect, 1920, 1080);
            Bounds b = prop.bounds;
            Vector3 sp = cam.WorldToScreenPoint(new Vector3(b.center.x, b.max.y, 0));
            float bob = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Time.unscaledTime * 2.2f + b.center.x) * 3;
            Vector3 design = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(sp.x, Screen.height - sp.y, 0)); // the viewport's own fit (safe area included)
            Vector2 at = new Vector2(design.x, design.y - 18 + bob);
            if (style == null) { style = ArcadeTypography.Style(22, true); style.alignment = TextAnchor.MiddleCenter; style.wordWrap = false; style.normal.textColor = ArcadeSkin.Ink; }
            float w = style.CalcSize(new GUIContent(word)).x + 40;
            Rect tag = new Rect(Mathf.Clamp(at.x - w * .5f, 12, 1908 - w), Mathf.Max(12, at.y - 32), w, 32);
            ArcadeSkin.Slant(tag, usable ? ArcadeSkin.Teal : new Color(.55f, .52f, .5f));
            GUI.Label(tag, word, style);
            GUI.matrix = previous;
        }
    }
}
