using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The "blood price" look of every offer bought with HP (play-test 20/10/2026, item 10): near-black red paper, the
    /// painted thorn-and-rose frame, slow drops of blood creeping down from the top edge, a faint heartbeat along the edge, and a
    /// wax seal carrying the heart and "−N". Art: Resources/UI/BloodPrice (cut from the image agent's UI_BloodPrice sheet); when a
    /// piece is missing the card falls back to painted strokes. Reduced Motion: the drops stay still and the edge does not pulse.</summary>
    public static class BloodPriceFrame
    {
        private static Sprite frame, seal, drip, button, buttonOff;
        private static bool loaded;
        private static GUIStyle sealText;
        public static readonly Color Paper = new Color(.13f, .025f, .04f, .97f), Blood = new Color(.62f, .05f, .09f), Ember = new Color(1f, .42f, .42f);

        private static void Load()
        {
            if (loaded) return; loaded = true;
            frame = Resources.Load<Sprite>("UI/BloodPrice/BloodFrame"); seal = Resources.Load<Sprite>("UI/BloodPrice/BloodSeal");
            drip = Resources.Load<Sprite>("UI/BloodPrice/BloodDrip"); button = Resources.Load<Sprite>("UI/BloodPrice/BloodButton");
            buttonOff = Resources.Load<Sprite>("UI/BloodPrice/BloodButtonOff");
        }
        public static bool HasArt { get { Load(); return frame != null; } }

        /// <summary>The card itself: paper, frame, heartbeat edge and creeping drops.</summary>
        public static void Draw(Rect panel, float clock)
        {
            Load();
            bool still = PresentationSettings.ReducedMotion;
            RunHud.Fill(panel, Paper);
            // a heartbeat: two quick swells, then rest (about 70 beats a minute)
            float beat = still ? 0 : Heartbeat(clock);
            ArcadeSkin.Glow(new Rect(panel.x - 6, panel.y - 6, panel.width + 12, panel.height + 12), new Color(Ember.r, Ember.g, Ember.b, .10f + beat * .22f));
            if (frame != null) NineSlice(panel, frame, .25f, Mathf.Min(86, panel.height * .36f));
            else
            {
                // fallback: an inked rim of thorns
                Color ink = new Color(.45f, .06f, .08f);
                ArcadeSkin.Line(new Rect(panel.x, panel.y, panel.width, 4), ink); ArcadeSkin.Line(new Rect(panel.x, panel.yMax - 4, panel.width, 4), ink);
                ArcadeSkin.Line(new Rect(panel.x, panel.y, 4, panel.height), ink); ArcadeSkin.Line(new Rect(panel.xMax - 4, panel.y, 4, panel.height), ink);
                for (float x = panel.x + 18; x < panel.xMax - 18; x += 34) ArcadeSkin.Line(new Rect(x, panel.y + 3, 3, 9), ink);
            }
            if (drip == null) return;
            // three drops creep down from the top edge, each on its own slow clock
            float[] at = { .24f, .56f, .83f };
            for (int i = 0; i < at.Length; i++)
            {
                float phase = still ? .55f : Mathf.Repeat(clock * .09f + i * .37f, 1f);
                float length = Mathf.Lerp(.25f, 1f, Mathf.SmoothStep(0, 1, phase)) * Mathf.Min(70, panel.height * .3f);
                float fade = still ? 1 : Mathf.Clamp01((1 - phase) * 6);
                float full = Mathf.Min(70, panel.height * .3f), width = full * drip.rect.width / drip.rect.height;
                Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * fade);
                Draw(new Rect(panel.x + panel.width * at[i] - width * .5f, panel.y + 6, width, length), drip, new Rect(0, 1 - length / full, 1, length / full));
                GUI.color = keep;
            }
        }

        /// <summary>The wax seal with the heart and the price ("−7"): what this costs, said once.</summary>
        public static void Seal(Rect r, int price, Sprite heart)
        {
            Load();
            if (seal != null) RunHud.Sprite(r, seal); else { ArcadeSkin.Glow(r, new Color(Blood.r, Blood.g, Blood.b, .9f)); }
            if (sealText == null) { sealText = ArcadeTypography.Style(28, true); sealText.alignment = TextAnchor.MiddleCenter; sealText.normal.textColor = new Color(1f, .9f, .86f); }
            float h = r.height * .34f;
            if (heart != null) RunHud.Sprite(new Rect(r.center.x - h * .5f, r.y + r.height * .2f, h, h), heart);
            sealText.fontSize = Mathf.RoundToInt(r.height * .26f);
            GUI.Label(new Rect(r.x, r.y + r.height * .48f, r.width, r.height * .3f), "−" + price, sealText);
        }

        /// <summary>The painted buy button for an HP price (drawn over the button's own hit area).</summary>
        public static void Button(Rect r, bool affordable)
        {
            Load();
            Sprite s = affordable ? button : buttonOff ?? button;
            if (s != null) NineSlice(r, s, .3f, Mathf.Min(r.height * .5f, 46));
            else RunHud.Fill(r, affordable ? Blood : new Color(.3f, .27f, .27f));
        }

        public static float Heartbeat(float clock)
        {
            float t = Mathf.Repeat(clock, .86f);
            return Mathf.Max(Mathf.Exp(-Mathf.Pow((t - .08f) / .05f, 2)), .6f * Mathf.Exp(-Mathf.Pow((t - .26f) / .05f, 2)));
        }

        private static void Draw(Rect r, Sprite s, Rect uv)
        {
            Rect t = s.textureRect; Texture tex = s.texture;
            GUI.DrawTextureWithTexCoords(r, tex, new Rect((t.x + uv.x * t.width) / tex.width, (t.y + uv.y * t.height) / tex.height, uv.width * t.width / tex.width, uv.height * t.height / tex.height), true);
        }

        /// <summary>A sprite drawn as nine pieces: corners keep their shape, edges and middle stretch.</summary>
        private static void NineSlice(Rect r, Sprite s, float border, float size)
        {
            float bw = Mathf.Min(size, r.width * .5f), bh = Mathf.Min(size, r.height * .5f);
            float[] xs = { r.x, r.x + bw, r.xMax - bw, r.xMax }, ys = { r.y, r.y + bh, r.yMax - bh, r.yMax };
            float[] us = { 0, border, 1 - border, 1 }, vs = { 1, 1 - border, border, 0 };
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Rect dest = new Rect(xs[i], ys[j], xs[i + 1] - xs[i], ys[j + 1] - ys[j]);
                    if (dest.width <= 0 || dest.height <= 0) continue;
                    Draw(dest, s, new Rect(us[i], vs[j + 1], us[i + 1] - us[i], vs[j] - vs[j + 1]));
                }
        }
    }
}
