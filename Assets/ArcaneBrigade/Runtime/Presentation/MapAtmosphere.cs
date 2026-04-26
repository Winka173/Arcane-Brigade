using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Each map's own air (owner, 22/10/2026), painted in code from soft runtime sprites, purely visual:
    /// Waiting Room - warm sepia, low fog drifting along the floor, dust motes in the lamp light;
    /// Post Office - cool blue, window light shafts, paper scraps and ash-like flecks on a draught;
    /// Lamp Hall - deep night on the floor that the lamps' own light cuts through, moths' wing glints round the lit lamps;
    /// Archive Vault - heavy dusk haze, the clock's slow ticking flicker, ink motes drifting up.
    /// The colour grade, vignette, fog, haze and shafts sit on the floor UNDER the map's floor marks, telegraphs and bodies,
    /// so darkness never hides an enemy, a bullet or a warning; only tiny motes float among the units. Counts are capped and
    /// follow AnimationDetailBudget; Reduced Effects keeps a still, particle-free version; Reduced Motion stops all drift;
    /// Reduced Flashes stops the flicker and blinking.</summary>
    public sealed class MapAtmosphere : MonoBehaviour
    {
        // MapEnvironmentVisual's floor marks and the lamps' light are at -40: the grade stays under them
        private const int TintOrder = -46, VignetteOrder = -45, HazeOrder = -44, AirOrder = 2140;
        private static Sprite vignette, flat;
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        private int used;
        private bool still, sparse, calm;
        private float t;
        private Vector2 centre; private float halfW, halfH;
        /// <summary>Pieces drawn last frame (read by checks and captures).</summary>
        public int Pieces => used;

        public void Display(CombatSimulation simulation)
        {
            used = 0;
            MapEnvironmentState s = simulation?.MapEnvironment;
            Camera cam = Camera.main;
            bool live = s != null && cam != null && cam.orthographic && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (live)
            {
                Ensure();
                still = PresentationSettings.ReducedMotion; sparse = PresentationSettings.ReducedEffects; calm = PresentationSettings.ReducedFlashes;
                t = still || sparse ? 0 : simulation.Elapsed;
                halfH = cam.orthographicSize; halfW = halfH * cam.aspect; centre = cam.transform.position;
                int level = AnimationDetailBudget.Level;
                int motes = sparse ? 0 : level >= 2 ? 6 : level == 1 ? 12 : 20;
                switch (s.Theme)
                {
                    case MapTheme.WaitingRoom: WaitingRoom(motes); break;
                    case MapTheme.PostOffice: PostOffice(motes); break;
                    case MapTheme.LampHall: LampHall(s, motes, simulation.Elapsed); break;
                    default: ArchiveVault(motes, simulation.Elapsed); break;
                }
            }
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }

        // ------------------------------------------------------------------ maps
        private void WaitingRoom(int motes)
        {
            Grade(new Color(.96f, .84f, .66f, .07f), new Color(.2f, .11f, .05f, .32f));
            // low fog banks along the floor, drifting slowly across
            for (int i = 0; i < 5; i++)
            {
                float y = Mathf.Lerp(-.85f, .55f, i / 4f) * halfH;
                Vector2 at = Wrap(new Vector2(H(i, 1) * 30, 0), new Vector2(.18f + .06f * H(i, 2), 0)); at.y = centre.y + y;
                Put(MechanicInk.Glow, at, new Vector2(halfW * 1.1f, 1.3f + H(i, 3) * .6f), 0, new Color(.93f, .9f, .86f, .13f), HazeOrder);
            }
            // dust motes turning in the lamp light
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 4) * 40, H(i, 5) * 40), new Vector2(.05f * (H(i, 6) - .5f), .07f + .05f * H(i, 7)));
                at.x += still ? 0 : Mathf.Sin(t * .7f + i) * .15f;
                float twinkle = calm || still ? .7f : .55f + .45f * Mathf.Sin(t * 1.3f + i * 2.1f);
                Put(MechanicInk.Glow, at, Vector2.one * (.07f + .06f * H(i, 8)), 0, new Color(1, .93f, .72f, .42f * twinkle), AirOrder);
            }
        }

        private void PostOffice(int motes)
        {
            Grade(new Color(.68f, .8f, 1f, .08f), new Color(.03f, .06f, .16f, .34f));
            // cool window light falling in long soft shafts from the upper left
            for (int i = 0; i < 3; i++)
            {
                float breathe = still || calm ? 1 : .8f + .2f * Mathf.Sin(t * .4f + i * 1.7f);
                Vector2 at = centre + new Vector2((-.55f + i * .5f) * halfW, .1f * halfH);
                Put(MechanicInk.Glow, at, new Vector2(1.5f + .4f * i, halfH * 3f), -28, new Color(.74f, .88f, 1f, .12f * breathe), HazeOrder);
            }
            // paper scraps fluttering down and ash-like flecks on the draught
            for (int i = 0; i < motes; i++)
            {
                bool scrap = i % 3 == 0;
                Vector2 at = Wrap(new Vector2(H(i, 1) * 40, H(i, 2) * 40), scrap ? new Vector2(.25f, -.35f - .15f * H(i, 3)) : new Vector2(.45f, -.22f - .1f * H(i, 3)));
                at.x += still ? 0 : Mathf.Sin(t * 1.6f + i) * (scrap ? .25f : .08f);
                if (scrap) Put(flat, at, new Vector2(.15f, .1f * (still ? 1 : .4f + .6f * Mathf.Abs(Mathf.Cos(t * 2.2f + i)))), still ? 15 : t * 60 + i * 40, new Color(.96f, .94f, .87f, .6f), AirOrder);
                else Put(MechanicInk.Glow, at, Vector2.one * (.06f + .04f * H(i, 4)), 0, new Color(.72f, .74f, .8f, .38f), AirOrder);
            }
        }

        private void LampHall(MapEnvironmentState s, int motes, float clock)
        {
            // deep night on the floor; the lamps' painted light (floor order -40) cuts through it
            // the round floor around the lamps: its edges sink deeper into the dark (owner 23/10/2026; edge was .5)
            Grade(new Color(.04f, .04f, .12f, .36f), new Color(.01f, .01f, .05f, .8f));
            int glints = 0;
            for (int i = 0; i < s.Lamps.Count && glints < Mathf.Max(motes, 4); i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                if (!lamp.Lit) continue;
                // moths' wing glints circling the light
                for (int k = 0; k < 2 && glints < Mathf.Max(motes, 4); k++, glints++)
                {
                    float a = (still || sparse ? 0 : clock * (1.1f + .3f * k)) + i * 2.3f + k * 3.1f;
                    Vector2 at = lamp.Base + new Vector2(Mathf.Cos(a) * (.5f + .2f * k), 2.3f + Mathf.Sin(a * 1.7f) * .3f);
                    float blink = calm || still || sparse ? .5f : Mathf.Pow(Mathf.Abs(Mathf.Sin(clock * 9 + i * 1.3f + k * 2f)), 4);
                    Put(MechanicInk.Glow, at, Vector2.one * .16f, 0, new Color(1, .96f, .85f, .8f * blink), AirOrder);
                }
            }
        }

        private void ArchiveVault(int motes, float clock)
        {
            // the vault clock's slow tick: a faint lift of the dusk light on each second (none with Reduced Flashes or Motion)
            float tick = calm || still || sparse ? 0 : Mathf.Exp(-Mathf.Repeat(clock, 1f) * 5f);
            Grade(new Color(.86f, .58f, .48f, .1f + .035f * tick), new Color(.13f, .05f, .15f, .4f));
            // heavy haze banks
            for (int i = 0; i < 4; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 1) * 30, 0), new Vector2(-.08f, 0)); at.y = centre.y + Mathf.Lerp(-.7f, .7f, i / 3f) * halfH;
                Put(MechanicInk.Glow, at, new Vector2(halfW * 1.3f, 2.2f), 0, new Color(.74f, .6f, .7f, .12f), HazeOrder);
            }
            // ink motes drifting up
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 2) * 40, H(i, 3) * 40), new Vector2(.04f * (H(i, 4) - .5f), .16f + .1f * H(i, 5)));
                at.x += still ? 0 : Mathf.Sin(t * .9f + i * 1.7f) * .12f;
                Put(MechanicInk.Glow, at, Vector2.one * (.08f + .06f * H(i, 6)), 0, new Color(.16f, .09f, .26f, .5f), AirOrder);
            }
        }

        // ------------------------------------------------------------------ helpers
        private static Color gradeTint, gradeEdge; private static int gradeFrame = -10; private static Vector2 gradeCentre, gradeHalf = Vector2.one;
        /// <summary>The light a standing prop at <paramref name="at"/> should take so it sits in the same air as the floor under it
        /// (owner 22/10/2026: props stayed bright on a dark floor): the map's grade and its darker edges, as a multiply tint.
        /// White when no map air is drawn (the shop, a secret room).</summary>
        public static Color PropShade(Vector2 at)
        {
            if (Time.frameCount - gradeFrame > 2) return Color.white;
            Color c = Color.Lerp(Color.white, new Color(gradeTint.r, gradeTint.g, gradeTint.b), Mathf.Clamp01(gradeTint.a * 1.5f));
            Vector2 d = new Vector2((at.x - gradeCentre.x) / Mathf.Max(.01f, gradeHalf.x), (at.y - gradeCentre.y) / Mathf.Max(.01f, gradeHalf.y));
            float edge = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.45f, 1.15f, d.magnitude)) * gradeEdge.a;
            c = Color.Lerp(c, new Color(gradeEdge.r * c.r, gradeEdge.g * c.g, gradeEdge.b * c.b), edge);
            c.a = 1; return c;
        }
        /// <summary>The map's colour grade on the floor: a tint over the whole view and a soft vignette at its edges.</summary>
        private void Grade(Color tint, Color edge)
        {
            gradeTint = tint; gradeEdge = edge; gradeFrame = Time.frameCount; gradeCentre = centre; gradeHalf = new Vector2(halfW, halfH);
            Vector2 size = new Vector2(halfW * 2.2f, halfH * 2.2f);
            Put(flat, centre, size, 0, tint, TintOrder);
            Put(vignette, centre, size, 0, edge, VignetteOrder);
        }
        /// <summary>A world-anchored point moving at velocity, wrapped into the view so it drifts across and comes back.</summary>
        private Vector2 Wrap(Vector2 seed, Vector2 velocity)
        {
            float x0 = centre.x - halfW - .5f, y0 = centre.y - halfH - .5f, w = halfW * 2 + 1, h = halfH * 2 + 1;
            return new Vector2(x0 + Mathf.Repeat(seed.x + velocity.x * t - x0, w), y0 + Mathf.Repeat(seed.y + velocity.y * t - y0, h));
        }
        private static float H(int i, int k) => Mathf.Repeat(Mathf.Sin(i * 12.9898f + k * 78.233f) * 43758.5453f, 1);

        private void Put(Sprite sprite, Vector2 at, Vector2 size, float angle, Color color, int order)
        {
            if (sprite == null || color.a <= .004f) return;
            if (used == pool.Count)
            {
                var go = new GameObject("Map atmosphere " + used); go.transform.SetParent(transform, false);
                pool.Add(go.AddComponent<SpriteRenderer>());
            }
            SpriteRenderer r = pool[used++];
            r.enabled = true; r.sprite = sprite; r.color = color; r.sortingOrder = order;
            Vector2 unit = sprite.bounds.size;
            r.transform.position = new Vector3(at.x, at.y, 0); r.transform.rotation = Quaternion.Euler(0, 0, angle);
            r.transform.localScale = new Vector3(size.x / Mathf.Max(.001f, unit.x), size.y / Mathf.Max(.001f, unit.y), 1);
        }

        private static void Ensure()
        {
            if (flat == null)
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "MapAtmosphere flat", hideFlags = HideFlags.DontSave };
                var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px); tex.Apply(false, true);
                flat = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 4, 0, SpriteMeshType.FullRect);
            }
            if (vignette == null)
            {
                const int n = 64;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "MapAtmosphere vignette", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float dx = (x + .5f) / n * 2 - 1, dy = (y + .5f) / n * 2 - 1, r = Mathf.Sqrt(dx * dx + dy * dy) / 1.2f;
                        float a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.5f, 1f, r));
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                    }
                tex.SetPixels32(px); tex.Apply(false, true);
                vignette = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), n, 0, SpriteMeshType.FullRect);
            }
        }
    }
}
