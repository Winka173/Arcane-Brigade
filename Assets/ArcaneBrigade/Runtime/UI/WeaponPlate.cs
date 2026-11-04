using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The weapon a friend comes with, on a card: a blued-steel armoury plate with a brass shield socket at its left end
    /// holding the weapon's own picture, the name and damage on the plain middle, a crossguard cap at the right. It never looks
    /// like a bond chip or a text tag (requirement 4/11/2026).</summary>
    public static class WeaponPlate
    {
        // the painted plate (720 x 128), in its pixels: the shield socket ends at 165 and its centre is at 82; the left piece runs to
        // 226 and the right piece (the crossguard tip) starts at 546; between them five rivet spans repeat as often as needed; the
        // words sit between 165 and 630
        private const float Source = 128, Full = 720, LeftEnd = 226, RightStart = 546, TextStart = 165, TextEnd = 630, SocketX = 82;
        private static Sprite plate;
        private static bool loaded;
        private static Sprite Art { get { if (!loaded) { loaded = true; plate = ArcadeSkin.KitSprite("Info/WeaponPlate"); } return ArcadeSkin.Crafted ? null : plate; } }
        /// <summary>The plate's width for words this wide at this height.</summary>
        public static float Width(float height, float words) => Mathf.Max(height * (LeftEnd + Full - RightStart) / Source, height * (TextStart + Full - TextEnd) / Source + words + 18);
        public static void Draw(Rect r, Sprite weapon, string label, GUIStyle style)
        {
            float k = r.height / Source;
            Sprite art = Art;
            if (art != null && Event.current.type == EventType.Repaint)
            {
                Texture t = art.texture; Rect uv = art.textureRect; float tw = t.width, th = t.height, s = uv.width / Full;
                Rect Part(float x0, float x1) => new Rect((uv.x + x0 * s) / tw, uv.y / th, (x1 - x0) * s / tw, uv.height / th);
                float left = LeftEnd * k, right = (Full - RightStart) * k, span = (RightStart - LeftEnd) * k;
                GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y, left, r.height), t, Part(0, LeftEnd));
                float x = r.x + left, end = r.xMax - right;
                while (x < end - .5f)
                {
                    float w = Mathf.Min(span, end - x);
                    GUI.DrawTextureWithTexCoords(new Rect(x, r.y, w, r.height), t, Part(LeftEnd, LeftEnd + w / k));
                    x += w;
                }
                GUI.DrawTextureWithTexCoords(new Rect(end, r.y, right, r.height), t, Part(RightStart, Full));
            }
            else if (art == null) ArcadeSkin.Tag(r, "", CodeTone.Walnut);
            if (weapon != null)
            {
                float d = r.height * .78f;
                ShopDescriptionCard.DrawSprite(new Rect(r.x + SocketX * k - d * .5f, r.center.y - d * .5f, d, d), weapon);
            }
            Color ink = style.normal.textColor; TextAnchor keep = style.alignment;
            style.normal.textColor = ArcadeSkin.Cream; style.alignment = TextAnchor.MiddleLeft;
            float from = TextStart * k + 8, to = (Full - TextEnd) * k;
            GUI.Label(new Rect(r.x + from, r.y, r.width - from - to, r.height), label, style);
            style.normal.textColor = ink; style.alignment = keep;
        }
    }
}
