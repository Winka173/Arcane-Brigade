using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Each map's own air (requirement 22/10/2026), painted in code from soft runtime sprites, purely visual:
    /// Waiting Room - warm sepia, low fog drifting along the floor, dust motes in the lamp light;
    /// Post Office - cool blue, window light shafts, paper scraps and ash-like flecks on a draught;
    /// Lamp Hall - deep night on the floor that the lamps' own light cuts through, moths' wing glints round the lit lamps;
    /// Archive Vault - heavy dusk haze, the clock's slow ticking flicker, ink motes drifting up.
    /// The colour grade, vignette, fog, haze and shafts sit on the floor UNDER the map's floor marks, telegraphs and bodies,
    /// so darkness never hides an enemy, a bullet or a warning; only tiny motes float among the units. Counts are capped and
    /// follow AnimationDetailBudget; Reduced Effects keeps a still, particle-free version; Reduced Motion stops all drift;
    /// Reduced Flashes stops the flicker and blinking.
    /// Requirement 25/10/2026: each map's air is in its own file under Presentation/Maps (Air{Theme}); the Glasshouse and the
    /// Theatre borrow their stand-in's air (MapIdentity.StandIn) until their own is written.</summary>
    public sealed partial class MapAtmosphere : MonoBehaviour
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
                    case MapTheme.WaitingRoom: AirWaitingRoom(s, motes, simulation.Elapsed); break;
                    case MapTheme.PostOffice: AirPostOffice(s, motes, simulation.Elapsed); break;
                    case MapTheme.Glasshouse: AirGlasshouse(s, motes, simulation.Elapsed); break;
                    case MapTheme.LampHall: AirLampHall(s, motes, simulation.Elapsed); break;
                    case MapTheme.Theatre: AirTheatre(s, motes, simulation.Elapsed); break;
                    case MapTheme.ArchiveVault: AirArchiveVault(s, motes, simulation.Elapsed); break;
                }
            }
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }

        // ------------------------------------------------------------------ helpers
        private static Color gradeTint, gradeEdge; private static int gradeFrame = -10; private static Vector2 gradeCentre, gradeHalf = Vector2.one;
        /// <summary>The light a standing prop at <paramref name="at"/> should take so it sits in the same air as the floor under it
        /// (requirement 22/10/2026: props stayed bright on a dark floor): the map's grade and its darker edges, as a multiply tint.
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
