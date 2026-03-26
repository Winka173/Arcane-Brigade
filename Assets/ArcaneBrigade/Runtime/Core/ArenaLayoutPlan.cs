using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a combat arena holds this wave, as plain data. The presentation derives it from the run state and the
    /// simulation receives it on its combat catalog, so both plan the very same <see cref="ArenaLayoutPlan"/>.</summary>
    public readonly struct ArenaScenery : IEquatable<ArenaScenery>
    {
        public readonly Vector2 Size;
        public readonly int Wave, Seed;
        public readonly bool ActOne, Toy, Letter;
        public ArenaScenery(Vector2 size, int wave, int seed, bool actOne, bool toy, bool letter)
        { Size = size; Wave = Mathf.Max(1, wave); Seed = seed; ActOne = actOne; Toy = toy; Letter = letter; }
        /// <summary>Act I zone props stand only in the twenty Act I halls; the keepsakes once rescued or sent.</summary>
        public static ArenaScenery For(RunSnapshot state, Vector2 size)
            => new ArenaScenery(size, state.Wave, state.Seed, !state.Endless && state.Wave <= ActOneCanon.Waves, state.Secrets?.RescuedToy == true, state.Secrets?.LetterSent == true);
        public ArenaScenery Resized(Vector2 size) => new ArenaScenery(size, Wave, Seed, ActOne, Toy, Letter);
        public int Zone => ActOneCanon.Zone(Wave);
        public bool Watchers => Wave >= 11;
        public bool Equals(ArenaScenery other) => Size == other.Size && Wave == other.Wave && Seed == other.Seed && ActOne == other.ActOne && Toy == other.Toy && Letter == other.Letter;
        public override bool Equals(object other) => other is ArenaScenery scenery && Equals(scenery);
        public override int GetHashCode() { unchecked { return ((Size.GetHashCode() * 31 + Wave) * 31 + Seed) * 8 + (ActOne ? 4 : 0) + (Toy ? 2 : 0) + (Letter ? 1 : 0); } }
        public override string ToString() => "arena " + Size + " wave " + Wave + " seed " + Seed + (ActOne ? " actI" : "") + (Toy ? " toy" : "") + (Letter ? " letter" : "");
    }

    public sealed partial class CheckpointCatalog
    {
        /// <summary>Filled by RunSession.PrepareCombat: what stands in this arena (toys and spectacles keep clear of it).</summary>
        [NonSerialized] public ArenaScenery? Scenery;
    }

    /// <summary>One layout pass for every fixed combat prop, the combat twin of ShopLayout. Pure and deterministic: planned
    /// once per arena (wave, zone, keepsakes, arena size, run seed) in priority order — the warm chair, the landmarks, the
    /// Act I zone props and seated travellers, the eight lanterns, the dream architecture, then the distant watchers (rim
    /// only). Each prop gets the free spot nearest to where it would like to stand, tested against real footprints (painted
    /// ink at its drawn size, lean, float clearance), inside the arena; must-show story props come in smaller rather than
    /// not at all, the rest sit the wave out. The simulation keeps its toys and spectacles off the same footprints.</summary>
    public sealed class ArenaLayoutPlan
    {
        public const float Pad = 0.1f;   // breathing room between footprints
        public const float Side = 0.16f; // sideways drift of a floating prop and its suspension motes
        public const float RimBand = 2.6f; // watchers keep within this distance of the arena edge
        public const float Reach = 14f;  // a prop never wanders further than this from its wish
        public sealed class Spot
        {
            public string Id;
            public Vector2 Wish, Foot, Ink; // Ink: painted width and height at full size
            public float Offset, Tilt, Hover, Fit = 1;
            public bool Visible, Shrink, RimOnly;
            public Rect Footprint;
            /// <summary>Size multiplier for art drawn at <paramref name="drawn"/> (its ink at full size): the planned fit, and
            /// never larger than the planned footprint even if the art is wider or taller than the planner assumed.</summary>
            public float Scale(Vector2 drawn) => Fit * Mathf.Min(1, Ink.x * 1.03f / Mathf.Max(.01f, drawn.x), Ink.y * 1.03f / Mathf.Max(.01f, drawn.y));
            public Rect FootprintAt(Vector2 foot, float fit) { Rect rel = Relative(fit); return new Rect(rel.x + foot.x, rel.y + foot.y, rel.width, rel.height); }
            /// <summary>The footprint around a floor contact at the origin.</summary>
            public Rect Relative(float fit)
            {
                float w = Ink.x * fit, h = Ink.y * fit, off = Offset * fit;
                float s = Mathf.Sin(Tilt * Mathf.Deg2Rad), c = Mathf.Cos(Tilt * Mathf.Deg2Rad);
                float left = off - w * .5f, right = off + w * .5f, reach = Mathf.Max(Mathf.Abs(left), Mathf.Abs(right));
                // leaning either way about the floor contact, rising up to Hover above it
                return Rect.MinMaxRect(left * c - h * s - Side, -.12f - reach * s, right * c + h * s + Side, h * c + reach * s + Hover);
            }
        }

        public ArenaScenery Scenery { get; }
        public Rect Bounds { get; }
        private readonly List<Spot> spots = new List<Spot>();
        private readonly Dictionary<string, Spot> byId = new Dictionary<string, Spot>(StringComparer.Ordinal);
        private readonly List<Rect> reserved = new List<Rect>();
        public IReadOnlyList<Spot> Spots => spots;
        /// <summary>Every visible prop's footprint (what toys and spectacles must stay off).</summary>
        public IReadOnlyList<Rect> Reserved => reserved;
        public int Hidden { get; private set; }

        private static readonly Dictionary<ArenaScenery, ArenaLayoutPlan> cache = new Dictionary<ArenaScenery, ArenaLayoutPlan>();
        /// <summary>The plan for this arena (computed once, then shared by the presentation and the simulation).</summary>
        public static ArenaLayoutPlan For(ArenaScenery scenery)
        {
            if (cache.TryGetValue(scenery, out ArenaLayoutPlan plan)) return plan;
            if (cache.Count > 24) cache.Clear();
            return cache[scenery] = new ArenaLayoutPlan(scenery);
        }
        public bool TryGet(string id, out Spot spot) => byId.TryGetValue(id, out spot);
        /// <summary>True when a disc of this radius at point touches any visible prop's footprint.</summary>
        public bool Blocks(Vector2 point, float radius)
        {
            foreach (Rect rect in reserved)
            {
                float dx = Mathf.Max(rect.xMin - point.x, 0, point.x - rect.xMax), dy = Mathf.Max(rect.yMin - point.y, 0, point.y - rect.yMax);
                if (dx * dx + dy * dy < radius * radius) return true;
            }
            return false;
        }
        public bool Blocks(Rect area) { foreach (Rect rect in reserved) if (rect.Overlaps(area)) return true; return false; }
        /// <summary>The point nearest wish (within reach, |x|,|y| inside limit) where area, relative to it, touches no prop;
        /// wish itself when the arena has no such point.</summary>
        public Vector2 ClearSpot(Vector2 wish, Rect area, Vector2 limit, float within)
        {
            foreach (Vector2 offset in Offsets)
            {
                if (offset.sqrMagnitude > within * within) break;
                Vector2 point = wish + offset;
                if (Mathf.Abs(point.x) > limit.x || Mathf.Abs(point.y) > limit.y) continue;
                if (!Blocks(new Rect(area.position + point, area.size))) return point;
            }
            return wish;
        }
        public IEnumerable<string> Describe()
        {
            yield return Scenery.ToString();
            foreach (Spot spot in spots)
                yield return spot.Id + " wish " + spot.Wish.ToString("0.00") + (spot.Visible ? " -> " + spot.Foot.ToString("0.00") + (spot.Fit < 1 ? " at " + spot.Fit.ToString("0.00") + "x" : "") + " " + spot.Footprint.ToString("0.00") : " -> left out");
        }

        private ArenaLayoutPlan(ArenaScenery scenery)
        {
            Scenery = scenery;
            Vector2 half = scenery.Size * .5f;
            Bounds = Rect.MinMaxRect(-half.x + .05f, -half.y + .05f, half.x - .05f, half.y - .05f);
            // the warm chair: the unreachable seat at the far edge of every fight
            Add("chair", new Vector2(0, half.y - .8f), ChairInk, 0, 4.51f, .72f, true);
            foreach (int index in new[] { 0, 1, 2, 3, 4, 6, 7 })
                if (index < 5 || index == 6 && scenery.Toy || index == 7 && scenery.Letter)
                    Add("landmark" + index, LandmarkWish(index, half), LandmarkInk(index), 0, 4.51f, .72f, true);
            if (scenery.ActOne)
            {
                string[] names = ZoneProps[scenery.Zone];
                for (int i = 0; i < names.Length; i++)
                {
                    float height = ZonePropHeight(i);
                    Add("prop" + i, ZonePropWish(i), new Vector2(height * Aspect(names[i]), height), 0, 4.51f, .72f, true);
                }
                for (int i = 0; i < SeatedCount(scenery.Zone); i++)
                    Add("seated" + i, SeatedWish(i), new Vector2(SeatedHeight * Aspect(SeatedName(i)), SeatedHeight), 0, 0, .05f, false);
            }
            for (int i = 0; i < 8; i++)
            {
                float height = LanternHeight(i, scenery.Seed);
                // the lamp's painted body sits right of its floor contact (uv .40) and leans by its own rest tilt plus a breeze
                Add("lantern" + i, LanternWish(i, scenery.Size, scenery.Seed), new Vector2(.5553f, .961f) * height, .102f * height, Mathf.Abs(LanternRestTilt(i, scenery.Seed)) + 2.83f, .72f, true);
            }
            for (int i = 0; i < ArchitectureCount; i++)
                Add("architecture" + i, ArchitectureWish(i, scenery.Size, scenery.Seed), ArchitectureInk(i), 0, 8.2f, .48f, false);
            if (scenery.Watchers)
                for (int i = 0; i < 8; i++)
                {
                    Vector2 ink = WatcherInk(i);
                    Spot watcher = Add("watcher" + i, WatcherCenter(i, half) - Vector2.up * ink.y * .5f, ink, 0, 0, .05f, false);
                    watcher.RimOnly = true;
                }
            Plan();
        }
        private Spot Add(string id, Vector2 wish, Vector2 ink, float offset, float tilt, float hover, bool shrink)
        {
            var spot = new Spot { Id = id, Wish = wish, Foot = wish, Ink = ink, Offset = offset, Tilt = tilt, Hover = hover, Shrink = shrink };
            spots.Add(spot); byId[id] = spot; return spot;
        }
        private void Plan()
        {
            Rect inner = Rect.MinMaxRect(Bounds.xMin + RimBand, Bounds.yMin + RimBand, Bounds.xMax - RimBand, Bounds.yMax - RimBand);
            foreach (Spot spot in spots) // already in priority order
            {
                for (float fit = 1; !spot.Visible && fit > .5f; fit -= .13f)
                {
                    if (fit < 1 && !spot.Shrink) break;
                    Rect rel = spot.Relative(fit);
                    // the floor contacts that keep this footprint inside the arena
                    float minX = Bounds.xMin - rel.xMin, maxX = Bounds.xMax - rel.xMax, minY = Bounds.yMin - rel.yMin, maxY = Bounds.yMax - rel.yMax;
                    if (minX > maxX || minY > maxY) continue;
                    foreach (Vector2 offset in Offsets)
                    {
                        if (offset.sqrMagnitude > Reach * Reach) break;
                        Vector2 foot = spot.Wish + offset;
                        if (foot.x < minX || foot.x > maxX || foot.y < minY || foot.y > maxY) continue;
                        Rect rect = new Rect(rel.x + foot.x, rel.y + foot.y, rel.width, rel.height);
                        if (spot.RimOnly && rect.Overlaps(inner)) continue;
                        if (Blocks(new Rect(rect.x - Pad, rect.y - Pad, rect.width + Pad * 2, rect.height + Pad * 2))) continue;
                        spot.Visible = true; spot.Foot = foot; spot.Fit = fit; spot.Footprint = rect; break;
                    }
                }
                if (spot.Visible) reserved.Add(spot.Footprint); else Hidden++;
            }
        }
        // every floor offset on a quarter-unit grid within reach, nearest first (ties broken by angle: deterministic)
        private static Vector2[] offsets;
        private static Vector2[] Offsets
        {
            get
            {
                if (offsets != null) return offsets;
                var list = new List<Vector2>(); int steps = Mathf.CeilToInt(Reach / .25f);
                for (int x = -steps; x <= steps; x++) for (int y = -steps; y <= steps; y++) if (x * x + y * y <= steps * steps) list.Add(new Vector2(x * .25f, y * .25f));
                list.Sort((a, b) => a.sqrMagnitude != b.sqrMagnitude ? a.sqrMagnitude.CompareTo(b.sqrMagnitude) : Mathf.Atan2(a.y, a.x).CompareTo(Mathf.Atan2(b.y, b.x)));
                return offsets = list.ToArray();
            }
        }

        // ---- what stands in the arena, where it would like to stand, and its painted size (measured from the art) ----
        public static readonly Vector2 ChairInk = new Vector2(1.25f, 1.55f); // warm chair 1.55 tall (the older armchair 1.45)
        /// <summary>Combat wish of a dream landmark (floor contact).</summary>
        public static Vector2 LandmarkWish(int index, Vector2 half) => index switch
        {
            0 => half - Vector2.one * 2, 1 => new Vector2(-half.x + 2, -half.y + 2), 2 => new Vector2(-half.x + 2, half.y - 2),
            3 => new Vector2(half.x - 2, .5f), 4 => new Vector2(0, half.y - 2), 5 => new Vector2(6.5f, -1),
            6 => new Vector2(-6, -3.8f), _ => new Vector2(-half.x + 4, -half.y + 2.3f)
        };
        /// <summary>Drawn width of a landmark in combat (its sprite's full width).</summary>
        public static float LandmarkWidth(int index) => index < 5 ? index == 0 ? 2.4f : 3.2f : .8f;
        /// <summary>Tallest painted height across every picture the landmark can wear (depths, redrawn frames, cabinets).</summary>
        public static Vector2 LandmarkInk(int index)
        {
            float tallest = index switch { 0 => 1.23f, 1 => .93f, 2 => .84f, 3 => .97f, 4 => .82f, 5 => .94f, 6 => .97f, _ => .76f };
            return new Vector2(LandmarkWidth(index), LandmarkWidth(index) * tallest);
        }
        public static readonly string[][] ZoneProps = {
            new[] { "Prop_BoundaryGate", "Prop_Ledger" },
            new[] { "Prop_PlatformBench", "Prop_DepartureBoard", "Prop_PhoneBooth" },
            new[] { "Prop_DiningTable", "Prop_NailedLetterbox", "Prop_InwardWindow", "Prop_FrontDoorMarks" },
            new[] { "Prop_SingleBed", "Prop_Nightstand", "Prop_ReceiptChair", "Prop_Ledger" }
        };
        public static Vector2 ZonePropWish(int index) => new Vector2((index % 2 == 0 ? -1 : 1) * 8.2f, 3.2f - index * 2.2f);
        public static float ZonePropHeight(int index) => index == 0 ? 2.8f : 1.8f;
        public const float SeatedHeight = 1.65f;
        public static int SeatedCount(int zone) => zone == 0 ? 0 : zone + 2;
        public static string SeatedName(int index) => "SeatedOne_" + (char)('A' + index % 5);
        public static Vector2 SeatedWish(int index) => new Vector2((index % 2 == 0 ? -1 : 1) * 10f, 3.8f - (index / 2) * 2.4f);
        /// <summary>Painted width over height of the Act I standing art (zone props, seated travellers).</summary>
        public static float Aspect(string name) => name switch
        {
            "Prop_BoundaryGate" => .90f, "Prop_Ledger" => .92f, "Prop_PlatformBench" => 1.79f, "Prop_DepartureBoard" => .99f, "Prop_PhoneBooth" => .65f,
            "Prop_DiningTable" => 1.40f, "Prop_NailedLetterbox" => .71f, "Prop_InwardWindow" => .71f, "Prop_FrontDoorMarks" => .56f,
            "Prop_SingleBed" => 1.0f, "Prop_Nightstand" => .81f, "Prop_ReceiptChair" => .71f,
            "SeatedOne_A" => .71f, "SeatedOne_B" => 1.15f, "SeatedOne_C" => .82f, "SeatedOne_D" => .64f, "SeatedOne_E" => .89f, _ => 1.0f
        };
        /// <summary>Presentation-only noise for the lanterns (never a run/shop/combat random stream).</summary>
        public static float LanternNoise(int index, int seed, uint channel)
        {
            unchecked
            {
                uint n = (uint)seed ^ (uint)(index + 1) * 0x9e3779b9u ^ channel * 0x85ebca6bu;
                n ^= n >> 16; n *= 0x7feb352du; n ^= n >> 15; n *= 0x846ca68bu; n ^= n >> 16;
                return (n & 0x00ffffffu) / 16777215f;
            }
        }
        public static float LanternHeight(int index, int seed) => Mathf.Lerp(1.6f, 1.9f, LanternNoise(index, seed, 3));
        public static float LanternRestTilt(int index, int seed) => (((index ^ seed) & 1) == 0 ? -1 : 1) * Mathf.Lerp(4.5f, 10.5f, LanternNoise(index, seed, 4));
        /// <summary>Combat wish of a lantern: its seeded scatter around eight anchors.</summary>
        public static Vector2 LanternWish(int index, Vector2 size, int seed)
        {
            var h = size * .5f;
            Vector2 jitter = new Vector2(LanternNoise(index, seed, 1) * 2 - 1, LanternNoise(index, seed, 2) * 2 - 1);
            Vector2 anchor = index switch
            {
                0 => new Vector2(-Mathf.Min(6, h.x - 2), -3.3f), 1 => new Vector2(Mathf.Min(6, h.x - 2), -3.3f),
                2 => new Vector2(-Mathf.Min(6, h.x - 2), 2.2f), 3 => new Vector2(Mathf.Min(6, h.x - 2), 2.2f),
                4 => new Vector2(-h.x + 3, h.y - 3), 5 => new Vector2(h.x - 3, h.y - 3),
                6 => new Vector2(-h.x * .4f, -h.y + 2.2f), _ => new Vector2(h.x * .4f, -h.y + 2.2f)
            };
            Vector2 spread = index < 4 ? new Vector2(1.35f, .85f) : new Vector2(1.5f, .7f);
            Vector2 position = anchor + Vector2.Scale(jitter, spread);
            return new Vector2(Mathf.Clamp(position.x, -h.x + 1.25f, h.x - 1.25f), Mathf.Clamp(position.y, -h.y + 1.3f, h.y - 1.3f));
        }
        public const int ArchitectureCount = 12;
        private static readonly Vector2[] Slots = { new Vector2(-.77f, .67f), new Vector2(-.24f, .74f), new Vector2(.3f, .67f), new Vector2(.77f, .64f), new Vector2(-.72f, 0), new Vector2(.72f, -.04f), new Vector2(-.77f, -.64f), new Vector2(-.24f, -.73f), new Vector2(.24f, -.68f), new Vector2(.77f, -.64f), new Vector2(-.53f, .35f), new Vector2(.53f, -.35f) };
        /// <summary>Combat wish of a dream architecture piece: a fixed slot of the map plus seeded jitter.</summary>
        public static Vector2 ArchitectureWish(int index, Vector2 size, int seed)
        {
            unchecked
            {
                uint n = (uint)seed ^ (uint)(index + 1) * 2654435761u; n ^= n >> 16; n *= 2246822519u; n ^= n >> 13;
                return Vector2.Scale(Slots[index % ArchitectureCount], size * .5f) + new Vector2((n & 65535) / 65535f - .5f, ((n >> 16) & 65535) / 65535f - .5f) * .9f;
            }
        }
        public static float ArchitectureWidth(int index) => index % 4 == 0 ? 3.1f : index % 4 == 1 ? 1.7f : index % 4 == 2 ? 2.7f : 2.8f;
        /// <summary>Painted size of an architecture piece at its tallest damage frame.</summary>
        public static Vector2 ArchitectureInk(int index) => new Vector2(ArchitectureWidth(index), ArchitectureWidth(index) * (index % 4 == 0 ? 1.06f : index % 4 == 1 ? 1.1f : index % 4 == 2 ? 1.13f : .5f));
        /// <summary>Combat wish of a distant watcher (the centre of its eyes).</summary>
        public static Vector2 WatcherCenter(int index, Vector2 half)
            => index < 4 ? new Vector2((index % 2 == 0 ? -1 : 1) * (half.x - .7f), (index / 2 * 2 - 1) * half.y * .55f) : new Vector2((index - 5.5f) * half.x * .46f, half.y - .7f);
        public static float WatcherWidth(int index) => index % 3 == 0 ? 1.2f : .95f;
        public static Vector2 WatcherInk(int index) => new Vector2(WatcherWidth(index), WatcherWidth(index) * (index % 3 == 0 ? .79f : index % 3 == 1 ? .49f : .85f));
    }
}
