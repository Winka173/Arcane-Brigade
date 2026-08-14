using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Code ink for the v3/v5 rules: bond steps, companion skills, enemy danger, fallen friends and the abstract
    /// world entities (allies, snacks, pages, lanes...). The simulation owns every gameplay result; this only draws what the
    /// requests describe. Requirements: every effect is code ink in its bond's element look; enemy danger is a SOLID filled
    /// area with a warning that lasts exactly its wind-up; friendly areas are a soft filled wash closed by an inked rim; a fallen
    /// friend shows how long until it stands up; bursts are rate-limited so 100+ enemies stay cheap.</summary>
    public sealed class RulesV3Ink : MonoBehaviour
    {
        private enum AreaShape { Disc, Ring, Line, Cone, Arena }
        private struct Area { public AreaShape Shape; public bool Hostile; public Vector2 A, B; public float R, Born, Warn, Life; public ElementLook Look; public int Seed; }

        private const int MaxAreas = 48, MaxBurstsPerFrame = 6;
        private readonly List<Area> areas = new List<Area>();
        private readonly Dictionary<int, FriendMortalityEvent> fallen = new Dictionary<int, FriendMortalityEvent>();
        private readonly Dictionary<int, RulesV3EntityRequest> entities = new Dictionary<int, RulesV3EntityRequest>();
        private ArenaController owner;
        private ElementInk bursts;
        private ArsenalInk ink;
        private CombatSimulation boundSimulation;
        private BondRuntime boundBonds;
        private CompanionSkillRuntime boundSkills;
        private EnemyRulesV3Runtime boundEnemies;
        private MortalityRuntime boundMortality;
        private int seed, burstsThisFrame;
        // what the last frame showed (read by checks)
        public int FriendAreas { get; private set; }
        public int DangerAreas { get; private set; }
        public int Fallen { get; private set; }
        public int Entities { get; private set; }

        public void Configure(ArenaController controller, ElementInk elementInk)
        {
            owner = controller; bursts = elementInk;
            ink = new ArsenalInk(transform);
        }

        /// <summary>The element look a rule, skill or bond name reads as.</summary>
        public static ElementLook LookFor(string key)
        {
            if (string.IsNullOrEmpty(key)) return ElementLook.Gold;
            string k = key.ToLowerInvariant();
            if (Has(k, "burn", "ember", "inferno", "firepit", "phoenix", "feast", "warmth", "dish", "chef", "hearth", "steam")) return ElementLook.Fire;
            if (Has(k, "chill", "freeze", "frozen", "blizzard", "frost", "shatter", "ice")) return ElementLook.Ice;
            if (Has(k, "tide", "soak", "riptide", "pool", "leviathan", "splash", "rain", "shanty")) return ElementLook.Water;
            if (Has(k, "storm", "chain", "charge", "discharge", "rod", "volt", "thunder")) return ElementLook.Storm;
            if (Has(k, "gale", "wind", "lane", "volley", "tailwind", "paper-wings")) return ElementLook.Wind;
            if (Has(k, "poison", "rot", "pandemic", "plague", "venom", "toxin")) return ElementLook.Poison;
            if (Has(k, "root", "sprout", "treant", "grove", "garden", "seed", "tree", "vine")) return ElementLook.Bloom;
            if (Has(k, "dusk", "mark", "shade", "eclipse", "gravity", "singularity", "mass", "pull")) return ElementLook.Void;
            if (Has(k, "star", "wish", "celestial", "comet", "lantern", "wick", "lit", "lamp", "light")) return ElementLook.Light;
            if (Has(k, "species", "page", "record", "chapter", "archive", "rune", "glyph")) return ElementLook.Paper;
            if (Has(k, "stone", "horn", "quake", "collision", "crack", "colossus", "thorn")) return ElementLook.Earth;
            if (Has(k, "gear", "clock", "echo", "spring", "time", "turret")) return ElementLook.Time;
            if (Has(k, "beat", "chorus", "orchestra", "note", "melody", "crescendo")) return ElementLook.Sound;
            if (Has(k, "spirit", "soul", "revenant", "realm", "dream", "sleep")) return ElementLook.Dream;
            if (Has(k, "shield", "bastion", "guard", "wall", "bulwark")) return ElementLook.Crystal;
            return ElementLook.Gold;
        }

        private static bool Has(string k, params string[] words) { foreach (string w in words) if (k.Contains(w)) return true; return false; }

        private static InkPalette PaletteFor(ElementLook look)
        {
            switch (look)
            {
                case ElementLook.Fire: return ArsenalInk.Amber;
                case ElementLook.Ice: case ElementLook.Crystal: return ArsenalInk.Glass;
                case ElementLook.Water: return ArsenalInk.Water;
                case ElementLook.Storm: return ArsenalInk.Cyan;
                case ElementLook.Wind: return ArsenalInk.Cotton;
                case ElementLook.Poison: case ElementLook.Bloom: return ArsenalInk.Green;
                case ElementLook.Void: return ArsenalInk.Violet;
                case ElementLook.Paper: return ArsenalInk.Dust;
                case ElementLook.Earth: return ArsenalInk.Terracotta;
                case ElementLook.Time: return ArsenalInk.Silver;
                case ElementLook.Sound: return ArsenalInk.Candy;
                case ElementLook.Dream: return ArsenalInk.Midnight;
                default: return ArsenalInk.Gold;
            }
        }

        private static InkToken TokenFor(ElementLook look)
        {
            switch (look)
            {
                case ElementLook.Fire: return InkToken.Ember;
                case ElementLook.Ice: case ElementLook.Crystal: return InkToken.Shard;
                case ElementLook.Water: return InkToken.Drop;
                case ElementLook.Storm: return InkToken.Bolt;
                case ElementLook.Wind: return InkToken.Puff;
                case ElementLook.Poison: return InkToken.Bubble;
                case ElementLook.Bloom: return InkToken.Leaf;
                case ElementLook.Void: case ElementLook.Dream: return InkToken.Moon;
                case ElementLook.Light: return InkToken.Star;
                case ElementLook.Paper: return InkToken.Page;
                case ElementLook.Earth: return InkToken.Sand;
                case ElementLook.Time: return InkToken.Knot;
                case ElementLook.Sound: return InkToken.Note;
                default: return InkToken.Coin;
            }
        }

        private void Bind()
        {
            CombatSimulation sim = owner != null ? owner.Simulation : null;
            if (sim != boundSimulation)
            {
                if (boundSimulation != null) boundSimulation.RulesV3EntityChanged -= OnEntity;
                boundSimulation = sim;
                if (sim != null) sim.RulesV3EntityChanged += OnEntity;
                entities.Clear(); fallen.Clear(); areas.Clear();
            }
            if (sim == null) return;
            // package runtimes are replaced on every start: follow them
            if (sim.Bonds != boundBonds) { if (boundBonds != null) boundBonds.RuleTriggered -= OnBondRule; boundBonds = sim.Bonds; if (boundBonds != null) boundBonds.RuleTriggered += OnBondRule; }
            if (sim.SkillsV3 != boundSkills) { if (boundSkills != null) boundSkills.CastRequested -= OnCast; boundSkills = sim.SkillsV3; if (boundSkills != null) boundSkills.CastRequested += OnCast; }
            if (sim.EnemyRulesV3 != boundEnemies) { if (boundEnemies != null) boundEnemies.DangerRequested -= OnDanger; boundEnemies = sim.EnemyRulesV3; if (boundEnemies != null) boundEnemies.DangerRequested += OnDanger; }
            if (sim.Mortality != boundMortality)
            {
                if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; }
                boundMortality = sim.Mortality; fallen.Clear();
                if (boundMortality != null) { boundMortality.Fell += OnFell; boundMortality.Rose += OnRose; }
            }
        }

        private void OnDestroy()
        {
            if (boundSimulation != null) boundSimulation.RulesV3EntityChanged -= OnEntity;
            if (boundBonds != null) boundBonds.RuleTriggered -= OnBondRule;
            if (boundSkills != null) boundSkills.CastRequested -= OnCast;
            if (boundEnemies != null) boundEnemies.DangerRequested -= OnDanger;
            if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; }
            ink?.Destroy();
        }

        private float Now => boundSimulation != null ? boundSimulation.Elapsed : Time.time;

        public void OnBondRule(BondRuleEvent e)
        {
            // a bond step fires: a burst in its element on the subject, capped per frame for big crowds
            if (bursts == null || burstsThisFrame >= MaxBurstsPerFrame) return;
            burstsThisFrame++;
            bursts.Burst(LookFor(e.Rule), e.Position, Vector2.right, 1f);
        }

        public void OnCast(SkillCastRequest c)
        {
            ElementLook look = LookFor(c.SkillId);
            if (look == ElementLook.Gold) look = LookFor(MainBond(c.SourceId));
            AreaShape shape = c.Shape == SkillShape.Ring ? AreaShape.Ring : c.Shape == SkillShape.Line ? AreaShape.Line : c.Shape == SkillShape.Cone ? AreaShape.Cone : AreaShape.Disc;
            if (c.Shape == SkillShape.Species) { if (bursts != null) bursts.Burst(look, c.Position, Vector2.right, 1.2f); return; }
            Add(new Area { Shape = shape, Hostile = false, A = c.Position, B = c.End, R = Mathf.Max(.4f, c.Radius), Born = Now, Warn = c.WarningTime, Life = Mathf.Max(.45f, c.WarningTime + Mathf.Max(.35f, c.Duration)), Look = look, Seed = ++seed });
        }

        public void OnDanger(EnemyDangerRequest d)
        {
            AreaShape shape = d.Shape == EnemyDangerShape.Line ? AreaShape.Line : d.Shape == EnemyDangerShape.Cone ? AreaShape.Cone : d.Shape == EnemyDangerShape.Ring ? AreaShape.Ring : d.Shape == EnemyDangerShape.Arena ? AreaShape.Arena : AreaShape.Disc;
            Add(new Area { Shape = shape, Hostile = true, A = d.Position, B = d.End, R = Mathf.Max(.4f, d.Radius), Born = Now, Warn = Mathf.Max(.1f, d.WarningTime), Life = Mathf.Max(.1f, d.WarningTime) + Mathf.Max(.2f, d.Duration), Look = ElementLook.Fire, Seed = ++seed });
        }

        public void OnFell(FriendMortalityEvent e) => fallen[e.FriendId] = e;
        public void OnRose(FriendMortalityEvent e) { fallen.Remove(e.FriendId); if (bursts != null) bursts.Burst(ElementLook.Light, e.Position, Vector2.up, 1.1f); }

        public void OnEntity(RulesV3EntityRequest e)
        {
            if (e.Change == RulesV3EntityChange.Created) entities[e.Id] = e;
            else
            {
                entities.Remove(e.Id);
                if (e.Change == RulesV3EntityChange.Collected && bursts != null) bursts.Burst(LookFor(e.Rule), e.Position, Vector2.up, .8f);
            }
        }

        private string MainBond(int unitId)
        {
            UnitState unit = boundSimulation?.UnitForPresentation(unitId);
            return unit != null && unit.Definition.Tags != null && unit.Definition.Tags.Length > 0 ? unit.Definition.Tags[0] : null;
        }

        private void Add(Area a)
        {
            if (areas.Count >= MaxAreas) areas.RemoveAt(0);
            areas.Add(a);
        }

        private void LateUpdate()
        {
            burstsThisFrame = 0;
            if (ink == null) return;
            Bind();
            FriendAreas = DangerAreas = Fallen = Entities = 0;
            ink.Begin();
            CombatSimulation sim = boundSimulation;
            bool live = sim != null && (sim.Phase == RunPhase.Combat || sim.Phase == RunPhase.Paused);
            if (!live) { areas.Clear(); fallen.Clear(); ink.End(); return; }
            float now = sim.Elapsed;
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                Area a = areas[i]; float age = now - a.Born;
                if (age > a.Life || age < -1) { areas.RemoveAt(i); continue; }
                if (a.Hostile) { DrawDanger(a, age); DangerAreas++; }
                else { DrawFriendly(a, age); FriendAreas++; }
            }
            foreach (KeyValuePair<int, RulesV3EntityRequest> pair in entities) { DrawEntity(pair.Value, now); Entities++; }
            foreach (KeyValuePair<int, FriendMortalityEvent> pair in fallen) { DrawFallen(pair.Value, now); Fallen++; }
            ink.End();
        }

        /// <summary>Enemy danger: solid red ground, filled brighter as the blow charges, cracks to the true edge.</summary>
        private void DrawDanger(Area a, float age)
        {
            float charge = Mathf.Clamp01(age / a.Warn), alpha = age <= a.Warn ? 1 : Mathf.Clamp01(1 - (age - a.Warn) / Mathf.Max(.1f, a.Life - a.Warn));
            switch (a.Shape)
            {
                case AreaShape.Line:
                    BossTelegraph.LaneGround(ink, a.A, a.B, a.R, charge, alpha);
                    Vector2 dir = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
                    BossTelegraph.Arrow(ink, a.B, dir, a.R + .3f, charge, alpha);
                    break;
                case AreaShape.Cone:
                    Vector2 aim = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
                    BossTelegraph.FanGround(ink, a.A, aim, .5f, Mathf.Max(a.R, (a.B - a.A).magnitude), charge, alpha);
                    break;
                case AreaShape.Arena:
                    ink.Vignette(a.A, a.R, BossTelegraph.Danger.Body, .15f * alpha * charge, .45f * alpha * charge);
                    break;
                default:
                    BossTelegraph.Rim(ink, a.A, a.R, a.R * .62f, charge, a.Seed, alpha);
                    break;
            }
        }

        /// <summary>A friendly skill area: its element's filled wash, a brushed rim and the element's own pieces moving inside it
        /// (embers, shards, drops, notes...), never a bare shape; held for the skill's duration.</summary>
        private void DrawFriendly(Area a, float age)
        {
            InkPalette p = PaletteFor(a.Look);
            float now = boundSimulation.Elapsed;
            float t = Mathf.Clamp01(age / a.Life), grow = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .2f)), alpha = (1 - t * t) * (PresentationSettings.ReducedEffects ? .6f : 1);
            switch (a.Shape)
            {
                case AreaShape.Line:
                {
                    Vector2 end = Vector2.Lerp(a.A, a.B, grow), side = Perp(a.B - a.A) * a.R;
                    ink.FloorLane(a.A, end, a.R, A(p.Body, .5f * alpha), A(p.Glow, .18f * alpha));
                    ink.FloorStroke(a.A + side, end + side, p, .06f, 1, .3f, alpha, .03f, a.Seed);
                    ink.FloorStroke(a.A - side, end - side, p, .06f, 1, .3f, alpha, .03f, a.Seed + 1);
                    break;
                }
                case AreaShape.Cone:
                {
                    Vector2 aim = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
                    float mid = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, reach = Mathf.Max(a.R, (a.B - a.A).magnitude) * grow;
                    ink.Cone(a.A, reach, mid - 30, mid + 30, p.Body, .5f * alpha, .15f * alpha);
                    for (int i = 0; i < 3; i++)
                    {
                        // ripples running out to the true reach
                        float r = reach * Mathf.Repeat(i / 3f + now * .6f, 1);
                        ink.Ribbon(InkCanvas.Arc(a.A, r, r, mid - 28, mid + 28, 12), p, .05f, 1, .4f, alpha * (1 - r / Mathf.Max(.1f, reach)), true);
                    }
                    break;
                }
                case AreaShape.Ring:
                    ink.FloorZone(a.A, a.R * grow, a.R * .62f * grow, A(p.Body, .1f * alpha), A(p.Body, .55f * alpha));
                    ink.Ribbon(InkCanvas.Arc(a.A, a.R * grow, a.R * .62f * grow, 0, 360, 40), p, .14f, 1, 1, alpha, true);
                    break;
                default:
                    ink.FloorZone(a.A, a.R * grow, a.R * .62f * grow, A(p.Glow, .62f * alpha), A(p.Body, .3f * alpha));
                    ink.Pool(a.A, a.R * .8f * grow, a.Life, p, a.Seed, alpha * .8f);
                    break;
            }
            Rim(a, p, grow, alpha, now);
            Pieces(a, p, grow, alpha, now);
        }

        /// <summary>A brushed rim: dashes that turn slowly around the edge, so the border reads as ink, not a vector circle.</summary>
        private void Rim(Area a, InkPalette p, float grow, float alpha, float now)
        {
            if (a.Shape == AreaShape.Line || a.Shape == AreaShape.Cone) return;
            const int dashes = 10;
            for (int i = 0; i < dashes; i++)
            {
                float from = i * 360f / dashes + now * 18 + a.Seed * 7, to = from + 360f / dashes * .62f;
                ink.Ribbon(InkCanvas.Arc(a.A, a.R * grow, a.R * .62f * grow, from, to, 6), p, .09f, 1, .5f, alpha, true);
            }
        }

        /// <summary>The element's own pieces drifting inside the area (fewer with reduced effects).</summary>
        private void Pieces(Area a, InkPalette p, float grow, float alpha, float now)
        {
            InkToken token = TokenFor(a.Look);
            int n = PresentationSettings.ReducedEffects ? 3 : 6;
            for (int i = 0; i < n; i++)
            {
                float u = (i + .5f) / n, spin = now * 50 + i * 67 + a.Seed;
                Vector2 at;
                if (a.Shape == AreaShape.Line) at = Vector2.Lerp(a.A, a.B, Mathf.Repeat(u + now * .4f, 1) * grow) + Perp(a.B - a.A) * a.R * (R(a.Seed, i) - .5f);
                else if (a.Shape == AreaShape.Cone)
                {
                    Vector2 aim = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
                    float reach = Mathf.Max(a.R, (a.B - a.A).magnitude) * grow, ang = Mathf.Atan2(aim.y, aim.x) + (R(a.Seed, i) - .5f) * .9f;
                    at = a.A + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * reach * Mathf.Repeat(u + now * .5f, 1);
                }
                else
                {
                    float ang = (u * 360 + now * 22 + a.Seed) * Mathf.Deg2Rad, dist = (.25f + .5f * R(a.Seed, i)) * grow;
                    at = a.A + new Vector2(Mathf.Cos(ang) * a.R * dist, Mathf.Sin(ang) * a.R * .62f * dist);
                }
                ink.Token(token, p, at + Vector2.up * Mathf.Sin(now * 3 + i) * .05f, .44f, spin, alpha, false);
            }
        }

        private static Vector2 Perp(Vector2 d) { if (d.sqrMagnitude < .0001f) return Vector2.up; d.Normalize(); return new Vector2(-d.y, d.x); }
        private static float R(int seed, int i) { uint h = (uint)(seed * 7919 + i * 104729); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xffff) / 65535f; }

        /// <summary>A fallen friend: a grey shadow at its feet and a ring that closes as the 15 s pass, gold in the last 2 s.</summary>
        private void DrawFallen(FriendMortalityEvent e, float now)
        {
            UnitState unit = boundSimulation.UnitForPresentation(e.FriendId);
            Vector2 at = unit != null ? unit.Position : e.Position;
            float total = Mathf.Max(.1f, e.RiseAt - e.Time), left = Mathf.Max(0, e.RiseAt - now), done = 1 - left / total;
            ink.FloorZone(at, .55f, .3f, new Color(.1f, .1f, .14f, .4f), new Color(.1f, .1f, .14f, 0));
            InkPalette p = left < 2 ? ArsenalInk.Gold : ArsenalInk.Silver;
            ink.Ribbon(InkCanvas.Arc(at, .62f, .38f, 90, 90 - 360 * done, 32), p, .07f, 1, 1, .9f, left < 2);
        }

        /// <summary>An abstract world entity (bond ally, snack, page, lane...) drawn once in its rule's look.</summary>
        private void DrawEntity(RulesV3EntityRequest e, float now)
        {
            ElementLook look = LookFor(e.Rule);
            InkPalette p = PaletteFor(look);
            Vector2 at = e.Position;
            switch (e.Kind)
            {
                case RulesV3EntityKind.WindLane:
                    ink.FloorLane(e.Position, e.End, Mathf.Max(.3f, e.Radius), A(ArsenalInk.Cotton.Body, .22f), A(ArsenalInk.Cotton.Body, .05f));
                    ink.Stripes(e.Position, e.End, Mathf.Max(.3f, e.Radius) * .6f, now, .7f);
                    break;
                case RulesV3EntityKind.Ally:
                    ink.Wash(at, .9f, p.Light, .22f);
                    ink.Orbit(at, .55f, now, TokenFor(look), p, 3, .95f);
                    ink.Token(TokenFor(look), p, at + Vector2.up * (.35f + Mathf.Sin(now * 3 + e.Id) * .06f), .5f, 0, 1);
                    break;
                default:
                    InkToken token = e.Kind == RulesV3EntityKind.Snack || e.Kind == RulesV3EntityKind.Dish ? InkToken.Dumpling
                        : e.Kind == RulesV3EntityKind.Clover || e.Kind == RulesV3EntityKind.Seed ? InkToken.Leaf
                        : e.Kind == RulesV3EntityKind.Feather ? InkToken.Petal
                        : e.Kind == RulesV3EntityKind.Page ? InkToken.Page
                        : e.Kind == RulesV3EntityKind.Pumpkin ? InkToken.Citrus
                        : e.Kind == RulesV3EntityKind.Skull ? InkToken.Moon
                        : e.Kind == RulesV3EntityKind.Banner ? InkToken.Seal : TokenFor(look);
                    ink.Wash(at, .45f, p.Light, .18f);
                    ink.Token(token, p, at + Vector2.up * (.18f + Mathf.Sin(now * 2.4f + e.Id) * .04f), .42f, 0, 1, false);
                    break;
            }
        }

        private static Color A(Color c, float a) { c.a = a; return c; }
    }
}
