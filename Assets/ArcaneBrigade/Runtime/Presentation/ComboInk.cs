using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The friend combos on screen (requirement 25/10/2026): small inked marks on the enemies a combo has touched, its
    /// patches of floor (steam, dust, thorns, a lit lane, a breath of tea), its bolts, shards and splashes drawn in ink, and the
    /// painted things that cross the field for the bigger groups (Resources/Combos: a ghost ship, a giant book, a storm cloud,
    /// an oven and its loaves, a parade float, a clock face, an ice-cream avalanche, a carousel, a letter). A group's name pops
    /// up when its move plays.</summary>
    public static class ComboInk
    {
        private struct Moment { public ComboId Id; public Vector2 From, To; public float Born; public bool Impact; }
        private static readonly List<Moment> moments = new List<Moment>();
        private static readonly Dictionary<int, SpriteRenderer> actorViews = new Dictionary<int, SpriteRenderer>();
        private static readonly HashSet<int> seen = new HashSet<int>();
        private static readonly List<int> gone = new List<int>();
        private static readonly Dictionary<ComboId, float> namedAt = new Dictionary<ComboId, float>();
        private static readonly Dictionary<string, Sprite> art = new Dictionary<string, Sprite>();
        private static readonly List<Vector2> path = new List<Vector2>();
        private static InkCanvas floor, ink, light;
        private static GameObject holder;
        private const float MomentLife = .45f;

        public static void Observe(IReadOnlyList<CombatEvent> events)
        {
            foreach (CombatEvent e in events)
            {
                if (e.Kind == CombatEventKind.ComboMoment && moments.Count < 60)
                {
                    int id = Mathf.FloorToInt(e.Value);
                    moments.Add(new Moment { Id = (ComboId)id, From = e.Position, To = e.Direction, Born = Time.time, Impact = e.Value - id > .25f });
                }
                else if (e.Kind == CombatEventKind.ComboNamed)
                {
                    var id = (ComboId)Mathf.RoundToInt(e.Value);
                    if (namedAt.TryGetValue(id, out float at) && Time.time - at < 6) continue;
                    namedAt[id] = Time.time;
                    ImpactWords.Pop(Combos.Of(id).Name.ToUpperInvariant(), e.Position + Vector2.up * 1.2f, Combos.Of(id).Members.Length >= 4 ? ImpactWords.Gold : ImpactWords.Cream, 2, Combos.Of(id).Members.Length >= 4 ? 1.25f : 1);
                }
            }
        }

        private static Sprite Art(string name)
        {
            if (!art.TryGetValue(name, out Sprite s)) art[name] = s = Resources.Load<Sprite>("Combos/Combo_" + name);
            return s;
        }

        public static void Draw(CombatSimulation simulation)
        {
            float now = Time.time;
            moments.RemoveAll(m => now - m.Born > MomentLife);
            bool any = simulation != null && (simulation.ComboMarks.Count > 0 || simulation.ComboPatches.Count > 0 || simulation.ComboActors.Count > 0) || moments.Count > 0;
            if (!any)
            {
                if (holder != null && (ink.Visible || floor.Visible || light.Visible)) { floor.Hide(); ink.Hide(); light.Hide(); }
                ClearActors(); return;
            }
            if (holder == null)
            {
                holder = new GameObject("Combo ink");
                floor = new InkCanvas(holder.transform, "Combo floor", 70); ink = new InkCanvas(holder.transform, "Combo ink", 2104); light = new InkCanvas(holder.transform, "Combo light", 2106, true);
                actorViews.Clear();
            }
            floor.Clear(); ink.Clear(); light.Clear();
            bool still = PresentationSettings.ReducedMotion;
            float sim = simulation.Elapsed;
            foreach (ComboMark m in simulation.ComboMarks) if (m.Until > sim && m.Target.Alive) DrawMark(m, now, sim);
            foreach (ComboPatch p in simulation.ComboPatches) if (p.Until > sim) DrawPatch(p, sim, still);
            foreach (Moment m in moments) DrawMoment(m, Mathf.Clamp01((now - m.Born) / MomentLife));
            DrawActors(simulation, sim, still);
            floor.Commit(); ink.Commit(); light.Commit();
        }

        // ---- marks: a small token over the enemy a combo has touched ------------------------------------------------------
        private static void DrawMark(ComboMark m, float now, float sim)
        {
            Vector2 head = m.Target.Position + Vector2.up * Mathf.Clamp(m.Target.Definition.BodyHeight * .7f, .45f, 1.6f);
            float fade = Mathf.Clamp01((m.Until - sim) / .25f), bob = Mathf.Sin(now * 4 + m.Target.Id) * .04f;
            Vector2 c = head + Vector2.up * bob;
            switch (m.Kind)
            {
                case ComboMarkKind.Bubble:
                    ink.Stroke(InkCanvas.Arc(c, .2f, .2f, 0, 360), InkStroke.Of(ArsenalInk.Water, .06f, .9f * fade, m.Target.Id));
                    light.GlowRibbon(InkCanvas.Arc(c, .13f, .13f, 110, 160), 1, 0, .05f, new Color(.9f, 1f, 1f, .8f * fade)); break;
                case ComboMarkKind.Film:
                    for (int s = -1; s <= 1; s += 2) ink.Stroke(InkCanvas.Line(c + new Vector2(-.32f, s * .11f), c + new Vector2(.32f, s * .11f)), InkStroke.Of(ArsenalInk.Midnight, .07f, .9f * fade, s));
                    for (int k = 0; k < 4; k++) ink.Stroke(InkCanvas.Line(c + new Vector2(-.24f + k * .16f, -.05f), c + new Vector2(-.24f + k * .16f, .05f)), InkStroke.Of(ArsenalInk.Cream, .05f, .9f * fade, k)); break;
                case ComboMarkKind.Thread:
                    ink.Stroke(InkCanvas.Line(c + new Vector2(-.18f, -.18f), c + new Vector2(.18f, .18f)), InkStroke.Of(ArsenalInk.Wine, .06f, fade, 1));
                    ink.Stroke(InkCanvas.Line(c + new Vector2(-.18f, .18f), c + new Vector2(.18f, -.18f)), InkStroke.Of(ArsenalInk.Wine, .06f, fade, 2)); break;
                case ComboMarkKind.Dream:
                    for (int k = 0; k < 2; k++) { Vector2 z = c + new Vector2(.12f * k, .18f * k + Mathf.Repeat(now * .5f, .3f)); float w = .12f - .03f * k; ink.Stroke(Zed(z, w), InkStroke.Of(ArsenalInk.Cream, .045f, fade, k)); } break;
                case ComboMarkKind.Lit:
                    ink.Stroke(InkCanvas.Line(c + Vector2.down * .1f, c + Vector2.up * .18f), InkStroke.Of(ArsenalInk.Amber, .16f, fade, 3));
                    light.GlowRibbon(InkCanvas.Line(c, c + Vector2.up * .14f), 1, 0, .12f, new Color(1f, .85f, .5f, .8f * fade)); break;
                case ComboMarkKind.Chill:
                    for (int k = 0; k < 3; k++) { float a = k * 60 * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .16f; ink.Stroke(InkCanvas.Line(c - d, c + d), InkStroke.Of(ArsenalInk.Glass, .05f, fade, k)); } break;
                case ComboMarkKind.Crumb:
                    for (int k = 0; k < 4; k++) ink.Stroke(InkCanvas.Line(c + new Vector2(-.2f + k * .13f, (k % 2) * .08f), c + new Vector2(-.15f + k * .13f, (k % 2) * .08f + .03f)), InkStroke.Of(ArsenalInk.Leather, .08f, fade, k)); break;
                case ComboMarkKind.Water:
                    ink.Stroke(InkCanvas.Line(c + Vector2.up * .18f, c + Vector2.down * .08f), InkStroke.Of(ArsenalInk.Water, .14f, fade, 4)); break;
                case ComboMarkKind.Star:
                    ink.Stroke(Star(c, .2f), InkStroke.Of(ArsenalInk.Gold, .05f, fade, 5), true);
                    light.GlowRibbon(InkCanvas.Line(c - Vector2.right * .1f, c + Vector2.right * .1f), 1, 0, .14f, new Color(1f, .9f, .6f, .7f * fade)); break;
                case ComboMarkKind.Boba:
                    for (int k = 0; k < Mathf.Max(1, m.Stacks); k++) ink.Stroke(InkCanvas.Arc(c + Vector2.right * (k - (m.Stacks - 1) * .5f) * .16f, .05f, .05f, 0, 360), InkStroke.Of(ArsenalInk.Midnight, .07f, fade, k)); break;
                case ComboMarkKind.Steeped:
                    for (int k = 0; k < 2; k++) ink.Stroke(InkCanvas.Line(c + new Vector2(-.08f + k * .16f, -.1f), c + new Vector2(-.04f + k * .16f, .2f), .04f, 3, false, k), InkStroke.Of(ArsenalInk.Dust, .05f, .8f * fade, k)); break;
                case ComboMarkKind.Clocked:
                    ink.Stroke(InkCanvas.Arc(c, .17f, .17f, 0, 360), InkStroke.Of(ArsenalInk.Cream, .05f, fade, 6));
                    ink.Stroke(InkCanvas.Line(c, c + Vector2.up * .13f), InkStroke.Of(ArsenalInk.Midnight, .04f, fade, 7)); break;
            }
        }
        private static List<Vector2> Zed(Vector2 c, float w) { path.Clear(); path.Add(c + new Vector2(-w, w)); path.Add(c + new Vector2(w, w)); path.Add(c + new Vector2(-w, -w)); path.Add(c + new Vector2(w, -w)); return path; }
        private static List<Vector2> Star(Vector2 c, float r)
        {
            path.Clear();
            for (int k = 0; k < 5; k++) { float a = (90 + k * 144) * Mathf.Deg2Rad; path.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r); }
            return path;
        }

        // ---- patches of floor ---------------------------------------------------------------------------------------------
        private static void DrawPatch(ComboPatch p, float sim, bool still)
        {
            float life = Mathf.Clamp01((p.Until - sim) / .4f) * Mathf.Clamp01((sim - p.Born) / .2f);
            int seed = Mathf.RoundToInt(p.A.x * 13 + p.A.y * 7);
            switch (p.Kind)
            {
                case ComboPatchKind.Steam:
                    for (int k = 0; k < 4; k++)
                    {
                        float a = (seed + k * 90) * Mathf.Deg2Rad, rise = still ? .3f : Mathf.Repeat((sim - p.Born) * .6f + k * .25f, 1);
                        Vector2 b = p.A + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * p.Radius * .6f + Vector2.up * rise * .8f;
                        light.GlowRibbon(InkCanvas.Line(b, b + Vector2.up * .5f, .08f, 2, false, k), 1, 0, .18f, new Color(1f, .97f, .9f, .55f * life * (1 - rise)));
                    }
                    break;
                case ComboPatchKind.Dust:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = (seed * 3 + k * 40) * Mathf.Deg2Rad, r = p.Radius * ((k * 37 % 10) / 10f);
                        Vector2 d = p.A + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * .6f);
                        light.GlowRibbon(InkCanvas.Line(d, d + Vector2.right * .06f), 1, 0, .1f, new Color(1f, .88f, .5f, .7f * life));
                    }
                    break;
                case ComboPatchKind.Lane:
                    floor.Band(p.A, p.B, p.Radius, new Color(1f, .86f, .5f, .28f * life), new Color(1f, .86f, .5f, 0));
                    light.GlowRibbon(InkCanvas.Line(p.A, p.B), 1, 0, p.Radius * .7f, new Color(1f, .93f, .7f, .45f * life));
                    break;
                case ComboPatchKind.Thorns:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = (seed + k * 40) * Mathf.Deg2Rad, r = p.Radius * (.25f + .7f * ((k * 53 % 10) / 10f));
                        Vector2 b = p.A + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * .6f);
                        float h = .35f + .15f * (k % 3); Vector2 tip = b + new Vector2(.08f * ((k % 2) * 2 - 1), h * Mathf.Min(1, (sim - p.Born) * 4));
                        floor.Stroke(InkCanvas.Line(b, tip), InkStroke.Of(ArsenalInk.Green, .12f, life, k));
                        floor.Stroke(InkCanvas.Line(tip, tip + new Vector2(.06f, -.05f)), InkStroke.Of(ArsenalInk.Leather, .05f, life, k + 9));
                    }
                    break;
                case ComboPatchKind.TeaCone:
                {
                    Vector2 d = p.B - p.A; float mid = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, reach = d.magnitude;
                    floor.Sector(p.A, reach, mid - p.Radius, mid + p.Radius, new Color(.85f, .62f, .38f, .1f * life), new Color(.85f, .62f, .38f, .32f * life));
                    for (int k = -1; k <= 1; k++)
                    {
                        float a = (mid + k * p.Radius * .6f) * Mathf.Deg2Rad; Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                        light.GlowRibbon(InkCanvas.Line(p.A + dir * .4f, p.A + dir * reach, .15f, 1.5f, false, k), 1, 0, .25f, new Color(1f, .93f, .82f, .45f * life));
                    }
                    break;
                }
            }
        }

        // ---- moments: bolts, shards, splashes, bursts -----------------------------------------------------------------------
        private static void DrawMoment(Moment m, float t)
        {
            float fade = 1 - t;
            switch (m.Id)
            {
                case ComboId.ChargedBubbles when m.Impact:
                    for (int k = 0; k < 6; k++) { float a = k * 60 * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); ink.Stroke(InkCanvas.Line(m.From + d * (.2f + .3f * t), m.From + d * (.35f + .4f * t)), InkStroke.Of(ArsenalInk.Water, .06f, fade, k)); }
                    break;
                case ComboId.ChargedBubbles: case ComboId.CometStrike: case ComboId.StaticNotes: case ComboId.StormFront:
                    Bolt(m.From, m.To, fade, (int)m.Id * 7 + Mathf.RoundToInt(m.To.x * 3)); break;
                case ComboId.BrainFreeze:
                {
                    Vector2 p = Vector2.Lerp(m.From, m.To, Mathf.Min(1, t * 2.2f)), d = (m.To - m.From).normalized * .3f;
                    ink.Stroke(InkCanvas.Line(p - d, p), InkStroke.Of(ArsenalInk.Glass, .1f, fade, 3));
                    light.GlowRibbon(InkCanvas.Line(p - d, p), 1, 0, .08f, new Color(.85f, .97f, 1f, .8f * fade)); break;
                }
                case ComboId.MirrorSpice:
                {
                    Vector2 d = (m.To - m.From); Vector2 n = new Vector2(-d.y, d.x).normalized * .35f;
                    ink.Stroke(InkCanvas.Line(m.To - n, m.To + n, .05f, 0, false, 2), InkStroke.Of(ArsenalInk.Wine, .12f, fade, 4));
                    light.GlowRibbon(InkCanvas.Line(m.From, m.To), 1, Mathf.Min(1, t * 1.5f), .06f, new Color(1f, .7f, .6f, .6f * fade)); break;
                }
                case ComboId.Airmail:
                    for (int k = 0; k < 7; k++) { float a = (k * 51 + 20) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .7f); ink.Stroke(InkCanvas.Line(m.From + d * .3f * t, m.From + d * (.3f + .8f * t)), InkStroke.Of(ArsenalInk.Midnight, .09f * fade + .02f, fade, k)); }
                    break;
                case ComboId.Crumbs:
                    for (int k = 0; k < 8; k++) { float a = (k * 45 + 10) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * (.3f + 1.1f * t); ink.Stroke(InkCanvas.Line(m.From + d, m.From + d * 1.08f), InkStroke.Of(ArsenalInk.Leather, .1f, fade, k)); }
                    break;
                case ComboId.StitchedLetter:
                    for (int k = 0; k < 3; k++) { Vector2 o = new Vector2(-.3f + k * .3f, -.35f); ink.Stroke(InkCanvas.Line(m.From + o, m.From + o + new Vector2(.15f, .25f)), InkStroke.Of(ArsenalInk.Wine, .06f, fade, k)); }
                    break;
                case ComboId.SproutingVines:
                    ink.Stroke(InkCanvas.Arc(m.From + Vector2.down * .1f, .4f, .2f, 180, 180 + 300 * Mathf.Min(1, t * 2)), InkStroke.Of(ArsenalInk.Green, .1f, fade, 5)); break;
                case ComboId.ThornDust:
                    for (int k = 0; k < 6; k++) { float a = (k * 60) * Mathf.Deg2Rad; Vector2 b = m.From + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * .5f; ink.Stroke(InkCanvas.Line(b, b + Vector2.up * (.5f * Mathf.Min(1, t * 3))), InkStroke.Of(ArsenalInk.Green, .1f, fade, k)); }
                    break;
                case ComboId.ScribbleBox:
                case ComboId.Carousel:
                {
                    float rise = Mathf.Sin(Mathf.Min(1, t * 1.6f) * Mathf.PI * .5f); Vector2 top = m.From + Vector2.up * 1.6f * rise;
                    if (m.Id == ComboId.ScribbleBox) { ink.Stroke(InkCanvas.Line(m.From, top, .25f, 0, true, 3), InkStroke.Of(ArsenalInk.Gold, .1f, fade, 3)); ink.Stroke(InkCanvas.Arc(top + Vector2.up * .2f, .16f, .16f, 0, 360), InkStroke.Of(ArsenalInk.Wine, .24f, fade, 4)); }
                    for (int k = 0; k < 8; k++) { float a = k * 45 * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f); ink.Stroke(InkCanvas.Line(m.From + d * (.6f + 1.4f * t), m.From + d * (.9f + 1.6f * t)), InkStroke.Of(ArsenalInk.Cream, .06f, fade, k)); }
                    break;
                }
                case ComboId.Lullaby:
                    ink.Stroke(Zed(m.From + Vector2.up * (1 + t * .6f), .18f), InkStroke.Of(ArsenalInk.Cream, .07f, fade, 1)); break;
                case ComboId.FrostSundae:
                case ComboId.WinterFeast:
                    for (int k = 0; k < 8; k++) { float a = k * 45 * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * (m.Id == ComboId.WinterFeast ? 2 : .8f); ink.Stroke(InkCanvas.Line(m.From + d * t * .6f, m.From + d * (.3f + t * .7f)), InkStroke.Of(ArsenalInk.Glass, .1f, fade, k)); }
                    break;
                case ComboId.ClockworkBand:
                {
                    Vector2 n = m.From + Vector2.up * (1.2f + t * .6f) + Vector2.right * .15f;
                    ink.Stroke(InkCanvas.Arc(n, .09f, .07f, 0, 360), InkStroke.Of(ArsenalInk.Gold, .09f, fade, 2));
                    ink.Stroke(InkCanvas.Line(n + Vector2.right * .08f, n + new Vector2(.08f, .35f)), InkStroke.Of(ArsenalInk.Gold, .05f, fade, 3)); break;
                }
                case ComboId.BeaconLane:
                    light.GlowRibbon(InkCanvas.Line(m.From, m.To), Mathf.Min(1, t * 1.8f), Mathf.Max(0, t * 1.8f - .35f), .35f, new Color(1f, .75f, .6f, .7f * fade)); break;
                case ComboId.Constellation: case ComboId.MidnightLibrary: case ComboId.MidnightClocktower:
                    for (int k = 0; k < 10; k++) { float a = (k * 36 + 8) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * 2.2f; ink.Stroke(InkCanvas.Line(m.From + d * t * .5f, m.From + d * (.25f + t * .6f)), InkStroke.Of(m.Id == ComboId.MidnightLibrary ? ArsenalInk.Cream : ArsenalInk.Gold, .08f, fade, k)); }
                    break;
            }
        }
        private static void Bolt(Vector2 a, Vector2 b, float fade, int seed)
        {
            List<Vector2> line = InkCanvas.Line(a, b, .22f, 0, true, seed);
            light.GlowRibbon(line, 1, 0, .3f, new Color(.55f, .95f, 1f, .5f * fade));
            ink.Stroke(InkCanvas.Line(a, b, .22f, 0, true, seed), InkStroke.Of(ArsenalInk.Cyan, .08f, fade, seed));
        }

        // ---- the painted things in motion -----------------------------------------------------------------------------------
        private static void DrawActors(CombatSimulation simulation, float sim, bool still)
        {
            seen.Clear();
            foreach (ComboActor a in simulation.ComboActors)
            {
                if (sim < a.Born) continue;
                float t = Mathf.Clamp01((sim - a.Born) / Mathf.Max(.01f, a.Duration));
                string name = null; float height = 1; Vector2 at = a.At(sim); float spin = 0, alpha = 1; int order = 2110; bool flip = false;
                switch (a.Kind)
                {
                    case ComboActorKind.Letter: name = "Letter"; height = .75f; at = a.From; flip = a.To.x < a.From.x; break;
                    case ComboActorKind.Ship: name = "GhostShip"; height = 2.4f; alpha = Mathf.Min(1, Mathf.Min(t, 1 - t) * 6) * .9f; order = 1000 - Mathf.RoundToInt(at.y * 12) + 6; at += Vector2.up * (still ? 0 : Mathf.Sin(sim * 5) * .06f); break;
                    case ComboActorKind.Parade: name = "ParadeFloat"; height = 2.6f; alpha = Mathf.Min(1, Mathf.Min(t, 1 - t) * 6); order = 1000 - Mathf.RoundToInt(at.y * 12) + 6; at += Vector2.up * (still ? 0 : Mathf.Abs(Mathf.Sin(sim * 8)) * .08f); break;
                    case ComboActorKind.Book: name = "GiantBook"; height = 2.2f; spin = (1 - t) * 25; break;
                    case ComboActorKind.Cloud: name = "StormCloud"; height = 1.7f; alpha = Mathf.Min(1, Mathf.Min(t, 1 - t) * 5); at = simulation.Champion.Position + Vector2.up * 3 + Vector2.right * (still ? 0 : Mathf.Sin(sim * 1.5f) * .3f); break;
                    case ComboActorKind.Oven: name = "BakerOven"; height = 2; alpha = Mathf.Min(1, Mathf.Min(t, 1 - t) * 5); at = a.From; flip = a.To.x < a.From.x; order = 1000 - Mathf.RoundToInt(at.y * 12) + 6; break;
                    case ComboActorKind.Loaf: name = "BreadLoaf"; height = .7f; flip = a.To.x < a.From.x; spin = still ? 0 : sim * 400 * (flip ? 1 : -1); break;
                    case ComboActorKind.Clock: name = "ClockFace"; height = 2.4f; alpha = Mathf.Min(1, Mathf.Min(t * 4, (1 - t) * 4)); at = simulation.Champion.Position + Vector2.up * 3.5f; break;
                    case ComboActorKind.Avalanche: name = "Avalanche"; height = 3.2f; at = Vector2.Lerp(a.From, a.To, t * t); break;
                    case ComboActorKind.Carousel: name = "Carousel"; height = 2.6f; alpha = Mathf.Min(1, Mathf.Min(t * 6, (1 - t) * 6)); order = 1000 - Mathf.RoundToInt(at.y * 12) + 6; at += Vector2.up * .6f; break;
                    case ComboActorKind.Star:
                    {
                        // a falling star drawn in ink: a gold star and its trail
                        Vector2 p = Vector2.Lerp(a.From, a.To, t * t);
                        light.GlowRibbon(InkCanvas.Line(a.From, p), 1, Mathf.Max(0, t - .4f), .3f, new Color(1f, .9f, .6f, .7f));
                        ink.Stroke(Star(p, .45f), InkStroke.Of(ArsenalInk.Gold, .12f, 1, 9), true);
                        continue;
                    }
                    default: continue;
                }
                Sprite sprite = Art(name); if (sprite == null) continue;
                seen.Add(a.Id);
                if (!actorViews.TryGetValue(a.Id, out SpriteRenderer view) || view == null)
                {
                    view = new GameObject("Combo " + name).AddComponent<SpriteRenderer>(); view.transform.SetParent(holder.transform, false);
                    view.sprite = sprite; actorViews[a.Id] = view;
                    view.transform.localScale = Vector3.one * (height / Mathf.Max(.01f, sprite.bounds.size.y));
                }
                view.sortingOrder = order; view.flipX = flip;
                view.transform.SetPositionAndRotation(at, Quaternion.Euler(0, 0, spin));
                view.color = new Color(1, 1, 1, alpha);
            }
            gone.Clear();
            foreach (var pair in actorViews) if (!seen.Contains(pair.Key)) gone.Add(pair.Key);
            foreach (int id in gone) { if (actorViews[id] != null) Object.Destroy(actorViews[id].gameObject); actorViews.Remove(id); }
        }
        private static void ClearActors()
        {
            foreach (var pair in actorViews) if (pair.Value != null) Object.Destroy(pair.Value.gameObject);
            actorViews.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { moments.Clear(); actorViews.Clear(); namedAt.Clear(); art.Clear(); holder = null; floor = ink = light = null; }
    }
}
