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

        /// <summary>The curse laid OVER the normal rarity frame (owner, 20/10/2026: keep the description card, curse its frame):
        /// cracks creep in from the corners with a heartbeat ember glowing in them, thorn-and-rose corners from the blood-price
        /// sheet, a thin dark-red bleed along the inner edge, and drops seeping from the top edge. The card keeps its own paper.</summary>
        public static void Draw(Rect panel, float clock)
        {
            Load();
            bool still = PresentationSettings.ReducedMotion;
            float beat = still ? .35f : Heartbeat(clock);
            // the heartbeat shows on the rim only (a radial glow over the card would tint the whole paper red)
            for (int i = 0; i < 4; i++)
            {
                Color pulse = new Color(Ember.r, Ember.g * .5f, Ember.b * .5f, (.10f + beat * .30f) * (1 - i / 4f)); float o = 1 + i * 2;
                ArcadeSkin.Line(new Rect(panel.x - o, panel.y - o, panel.width + 2 * o, 2), pulse); ArcadeSkin.Line(new Rect(panel.x - o, panel.yMax + o - 2, panel.width + 2 * o, 2), pulse);
                ArcadeSkin.Line(new Rect(panel.x - o, panel.y - o, 2, panel.height + 2 * o), pulse); ArcadeSkin.Line(new Rect(panel.xMax + o - 2, panel.y - o, 2, panel.height + 2 * o), pulse);
            }
            // a thin bleed just inside the rim, not a new background
            for (int i = 0; i < 6; i++)
            {
                Color bleed = new Color(Blood.r * .6f, 0, .02f, .16f * (1 - i / 6f)); float o = 3 + i * 3;
                ArcadeSkin.Line(new Rect(panel.x + o, panel.y + o, panel.width - 2 * o, 3), bleed); ArcadeSkin.Line(new Rect(panel.x + o, panel.yMax - o - 3, panel.width - 2 * o, 3), bleed);
                ArcadeSkin.Line(new Rect(panel.x + o, panel.y + o, 3, panel.height - 2 * o), bleed); ArcadeSkin.Line(new Rect(panel.xMax - o - 3, panel.y + o, 3, panel.height - 2 * o), bleed);
            }
            // cracks from every corner (mirrored): dark ink with an ember line pulsing inside
            float c = Mathf.Clamp(Mathf.Min(panel.width, panel.height) * .55f, 70, 170);
            for (int k = 0; k < 4; k++)
            {
                bool right = k == 1 || k == 3, bottom = k >= 2;
                Rect corner = new Rect(right ? panel.xMax - c : panel.x, bottom ? panel.yMax - c : panel.y, c, c);
                Rect uv = new Rect(right ? 1 : 0, bottom ? 1 : 0, right ? -1 : 1, bottom ? -1 : 1);
                Color keep = GUI.color;
                GUI.color = new Color(.05f, 0, .01f, .85f * keep.a); GUI.DrawTextureWithTexCoords(corner, Cracks, uv, true);
                GUI.color = new Color(Ember.r, Ember.g * .7f, Ember.b * .6f, (.25f + beat * .6f) * keep.a); GUI.DrawTextureWithTexCoords(corner, CrackGlow, uv, true);
                GUI.color = keep;
            }
            // the thorn-and-rose corners from the painted blood-price sheet, corners only
            if (frame != null)
            {
                float cs = Mathf.Min(78, panel.height * .4f);
                Draw(new Rect(panel.x - cs * .18f, panel.y - cs * .18f, cs, cs), frame, new Rect(0, .75f, .25f, .25f));
                Draw(new Rect(panel.xMax - cs * .82f, panel.y - cs * .18f, cs, cs), frame, new Rect(.75f, .75f, .25f, .25f));
                Draw(new Rect(panel.x - cs * .18f, panel.yMax - cs * .82f, cs, cs), frame, new Rect(0, 0, .25f, .25f));
                Draw(new Rect(panel.xMax - cs * .82f, panel.yMax - cs * .82f, cs, cs), frame, new Rect(.75f, 0, .25f, .25f));
            }
            if (drip == null) return;
            // three drops creep down from the top edge, each on its own slow clock
            float[] spots = { .24f, .56f, .83f };
            for (int i = 0; i < spots.Length; i++)
            {
                float phase = still ? .55f : Mathf.Repeat(clock * .09f + i * .37f, 1f);
                float length = Mathf.Lerp(.25f, 1f, Mathf.SmoothStep(0, 1, phase)) * Mathf.Min(70, panel.height * .3f);
                float fade = still ? 1 : Mathf.Clamp01((1 - phase) * 6);
                float full = Mathf.Min(70, panel.height * .3f), width = full * drip.rect.width / drip.rect.height;
                Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * fade);
                Draw(new Rect(panel.x + panel.width * spots[i] - width * .5f, panel.y + 6, width, length), drip, new Rect(0, 1 - length / full, 1, length / full));
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

        // ---- procedural cracks: a jagged branching fracture grown from the corner (seeded, so every card cracks the same) ----
        private static Texture2D cracks, crackGlow;
        private static Texture2D Cracks { get { if (cracks == null) BuildCracks(); return cracks; } }
        private static Texture2D CrackGlow { get { if (crackGlow == null) BuildCracks(); return crackGlow; } }
        private static void BuildCracks()
        {
            const int N = 192; var ink = new float[N * N]; var glow = new float[N * N];
            var random = new System.Random(7731);
            void Stamp(float[] map, float x, float y, float radius)
            {
                int r = Mathf.CeilToInt(radius + 1);
                for (int dy = -r; dy <= r; dy++) for (int dx = -r; dx <= r; dx++)
                {
                    int px = Mathf.RoundToInt(x) + dx, py = Mathf.RoundToInt(y) + dy; if (px < 0 || py < 0 || px >= N || py >= N) continue;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / Mathf.Max(.5f, radius); if (d > 1) continue;
                    map[py * N + px] = Mathf.Max(map[py * N + px], 1 - d * d);
                }
            }
            void Crack(float x, float y, float angle, float width, int steps, int depth)
            {
                for (int i = 0; i < steps; i++)
                {
                    angle += (float)(random.NextDouble() - .5) * .9f; float len = 3 + (float)random.NextDouble() * 4;
                    float nx = x + Mathf.Cos(angle) * len, ny = y + Mathf.Sin(angle) * len;
                    for (float t = 0; t <= 1; t += .2f)
                    {
                        float px = Mathf.Lerp(x, nx, t), py = Mathf.Lerp(y, ny, t), w = width * (1 - i / (float)steps * .8f);
                        Stamp(ink, px, py, w); Stamp(glow, px, py, Mathf.Max(.6f, w * .45f));
                    }
                    x = nx; y = ny;
                    if (depth < 3 && random.NextDouble() < .16) Crack(x, y, angle + (random.NextDouble() < .5 ? -1 : 1) * (.5f + (float)random.NextDouble() * .7f), width * .6f, steps / 2, depth + 1);
                }
            }
            // texture origin is bottom-left; the card corner is the texture's top-left, (0, N)
            Crack(2, N - 2, -.75f, 3.2f, 26, 0);
            Crack(2, N - 30, -.2f, 2.2f, 16, 1);
            Crack(30, N - 2, -1.3f, 2.2f, 16, 1);
            cracks = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            crackGlow = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var a = new Color32[N * N]; var g = new Color32[N * N];
            for (int i = 0; i < a.Length; i++) { a[i] = new Color32(255, 255, 255, (byte)(255 * ink[i])); g[i] = new Color32(255, 255, 255, (byte)(255 * glow[i])); }
            cracks.SetPixels32(a); cracks.Apply(); crackGlow.SetPixels32(g); crackGlow.Apply();
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
