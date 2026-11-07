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

        /// <summary>An HP-priced offer keeps the ordinary description card (requirement 21/10/2026: "use a normal description and give
        /// it an effect"); only this goes on top: a dark-red rim that beats like a heart, a red light running round the edge, and a
        /// few slow drops seeping from the top edge; inside, a nightmare vignette, drifting smoke and corner veins drawn before the text.</summary>
        public static void Draw(Rect panel, float clock)
        {
            bool still = PresentationSettings.ReducedMotion;
            float beat = still ? .4f : Heartbeat(clock);
            // INSIDE the card (requirement 22/10/2026: "a nightmare, a trade-off, inside too"), all drawn before the text so it stays
            // readable: a crimson-black vignette closing in from the edges with each heartbeat, slow nightmare smoke drifting
            // across, and dark veins creeping in from the corners with an ember pulse in them.
            Color keepInside = GUI.color;
            GUI.color = new Color(.24f, 0, .03f, (.5f + beat * .25f) * keepInside.a); // darker (requirement 24/10/2026)
            GUI.DrawTexture(panel, Vignette);
            Rect inner = new Rect(panel.x + 4, panel.y + 4, panel.width - 8, panel.height - 8);
            float drift = still ? 0 : clock * .025f;
            GUI.color = new Color(.45f, .02f, .08f, .22f * keepInside.a);
            GUI.DrawTextureWithTexCoords(inner, Smoke, new Rect(drift, drift * .35f, inner.width / 420f, inner.height / 420f), true);
            GUI.color = new Color(.2f, 0, .05f, .18f * keepInside.a);
            GUI.DrawTextureWithTexCoords(inner, Smoke, new Rect(-drift * 1.6f + .37f, .5f, inner.width / 300f, inner.height / 300f), true);
            // requirement 24/10/2026: the crackling veins spread over the whole card (each corner reaches its middle), darker
            for (int k = 0; k < 4; k++)
            {
                bool right = k == 1 || k == 3, bottom = k >= 2;
                float vw = panel.width * .5f + 2, vh = panel.height * .5f + 2;
                Rect corner = new Rect(right ? panel.xMax - vw : panel.x, bottom ? panel.yMax - vh : panel.y, vw, vh);
                Rect uv = new Rect(right ? 1 : 0, bottom ? 1 : 0, right ? -1 : 1, bottom ? -1 : 1);
                GUI.color = new Color(.05f, 0, .015f, .85f * keepInside.a); GUI.DrawTextureWithTexCoords(corner, Cracks, uv, true);
                GUI.color = new Color(.6f, .03f, .07f, (.2f + beat * .6f) * keepInside.a); GUI.DrawTextureWithTexCoords(corner, CrackGlow, uv, true);
            }
            GUI.color = keepInside;
            // the heartbeat: a soft red glow just outside the rim, swelling twice and resting
            for (int i = 0; i < 4; i++)
            {
                Color pulse = new Color(Blood.r, Blood.g, Blood.b, (.14f + beat * .34f) * (1 - i / 4f)); float o = 1 + i * 2;
                ArcadeSkin.Line(new Rect(panel.x - o, panel.y - o, panel.width + 2 * o, 2), pulse); ArcadeSkin.Line(new Rect(panel.x - o, panel.yMax + o - 2, panel.width + 2 * o, 2), pulse);
                ArcadeSkin.Line(new Rect(panel.x - o, panel.y - o, 2, panel.height + 2 * o), pulse); ArcadeSkin.Line(new Rect(panel.xMax + o - 2, panel.y - o, 2, panel.height + 2 * o), pulse);
            }
            // a red light running round the edge (the same running light as the bond tiles, in blood red)
            if (!still) ArcadeSkin.RunningLight(panel, clock, Ember, .9f);
            // three slow drops seeping from the top edge, each on its own clock, small enough to stay above the text
            float[] at = { .22f, .58f, .86f };
            for (int i = 0; i < at.Length; i++)
            {
                float phase = still ? .5f : Mathf.Repeat(clock * .11f + i * .37f, 1f);
                float length = Mathf.Lerp(3, 16, Mathf.SmoothStep(0, 1, phase)), fade = still ? 1 : Mathf.Clamp01((1 - phase) * 5);
                float x = panel.x + panel.width * at[i];
                Color keep = GUI.color; GUI.color = new Color(Blood.r, Blood.g, Blood.b, .85f * fade * keep.a);
                GUI.DrawTexture(new Rect(x - 1.5f, panel.y, 3, length), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 4, panel.y + length - 4, 8, 8), Drop);
                GUI.color = keep;
            }
        }
        // a soft frame-shaped vignette (transparent centre, dense edges) and tileable value-noise smoke, both built once at runtime
        private static Texture2D vignette, smoke;
        private static Texture2D Vignette
        {
            get
            {
                if (vignette != null) return vignette;
                const int N = 64; vignette = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
                {
                    float e = Mathf.Min(Mathf.Min(x, N - 1 - x), Mathf.Min(y, N - 1 - y)) / (N * .5f); // 0 at the edge, 1 in the middle
                    vignette.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - e * 1.7f), 1.8f)));
                }
                vignette.Apply(); return vignette;
            }
        }
        private static Texture2D Smoke
        {
            get
            {
                if (smoke != null) return smoke;
                const int N = 128; smoke = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, hideFlags = HideFlags.HideAndDontSave };
                var random = new System.Random(4471); var grid = new float[8, 8];
                for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) grid[x, y] = (float)random.NextDouble();
                float Sample(float u, float v, int cells)
                {
                    float fx = u * cells, fy = v * cells; int x0 = Mathf.FloorToInt(fx), y0 = Mathf.FloorToInt(fy);
                    float tx = Mathf.SmoothStep(0, 1, fx - x0), ty = Mathf.SmoothStep(0, 1, fy - y0);
                    float G(int gx, int gy) => grid[((gx % cells) + cells) % cells % 8, ((gy % cells) + cells) % cells % 8];
                    return Mathf.Lerp(Mathf.Lerp(G(x0, y0), G(x0 + 1, y0), tx), Mathf.Lerp(G(x0, y0 + 1), G(x0 + 1, y0 + 1), tx), ty);
                }
                for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
                {
                    float u = x / (float)N, v = y / (float)N;
                    float n = Sample(u, v, 4) * .55f + Sample(u, v, 8) * .45f;
                    smoke.SetPixel(x, y, new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.Clamp01((n - .38f) * 2.2f))));
                }
                smoke.Apply(); return smoke;
            }
        }
        private static Texture2D drop;
        private static Texture2D Drop
        {
            get
            {
                if (drop != null) return drop;
                drop = new Texture2D(16, 16, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(8, 8)) / 8f;
                    drop.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1 - d) * 3)));
                }
                drop.Apply(); return drop;
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
            LocalizedGUI.Label(new Rect(r.x, r.y + r.height * .48f, r.width, r.height * .3f), "−" + price, sealText);
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
