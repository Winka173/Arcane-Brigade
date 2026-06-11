using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A picture only (requirement 24/10/2026: "only effects to look at"): the game's damage elements stay Physical / Ember /
    /// Frost / Arcane; each hero's blows simply land with its own look.</summary>
    public enum ElementLook { None, Fire, Ice, Water, Storm, Void, Light, Wind, Poison, Earth, Bloom, Crystal, Gold, Paper, Thread, Sound, Time, Dream }

    /// <summary>Each hero's skill and mechanic blows (never basic attacks: friends and heroes attack alike, requirement 24/10/2026) land with a painted burst of its look (fire licks, ice shards, water drops, a forking spark, void
    /// specks drawn in, light rays, a gust with leaves, poison bubbles, flung chips of earth, petals, prismatic glass, coins,
    /// paper scraps, a snapping thread, notes and sound arcs, sand and a sweeping clock hand, dream stars and a crescent). Drawn in
    /// ink in code over the struck enemy, short and throttled; never a ring, disc or pool; friendly colours only (never violet).</summary>
    public sealed class ElementInk : MonoBehaviour
    {
        public static ElementLook For(string hero) => hero switch
        {
            "CopperCauldron" or "CometRunner" or "SupperMortar" => ElementLook.Fire,
            "SoapSkater" or "SundaeDuchess" => ElementLook.Ice,
            "TideSinger" or "BellDiver" or "LoneJelly" => ElementLook.Water,
            "StormFencer" or "RainArchivist" => ElementLook.Storm,
            "NightMaw" or "MoonProwler" => ElementLook.Void,
            "LanternKnight" or "LanternWeaver" or "IvorySovereign" or "CitrusMarshal" => ElementLook.Light,
            "KiteAdmiral" or "JugglingJester" => ElementLook.Wind,
            "VerdantMatron" or "ThornTreasurer" => ElementLook.Poison,
            "RootOracle" or "BonsaiClocksmith" => ElementLook.Earth,
            "PetalCitadel" => ElementLook.Bloom,
            "GlassDice" or "MirrorKeeper" or "ChinaMarionette" => ElementLook.Crystal,
            "TreasureAcorn" or "PennyBroker" or "VelvetRegent" => ElementLook.Gold,
            "Postmaster" or "ArchiveCurator" => ElementLook.Paper,
            "ButtonCavalier" or "PatchworkHost" => ElementLook.Thread,
            "TinVirtuoso" => ElementLook.Sound,
            "HourglassStag" => ElementLook.Time,
            "DrowsySeer" => ElementLook.Dream,
            _ => ElementLook.None
        };

        private struct Fx { public ElementLook Look; public Vector2 At, Dir; public float Born, Size; public int Seed; }
        private const int Order = 2070, MaxLive = 18;
        private const float Gap = .07f; // at most one new burst every 70 ms
        private readonly List<Fx> live = new List<Fx>();
        private InkCanvas ink, glow;
        private ArenaController owner;
        private float nextAt;
        private int seed;

        private static readonly InkPalette EarthP = new InkPalette(new Color(.16f, .1f, .05f), new Color(.55f, .4f, .24f), new Color(.88f, .78f, .58f), new Color(.75f, .6f, .4f), new Color(.95f, .85f, .6f));
        private static readonly InkPalette FireP = new InkPalette(new Color(.25f, .08f, .03f), new Color(1f, .55f, .18f), new Color(1f, .93f, .62f), new Color(1f, .62f, .25f), new Color(1f, .85f, .4f));
        private static readonly InkPalette ThreadP = new InkPalette(new Color(.2f, .06f, .06f), new Color(.86f, .3f, .28f), new Color(1f, .85f, .78f), new Color(.95f, .55f, .5f), new Color(.98f, .9f, .7f));
        private static readonly InkPalette DreamP = new InkPalette(new Color(.05f, .07f, .14f), new Color(.3f, .42f, .66f), new Color(.95f, .93f, .82f), new Color(.6f, .75f, .95f), new Color(.98f, .9f, .6f));

        public void Configure(ArenaController controller)
        {
            owner = controller;
            ink = new InkCanvas(transform, "Element look ink", Order);
            glow = new InkCanvas(transform, "Element look light", Order + 2, true);
        }

        /// <summary>A blow of the hero's skill or mechanic lands (its cause "hero-identity-..."): its look bursts on the enemy.</summary>
        public void Observe(CombatEvent e)
        {
            if (owner?.Simulation?.Champion == null || e.Kind != CombatEventKind.Hit || e.Faction != Faction.Enemy) return;
            UnitState hero = owner.Simulation.Champion;
            if (e.Cause.SourceUnitId != hero.Id || e.Cause.ItemId == null || !e.Cause.ItemId.StartsWith("hero-identity")) return; // the skill only
            ElementLook look = For(hero.Definition.Id);
            float now = Time.time;
            if (look == ElementLook.None || now < nextAt || live.Count >= MaxLive) return;
            nextAt = now + (PresentationSettings.ReducedEffects ? Gap * 3 : Gap);
            Vector2 dir = e.Direction.sqrMagnitude > .001f ? e.Direction.normalized : Vector2.right;
            float size = e.Critical ? 1.3f : 1f;
            live.Add(new Fx { Look = look, At = e.Position + Vector2.up * .25f, Dir = dir, Born = now, Size = size, Seed = ++seed });
        }

        private static float Life(ElementLook l) => l == ElementLook.Dream || l == ElementLook.Time || l == ElementLook.Bloom || l == ElementLook.Paper ? .62f : .45f;
        private static Vector2 Dir(float deg) => new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
        private static float Out(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float R(int seed, int i) { uint h = (uint)(seed * 7919 + i * 104729); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xffff) / 65535f; }

        private void LateUpdate()
        {
            if (ink == null) return;
            ink.Clear(); glow.Clear();
            float now = Time.time;
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Fx f = live[i]; float t = (now - f.Born) / Life(f.Look);
                if (t >= 1 || t < 0) { live.RemoveAt(i); continue; }
                Draw(f, t);
            }
            ink.Commit(); glow.Commit();
        }

        private void Line(Vector2 a, Vector2 b, InkPalette p, float width, float alpha, int s, float amp = 0, float freq = 0, bool zig = false)
            => ink.Stroke(InkCanvas.Line(a, b, amp, freq, zig, s), InkStroke.Of(p, width, alpha, s));
        private void Light(Vector2 a, Vector2 b, Color c, float width, float amp = 0, float freq = 0, bool zig = false, int s = 0)
            => glow.GlowRibbon(InkCanvas.Line(a, b, amp, freq, zig, s), 1, .2f, width, c);
        private void Twinkle(Vector2 at, float size, Color c) { Light(at - Vector2.right * size, at + Vector2.right * size, c, size * .35f); Light(at - Vector2.up * size, at + Vector2.up * size, c, size * .35f); }
        private void Bead(Vector2 at, float r, InkPalette p, float alpha, int s) => ink.Stroke(InkCanvas.Arc(at, r, r * .85f, 0, 360, 10), InkStroke.Of(p, r * .9f, alpha, s), true);

        private void Draw(Fx f, float t)
        {
            float a = Mathf.Pow(1 - t, 1.2f), o = Out(t), z = f.Size;
            Vector2 p = f.At, up = Vector2.up;
            int s = f.Seed;
            float face = Mathf.Atan2(f.Dir.y, f.Dir.x) * Mathf.Rad2Deg;
            switch (f.Look)
            {
                case ElementLook.Fire:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 b = p + new Vector2((i - 2) * .08f * z, -.05f), tip = b + up * (.18f + .28f * o) * z * (.7f + .5f * R(s, i)) + Vector2.right * Mathf.Sin(t * 14 + i) * .05f;
                        Line(b, tip, FireP, .08f * z * (1 - .5f * t), a, s + i, .03f, 3);
                        Light(b, tip, new Color(1f, .78f, .38f, .6f * a), .06f * z);
                    }
                    break;
                case ElementLook.Ice:
                case ElementLook.Crystal:
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 d = Dir(i * 60 + R(s, i) * 30 + face);
                        Vector2 a0 = p + d * .08f * z, b0 = p + d * (.16f + .32f * o) * z;
                        Line(a0, b0, ArsenalInk.Glass, .06f * z, a, s + i);
                        Color lit = f.Look == ElementLook.Crystal ? Color.HSVToRGB(Mathf.Repeat(i / 6f + t * .5f, 1), .35f, 1) : Color.white; // a prism's every colour
                        Light(a0, b0, new Color(lit.r, lit.g, lit.b, .75f * a), .04f * z);
                    }
                    break;
                case ElementLook.Water:
                    for (int i = 0; i < 7; i++)
                    {
                        Vector2 d = Dir(30 + i * 20 + R(s, i) * 12), end = p + new Vector2(d.x * (.35f + .2f * R(s, i + 9)), -.15f) * z;
                        var path = InkCanvas.Lob(p, end, (.22f + .1f * R(s, i)) * z, 10);
                        Vector2 head = InkCanvas.Sample(path, o), tail = InkCanvas.Sample(InkCanvas.Lob(p, end, (.22f + .1f * R(s, i)) * z, 10), Mathf.Max(0, o - .18f));
                        Line(tail, head, ArsenalInk.Water, .06f * z, a, s + i);
                        Light(tail, head, new Color(.85f, 1f, 1f, .6f * a), .035f * z);
                    }
                    break;
                case ElementLook.Storm:
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 d = Dir(face + (i - 1) * 50 + R(s, i) * 30), end = p + d * (.35f + .2f * R(s, i + 3)) * z;
                        int flick = s + i * 31 + Mathf.FloorToInt(t * 14); // the spark re-forks as it lives
                        Line(p, end, ArsenalInk.Water, .035f * z, a, flick, .07f, 6, true);
                        Light(p, end, new Color(.85f, .97f, 1f, .9f * a), .05f * z, .07f, 6, true, flick);
                    }
                    Twinkle(p, .1f * z * (1 - t), new Color(1f, 1f, .9f, a));
                    break;
                case ElementLook.Void:
                    for (int i = 0; i < 8; i++)
                    {
                        float ang = i * 45 + R(s, i) * 20 + t * 120, r = .55f * (1 - o) * z;
                        Vector2 from = p + Dir(ang - 25) * (r + .12f), to = p + Dir(ang) * r;
                        Line(from, to, ArsenalInk.Midnight, .045f * z, a, s + i);
                        Light(to - (to - p) * .1f, to, new Color(.55f, .95f, .9f, .8f * a), .04f * z);
                    }
                    for (int k = 0; k < 2; k++) { float from = s * 37 + k * 180 + t * 200; ink.Stroke(InkCanvas.Arc(p, .3f * (1 - t) * z, .22f * (1 - t) * z, from, from + 100, 8), InkStroke.Of(ArsenalInk.Midnight, .05f * z, a, s + k)); } // two turning arcs, never a ring
                    break;
                case ElementLook.Light:
                    for (int i = 0; i < 8; i++) { Vector2 d = Dir(i * 45 + R(s, i) * 15); Light(p + d * .06f * z, p + d * (.14f + .36f * o) * z, new Color(1f, .93f, .66f, .85f * a), .05f * z * (1 - .5f * t)); }
                    for (int i = 0; i < 3; i++) Twinkle(p + Dir(R(s, i + 20) * 360) * .3f * o * z, .07f * z * (1 - t), new Color(1f, 1f, .9f, a));
                    break;
                case ElementLook.Wind:
                    for (int i = 0; i < 3; i++)
                    {
                        float from = s * 40 + i * 120 + t * 170, r = (.18f + .2f * t) * z;
                        ink.Stroke(InkCanvas.Arc(p, r, r * .7f, from, from + 110, 10), InkStroke.Of(ArsenalInk.Cream, .04f * z, .7f * a, s + i));
                        glow.GlowRibbon(InkCanvas.Arc(p, r, r * .7f, from, from + 110, 10), 1, .1f, .04f * z, new Color(.85f, 1f, .95f, .55f * a));
                    }
                    for (int i = 0; i < 2; i++) { Vector2 lf = p + Dir(face + (i == 0 ? 40 : -40) + t * 90) * (.2f + .4f * o) * z; Line(lf, lf + Dir(t * 400 + i * 90) * .09f * z, ArsenalInk.Green, .05f * z, a, s + 9 + i); }
                    break;
                case ElementLook.Poison:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 at = p + new Vector2((R(s, i) - .5f) * .4f * z, (.05f + (.2f + .3f * R(s, i + 5)) * o) * z);
                        Bead(at, (.035f + .03f * R(s, i + 9)) * z * (1 - .4f * t), ArsenalInk.Green, a, s + i);
                    }
                    for (int i = 0; i < 2; i++) { Vector2 d0 = p + new Vector2((i - .5f) * .14f * z, -.04f); Line(d0, d0 - up * (.05f + .18f * o) * z, ArsenalInk.Green, .04f * z, a, s + 20 + i); }
                    break;
                case ElementLook.Earth:
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 d = Dir(20 + i * 28 + R(s, i) * 10), end = p + new Vector2(d.x * (.3f + .2f * R(s, i + 4)), -.12f) * z;
                        Vector2 head = InkCanvas.Sample(InkCanvas.Lob(p, end, .3f * z, 10), o), tail = InkCanvas.Sample(InkCanvas.Lob(p, end, .3f * z, 10), Mathf.Max(0, o - .1f));
                        Line(tail, head, EarthP, .085f * z, a, s + i);
                    }
                    for (int i = 0; i < 3; i++) { Vector2 d = Dir(60 + i * 30); Light(p, p + d * .25f * o * z, new Color(.9f, .8f, .6f, .35f * a), .08f * z); } // a little dust
                    break;
                case ElementLook.Bloom:
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 at = p + Dir(i * 60 + R(s, i) * 25) * (.1f + .38f * o) * z - up * .12f * t * t;
                        Vector2 d = Dir(i * 60 + t * 260 + s * 20) * .07f * z;
                        ink.Stroke(InkCanvas.Lob(at - d, at + d, .04f * z, 6), InkStroke.Of(ArsenalInk.Candy, .07f * z, a, s + i));
                    }
                    break;
                case ElementLook.Gold:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 d = Dir(30 + i * 30 + R(s, i) * 10), end = p + new Vector2(d.x * .4f, -.1f) * z;
                        Vector2 head = InkCanvas.Sample(InkCanvas.Lob(p, end, .32f * z, 10), o);
                        ink.Stroke(InkCanvas.Arc(head, .05f * z, .05f * z * Mathf.Abs(Mathf.Cos(t * 12 + i)) + .01f, 0, 360, 10), InkStroke.Of(ArsenalInk.Gold, .04f * z, a, s + i), true); // a spinning coin
                    }
                    for (int i = 0; i < 3; i++) Twinkle(p + Dir(R(s, i + 30) * 360) * .25f * o * z, .06f * z * (1 - t), new Color(1f, .9f, .5f, a));
                    break;
                case ElementLook.Paper:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 at = p + Dir(40 + i * 25 + R(s, i) * 20) * (.12f + .35f * o) * z - up * .2f * t * t + Vector2.right * Mathf.Sin(t * 9 + i) * .05f;
                        Vector2 d = Dir(t * 300 + i * 70) * .06f * z;
                        Line(at - d, at + d, ArsenalInk.Cream, .07f * z, a, s + i);
                    }
                    break;
                case ElementLook.Thread:
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 d = Dir(face + (i == 0 ? 35 : -35)), mid = p + d * .22f * z, end = p + d * (.25f + .2f * o) * z;
                        Line(p, mid, ThreadP, .035f * z, a, s + i, .04f, 4);
                        Line(mid + d * .06f * o, end, ThreadP, .035f * z, a, s + i + 5, .05f, 5); // snapped
                        Bead(end + Dir(face + 90) * .05f, .045f * z, ArsenalInk.Gold, a, s + i + 9); // a button flies
                    }
                    break;
                case ElementLook.Sound:
                    for (int i = 0; i < 3; i++) { float r = (.12f + .32f * Mathf.Clamp01(o - i * .15f)) * z; ink.Stroke(InkCanvas.Arc(p, r, r, face - 35, face + 35, 8), InkStroke.Of(ArsenalInk.Gold, .04f * z, a * (1 - i * .25f), s + i)); }
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 at = p + new Vector2((i - .5f) * .3f * z, .1f + .35f * o * z);
                        Bead(at, .04f * z, ArsenalInk.Gold, a, s + 10 + i);
                        Line(at + new Vector2(.035f, 0) * z, at + new Vector2(.035f, .14f) * z, ArsenalInk.Gold, .025f * z, a, s + 12 + i); // a note's stem
                    }
                    break;
                case ElementLook.Time:
                {
                    float hand = 90 - t * 300;
                    Line(p, p + Dir(hand) * .26f * z, ArsenalInk.Gold, .04f * z, a, s);
                    Light(p, p + Dir(hand) * .26f * z, new Color(1f, .92f, .6f, .6f * a), .035f * z);
                    for (int i = 0; i < 8; i++) { Vector2 g = p + new Vector2((R(s, i) - .5f) * .35f * z, .15f * z - (.1f + .4f * R(s, i + 8)) * o * z); Line(g, g - up * .03f, ArsenalInk.Gold, .03f * z, a, s + i); } // falling sand
                    break;
                }
                case ElementLook.Dream:
                    for (int i = 0; i < 5; i++) Twinkle(p + new Vector2((R(s, i) - .5f) * .5f * z, (.1f + .45f * o * R(s, i + 5)) * z), .06f * z * (1 - .5f * t), new Color(.98f, .95f, .8f, .85f * a));
                    ink.Stroke(InkCanvas.Arc(p + up * (.2f + .2f * o) * z, .1f * z, .1f * z, 60, 300, 12), InkStroke.Of(DreamP, .04f * z, a, s)); // a small crescent
                    break;
            }
        }
    }
}
