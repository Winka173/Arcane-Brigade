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
        private struct Area { public AreaShape Shape; public bool Hostile, Gloom, HasMotif; public InkToken Motif; public Vector2 A, B; public float R, Born, Warn, Life; public ElementLook Look; public int Seed; }
        private enum FieldKind { Blizzard, Eclipse, Gale, Riptide, Inferno, Pandemic, Realm }
        private struct Field { public FieldKind Kind; public float Born, Life; public Vector2 Dir; }
        private readonly List<Field> fields = new List<Field>();
        private struct Pop { public Vector2 At; public ElementLook Look; public float Born, Size; public int Seed; }
        private readonly List<Pop> pops = new List<Pop>();
        private const int MaxPops = 40, MaxPopsPerFrame = 6;
        private const float PopLife = .5f;
        private int popsThisFrame;
        private struct Link { public int From, To; public float Until; }
        private readonly List<Link> links = new List<Link>();

        private enum AuraKind { Shield, Heal, Buff, Untouchable }
        private struct Aura { public int Unit; public AuraKind Kind; public float Born, Until; public ElementLook Look; }
        private struct Chain { public Vector2 A, B; public float Born; public int Seed; }
        private struct Wall { public Vector2 A, B; public float Born, Until; public ElementLook Look; }

        private const int MaxAreas = 48, MaxBurstsPerFrame = 6, MaxAuras = 40, MaxChains = 24, MaxChainsPerFrame = 8, MaxWalls = 12;
        private const float ChainLife = .32f;
        private readonly List<Aura> auras = new List<Aura>();
        private readonly List<Chain> chains = new List<Chain>();
        private readonly List<Wall> walls = new List<Wall>();
        private float stillUntil, realmUntil;
        private int chainsThisFrame;
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
        public int Auras { get; private set; }
        public int Chains { get; private set; }
        public int Walls { get; private set; }

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

        /// <summary>A skill's pieces come from the part of the creature it uses ("its teapot head" -> steam puffs, "its die
        /// shell" -> dice, "its envelope body" -> letters), so every companion's skill shows its own look.</summary>
        public static bool MotifFor(string bodySource, out InkToken token)
        {
            token = InkToken.Spark;
            if (string.IsNullOrEmpty(bodySource)) return false;
            string k = bodySource.ToLowerInvariant();
            if (Has(k, "teapot", "teacup", "kettle", "steam", "cup", "mist", "cloud", "fog", "pillow")) token = InkToken.Puff;
            else if (Has(k, "candle", "flame", "fire", "ember", "coal", "lamp", "lantern", "wick")) token = InkToken.Ember;
            else if (Has(k, "envelope", "letter", "mail", "post", "parcel", "stamp")) token = InkToken.Envelope;
            else if (Has(k, "dice", " die", "die ")) token = InkToken.Die;
            else if (Has(k, "pepper", "citrus", "lemon", "lime", "orange", "pear", "apple", "fruit", "cherry", "radish", "pumpkin")) token = InkToken.Citrus;
            else if (Has(k, "petal", "flower", "bloom", "dandelion", "rose")) token = InkToken.Petal;
            else if (Has(k, "leaf", "sprout", "seed", "root", "bonsai", "mushroom", "acorn", "tree", "vine")) token = InkToken.Leaf;
            else if (Has(k, "moon", "night", "eclipse", "shadow", "egg")) token = InkToken.Moon;
            else if (Has(k, "star", "comet", "orbit", "planet", "satellite", "wish")) token = InkToken.Star;
            else if (Has(k, "bell", "harp", "horn", "drum", "piano", "choir", "trumpet", "xylophone", "bagpipe", "cymbal", "metronome", "accordion", "music", "note", "song", "flute", "pipe", "whistle")) token = InkToken.Note;
            else if (Has(k, "ice", "snow", "frost", "spoon", "fridge", "glass", "mirror", "lens", "crystal", "shard", "china")) token = InkToken.Shard;
            else if (Has(k, "bubble", "boba", "tapioca", "pearl", "soap")) token = InkToken.Bubble;
            else if (Has(k, "water", "rain", "coral", "bottle", "sea", "shell", "ink", "drop", "tide", "whale", "squid", "lifebuoy")) token = InkToken.Drop;
            else if (Has(k, "button")) token = InkToken.Button;
            else if (Has(k, "thread", "ribbon", "yarn", "stitch", "needle", "thimble", "spindle", "knot", "rope", "string", "yo-yo", "yoyo")) token = InkToken.Knot;
            else if (Has(k, "quilt", "patch", "sock", "bandage", "plaster", "cloth", "rag")) token = InkToken.Patch;
            else if (Has(k, "book", "page", "paper", "folded", "index", "bookmark", "kite", "card", "map")) token = InkToken.Page;
            else if (Has(k, "coin", "gold", "penny", "treasure", "brass", "medal", "crown")) token = InkToken.Coin;
            else if (Has(k, "bolt", "storm", "static", "volt", "lightning", "eel", "spark")) token = InkToken.Bolt;
            else if (Has(k, "spear", "sword", "lance", "beak", "needle", "pin")) token = InkToken.Spear;
            else if (Has(k, "fang", "jaw", "mouth", "teeth", "tooth", "maw")) token = InkToken.Fang;
            else if (Has(k, "thorn", "bristle", "spike", "quill", "cactus", "hedgehog")) token = InkToken.Thorn;
            else if (Has(k, "sand", "stone", "rock", "marble", "fossil", "brick", "anvil", "rook", "tower", "clay")) token = InkToken.Sand;
            else if (Has(k, "dumpling", "bun", "dough", "bread", "pie", "toast", "sundae", "cake", "ice cream", "cookie", "sugar", "sweet")) token = InkToken.Dumpling;
            else if (Has(k, "gear", "clock", "tick", "spring", "key", "wind-up", "hourglass", "compass")) token = InkToken.Knot;
            else if (Has(k, "seal", "wax", "badge")) token = InkToken.Seal;
            else if (Has(k, "feather", "wing", "bird", "owl", "swan", "gull", "wren", "pigeon", "hen", "canary", "crow")) token = InkToken.Petal;
            else return false;
            return true;
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
                if (boundSimulation != null) { boundSimulation.RulesV3EntityChanged -= OnEntity; boundSimulation.RulesV3SupportApplied -= OnSupport; boundSimulation.RulesV3HeroApplied -= OnHero; }
                boundSimulation = sim;
                if (sim != null) { sim.RulesV3EntityChanged += OnEntity; sim.RulesV3SupportApplied += OnSupport; sim.RulesV3HeroApplied += OnHero; }
                entities.Clear(); fallen.Clear(); areas.Clear(); auras.Clear(); chains.Clear(); walls.Clear(); links.Clear(); fields.Clear(); pops.Clear(); stillUntil = realmUntil = 0;
            }
            if (sim == null) return;
            // package runtimes are replaced on every start: follow them
            if (sim.Bonds != boundBonds)
            {
                if (boundBonds != null) { boundBonds.RuleTriggered -= OnBondRule; boundBonds.DamageRequested -= OnBondDamage; }
                boundBonds = sim.Bonds;
                if (boundBonds != null) { boundBonds.RuleTriggered += OnBondRule; boundBonds.DamageRequested += OnBondDamage; }
            }
            if (sim.SkillsV3 != boundSkills)
            {
                if (boundSkills != null) { boundSkills.CastRequested -= OnCast; boundSkills.FriendRequested -= OnFriend; boundSkills.ConstructRequested -= OnConstruct; }
                boundSkills = sim.SkillsV3;
                if (boundSkills != null) { boundSkills.CastRequested += OnCast; boundSkills.FriendRequested += OnFriend; boundSkills.ConstructRequested += OnConstruct; }
            }
            if (sim.EnemyRulesV3 != boundEnemies)
            {
                if (boundEnemies != null) { boundEnemies.DangerRequested -= OnDanger; boundEnemies.Requested -= OnEnemyRule; }
                boundEnemies = sim.EnemyRulesV3;
                if (boundEnemies != null) { boundEnemies.DangerRequested += OnDanger; boundEnemies.Requested += OnEnemyRule; }
            }
            if (sim.Mortality != boundMortality)
            {
                if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; }
                boundMortality = sim.Mortality; fallen.Clear();
                if (boundMortality != null) { boundMortality.Fell += OnFell; boundMortality.Rose += OnRose; }
            }
        }

        private void OnDestroy()
        {
            if (boundSimulation != null) { boundSimulation.RulesV3EntityChanged -= OnEntity; boundSimulation.RulesV3SupportApplied -= OnSupport; boundSimulation.RulesV3HeroApplied -= OnHero; }
            if (boundBonds != null) { boundBonds.RuleTriggered -= OnBondRule; boundBonds.DamageRequested -= OnBondDamage; }
            if (boundSkills != null) { boundSkills.CastRequested -= OnCast; boundSkills.FriendRequested -= OnFriend; boundSkills.ConstructRequested -= OnConstruct; }
            if (boundEnemies != null) { boundEnemies.DangerRequested -= OnDanger; boundEnemies.Requested -= OnEnemyRule; }
            if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; }
            ink?.Destroy();
        }

        private float Now => boundSimulation != null ? boundSimulation.Elapsed : Time.time;

        public void OnBondRule(BondRuleEvent e)
        {
            FieldKind field;
            if (FieldFor(e.Rule, out field, out float life))
            {
                for (int i = fields.Count - 1; i >= 0; i--) if (fields[i].Kind == field) fields.RemoveAt(i);
                float ang = (++seed * 47) % 360 * Mathf.Deg2Rad;
                fields.Add(new Field { Kind = field, Born = Now, Life = life, Dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * .4f).normalized });
            }
            // a bond step fires: a burst in its element on the subject, capped per frame for big crowds
            if (popsThisFrame < MaxPopsPerFrame && Loud(e.Rule, out float size))
            {
                popsThisFrame++;
                if (pops.Count >= MaxPops) pops.RemoveAt(0);
                pops.Add(new Pop { At = e.Position, Look = LookFor(e.Rule), Born = Now, Size = size, Seed = ++seed });
            }
            if (bursts == null || burstsThisFrame >= MaxBurstsPerFrame) return;
            burstsThisFrame++;
            bursts.Burst(LookFor(e.Rule), e.Position, Vector2.right, 1.6f);
        }

        public void OnCast(SkillCastRequest c)
        {
            ElementLook look = LookFor(c.SkillId);
            if (look == ElementLook.Gold) look = LookFor(MainBond(c.SourceId));
            AreaShape shape = c.Shape == SkillShape.Ring ? AreaShape.Ring : c.Shape == SkillShape.Line ? AreaShape.Line : c.Shape == SkillShape.Cone ? AreaShape.Cone : AreaShape.Disc;
            if (c.Shape == SkillShape.Species) { if (bursts != null) bursts.Burst(look, c.Position, Vector2.right, 1.2f); return; }
            bool motif = CompanionSkillTable.TryGet(c.SkillId, out CompanionSkillDefinition skill) & MotifFor(skill?.BodySource, out InkToken token);
            Add(new Area { Shape = shape, Hostile = false, A = c.Position, B = c.End, R = Mathf.Max(.4f, c.Radius), Born = Now, Warn = c.WarningTime, Life = Mathf.Max(.45f, c.WarningTime + Mathf.Max(.35f, c.Duration)), Look = look, Seed = ++seed, HasMotif = motif, Motif = token });
        }

        public void OnDanger(EnemyDangerRequest d)
        {
            AreaShape shape = d.Shape == EnemyDangerShape.Line ? AreaShape.Line : d.Shape == EnemyDangerShape.Cone ? AreaShape.Cone : d.Shape == EnemyDangerShape.Ring ? AreaShape.Ring : d.Shape == EnemyDangerShape.Arena ? AreaShape.Arena : AreaShape.Disc;
            Add(new Area { Shape = shape, Hostile = true, A = d.Position, B = d.End, R = Mathf.Max(.4f, d.Radius), Born = Now, Warn = Mathf.Max(.1f, d.WarningTime), Life = Mathf.Max(.1f, d.WarningTime) + Mathf.Max(.2f, d.Duration), Look = ElementLook.Fire, Seed = ++seed });
        }

        /// <summary>A chained bond hit (Storm, discharge): a forked lightning stroke from the attacker to the enemy it reached,
        /// a few per frame at most.</summary>
        public void OnBondDamage(BondDamageRequest d)
        {
            if (d.Attacker == null || d.Target == null || chainsThisFrame >= MaxChainsPerFrame) return;
            string r = d.Rule ?? string.Empty;
            if (r.IndexOf("chain", StringComparison.OrdinalIgnoreCase) < 0 && r.IndexOf("discharge", StringComparison.OrdinalIgnoreCase) < 0 && r.IndexOf("storm", StringComparison.OrdinalIgnoreCase) < 0) return;
            chainsThisFrame++;
            if (chains.Count >= MaxChains) chains.RemoveAt(0);
            chains.Add(new Chain { A = d.Attacker.Position, B = d.Target.Position, Born = Now, Seed = ++seed });
        }

        /// <summary>A bond's support: shields and heals as auras on the friend, walls as inked segments, time stop and the
        /// Spirit Realm as a field-wide tint for their duration.</summary>
        public void OnSupport(BondSupportRequest s)
        {
            float now = Now, until = now + Mathf.Max(.8f, s.Duration);
            switch (s.Kind)
            {
                case BondSupportKind.Shield: case BondSupportKind.Armor: AddAura(s.Subject, AuraKind.Shield, until, LookFor(s.Rule)); break;
                case BondSupportKind.Heal: case BondSupportKind.Revive: case BondSupportKind.Cleanse: AddAura(s.Subject, AuraKind.Heal, now + .9f, ElementLook.Bloom); break;
                case BondSupportKind.Untouchable: AddAura(s.Subject, AuraKind.Untouchable, until, ElementLook.Light); break;
                case BondSupportKind.DamageBuff: case BondSupportKind.CritBuff: case BondSupportKind.Speed: AddAura(s.Subject, AuraKind.Buff, now + Mathf.Clamp(s.Duration, .8f, 4), LookFor(s.Rule)); break;
                case BondSupportKind.Wall:
                    Vector2 c = s.Subject != null ? s.Subject.Position : s.Position, half = new Vector2(Mathf.Max(1.2f, s.Radius), 0);
                    if (walls.Count >= MaxWalls) walls.RemoveAt(0);
                    walls.Add(new Wall { A = c - half + Vector2.up * .8f, B = c + half + Vector2.up * .8f, Born = now, Until = until, Look = ElementLook.Crystal });
                    break;
                case BondSupportKind.TimeStop: stillUntil = Mathf.Max(stillUntil, now + Mathf.Max(.5f, s.Duration)); break;
                case BondSupportKind.SpiritRealm: realmUntil = Mathf.Max(realmUntil, until); break;
            }
        }

        /// <summary>A companion skill's effect on a friend: protection as a shield aura, healing as rising leaves, buffs as the
        /// skill's element rising off the friend.</summary>
        public void OnFriend(SkillFriendRequest f)
        {
            if (f.Target == null) return;
            float now = Now;
            ElementLook look = LookFor(f.SkillId); if (look == ElementLook.Gold) look = LookFor(MainBond(f.SourceId));
            switch (f.Kind)
            {
                case SkillFriendEffect.ShieldFraction: case SkillFriendEffect.DamageReduction: case SkillFriendEffect.ShareDamage: case SkillFriendEffect.Reflect: case SkillFriendEffect.ReflectHits:
                    AddAura(f.Target, AuraKind.Shield, now + Mathf.Max(.8f, f.Duration), look); break;
                case SkillFriendEffect.HealFraction: case SkillFriendEffect.HealFlat: case SkillFriendEffect.ReviveSooner: case SkillFriendEffect.ReviveNow: case SkillFriendEffect.Cleanse:
                    AddAura(f.Target, AuraKind.Heal, now + .9f, ElementLook.Bloom); break;
                case SkillFriendEffect.None: break;
                default: AddAura(f.Target, AuraKind.Buff, now + Mathf.Clamp(f.Duration, .8f, 4), look); break;
            }
        }

        /// <summary>A skill wall: an inked segment; other constructs (summons, pickups) arrive as rule entities.</summary>
        public void OnConstruct(SkillConstructRequest c)
        {
            if (!c.BlocksBodies && !c.BlocksBullets) return;
            Vector2 a = c.Position, b = (c.End - c.Position).sqrMagnitude > .01f ? c.End : c.Position + Vector2.right * Mathf.Max(1.2f, c.Radius * 2);
            if (walls.Count >= MaxWalls) walls.RemoveAt(0);
            walls.Add(new Wall { A = a, B = b, Born = Now, Until = Now + Mathf.Max(.8f, c.Duration), Look = LookFor(MainBond(c.SourceId)) });
        }

        private void AddAura(UnitState unit, AuraKind kind, float until, ElementLook look)
        {
            if (unit == null) return;
            for (int i = 0; i < auras.Count; i++)
                if (auras[i].Unit == unit.Id && auras[i].Kind == kind) { Aura a = auras[i]; a.Until = Mathf.Max(a.Until, until); auras[i] = a; return; }
            if (auras.Count >= MaxAuras) auras.RemoveAt(0);
            auras.Add(new Aura { Unit = unit.Id, Kind = kind, Born = Now, Until = until, Look = look });
        }

        /// <summary>The hero's rule actions: areas in the hero's own element look, shields and heals as auras on the friend.</summary>
        public void OnHero(HeroRuleRequest h)
        {
            UnitState hero = boundSimulation?.Champion;
            ElementLook look = hero != null ? ElementInk.For(hero.Definition.Id) : ElementLook.Gold;
            if (look == ElementLook.None) look = LookFor(h.Rule);
            switch (h.Action)
            {
                case HeroRuleAction.Heal: AddAura(boundSimulation?.UnitForPresentation(h.FriendId) ?? hero, AuraKind.Heal, Now + .9f, ElementLook.Bloom); return;
                case HeroRuleAction.Shield: AddAura(boundSimulation?.UnitForPresentation(h.FriendId) ?? hero, AuraKind.Shield, Now + Mathf.Max(.8f, h.Duration), look); return;
            }
            if (h.Radius <= 0) return;
            AreaShape shape = h.Shape == HeroRuleShape.Line ? AreaShape.Line : h.Shape == HeroRuleShape.Ring ? AreaShape.Ring : h.Shape == HeroRuleShape.Arena ? AreaShape.Ring : AreaShape.Disc;
            Add(new Area { Shape = shape, A = h.Position, B = h.End, R = Mathf.Min(h.Radius, 6), Born = Now, Life = Mathf.Max(.45f, h.Duration), Look = look, Seed = ++seed });
        }

        /// <summary>Enemy rule effects that are not a warned blow: gloom zones (dark, silence, time, flood) as a solid violet
        /// ground, leech and binder links as a red cord, charm as hearts over the friend.</summary>
        public void OnEnemyRule(EnemyRuleRequest e)
        {
            switch (e.Action)
            {
                case EnemyRuleAction.Darken: case EnemyRuleAction.PauseSkills: case EnemyRuleAction.MissShots: case EnemyRuleAction.SkillFill: case EnemyRuleAction.Slow: case EnemyRuleAction.DrainCoins: case EnemyRuleAction.DrainPickups:
                    if (e.Radius <= 0) return;
                    Add(new Area { Shape = e.Shape == EnemyDangerShape.Ring ? AreaShape.Ring : AreaShape.Disc, Gloom = true, A = e.Position, R = e.Radius, Born = Now, Life = Mathf.Max(.6f, e.Duration), Look = e.Action == EnemyRuleAction.Slow ? ElementLook.Water : ElementLook.Void, Seed = ++seed });
                    return;
                case EnemyRuleAction.LeechLink: case EnemyRuleAction.Chain:
                    if (links.Count >= 24) links.RemoveAt(0);
                    links.Add(new Link { From = e.SourceId, To = e.TargetId, Until = Now + Mathf.Max(.6f, e.Duration) });
                    return;
                case EnemyRuleAction.ChainCut: links.RemoveAll(l => l.From == e.SourceId || l.To == e.SourceId); return;
                case EnemyRuleAction.Charm: AddAura(boundSimulation?.UnitForPresentation(e.TargetId), AuraKind.Buff, Now + Mathf.Max(.8f, e.Duration), ElementLook.Sound); return;
            }
        }

        /// <summary>Bond capstone moments that change the whole field get a field-wide look for their length.</summary>
        /// <summary>Bond moments that deserve a loud impact on the spot (bursts, freezes, shatters, discharges, executes...).</summary>
        private static bool Loud(string rule, out float size)
        {
            size = 1;
            if (string.IsNullOrEmpty(rule)) return false;
            string k = rule.ToLowerInvariant();
            if (Has(k, "inferno", "shatter", "execute", "pandemic", "colossus", "leviathan")) { size = 1.6f; return true; }
            if (Has(k, "burst", "freeze", "frozen", "discharge", "rot", "crack", "pile-up", "collision", "sugar-rush", "mask-double", "chain", "splash", "eclipse", "gale", "riptide", "blizzard", "feast", "beat")) { size = 1.1f; return true; }
            return false;
        }

        private static bool FieldFor(string rule, out FieldKind kind, out float life)
        {
            kind = FieldKind.Blizzard; life = 0;
            if (string.IsNullOrEmpty(rule)) return false;
            string k = rule.ToLowerInvariant();
            if (k.Contains("blizzard")) { kind = FieldKind.Blizzard; life = 1.6f; }
            else if (k.Contains("eclipse")) { kind = FieldKind.Eclipse; life = 2.2f; }
            else if (k.Contains("gale")) { kind = FieldKind.Gale; life = 1.2f; }
            else if (k.Contains("riptide")) { kind = FieldKind.Riptide; life = 1.1f; }
            else if (k.Contains("inferno")) { kind = FieldKind.Inferno; life = 5f; }
            else if (k.Contains("pandemic")) { kind = FieldKind.Pandemic; life = 1.4f; }
            else return false;
            return true;
        }

        /// <summary>A field moment: a tinted veil over the arena plus moving strokes in its element (snow for a blizzard,
        /// gusts for a gale, a wave band for a riptide, heat shimmer for an inferno, spores for a pandemic, dusk for an eclipse).</summary>
        private void DrawField(Field f, Vector2 centre, float now)
        {
            float t = Mathf.Clamp01((now - f.Born) / f.Life), a = Mathf.Sin(t * Mathf.PI);
            if (PresentationSettings.ReducedEffects) a *= .5f;
            switch (f.Kind)
            {
                case FieldKind.Blizzard:
                    ink.Vignette(centre, 28, ArsenalInk.Glass.Light, .25f * a, .7f * a);
                    for (int i = 0; i < 18; i++)
                    {
                        Vector2 p = centre + new Vector2(Mathf.Repeat(R(f.Born.GetHashCode(), i) * 20 + now * 6, 20) - 10, Mathf.Repeat(R(7, i) * 12 - now * 3, 12) - 6);
                        ink.Token(InkToken.Shard, ArsenalInk.Glass, p, .22f, now * 90 + i * 30, a, true);
                    }
                    break;
                case FieldKind.Eclipse:
                    ink.Vignette(centre, 28, ArsenalInk.Violet.Ink, .4f * a, .8f * a);
                    break;
                case FieldKind.Gale:
                    ink.Vignette(centre, 28, ArsenalInk.Cotton.Light, .12f * a, .35f * a);
                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 p = centre + new Vector2(Mathf.Repeat(R(3, i) * 22 + now * 14, 22) - 11, R(5, i) * 12 - 6);
                        ink.Streaks(p, f.Dir.sqrMagnitude > .01f ? f.Dir : Vector2.right, a);
                    }
                    break;
                case FieldKind.Riptide:
                {
                    float x = Mathf.Lerp(-12, 12, t);
                    Vector2 a0 = centre + new Vector2(x, -7), a1 = centre + new Vector2(x, 7);
                    ink.FloorLane(a0, a1, 1.4f, A(ArsenalInk.Water.Body, .45f * a), A(ArsenalInk.Water.Glow, .1f * a));
                    ink.FloorStroke(a0 + Vector2.right * 1.3f, a1 + Vector2.right * 1.3f, ArsenalInk.Water, .1f, 1, 1, a, .15f, 3);
                    break;
                }
                case FieldKind.Inferno:
                    ink.Vignette(centre, 28, ArsenalInk.Amber.Glow, .22f * a, .6f * a);
                    break;
                case FieldKind.Pandemic:
                    ink.Vignette(centre, 28, ArsenalInk.Green.Body, .22f * a, .6f * a);
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 p = centre + new Vector2(R(11, i) * 20 - 10, R(13, i) * 12 - 6 + t * 1.5f);
                        ink.Token(InkToken.Bubble, ArsenalInk.Green, p, .2f, 0, a, true);
                    }
                    break;
            }
        }

        public void OnFell(FriendMortalityEvent e) { fallen[e.FriendId] = e; fallenAt[e.FriendId] = Now; }
        // the mortality clock may differ from the combat clock: the countdown runs from when the fall was seen
        private readonly Dictionary<int, float> fallenAt = new Dictionary<int, float>();
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
            burstsThisFrame = chainsThisFrame = popsThisFrame = 0;
            if (ink == null) return;
            Bind();
            FriendAreas = DangerAreas = Fallen = Entities = Auras = Chains = Walls = 0;
            ink.Begin();
            CombatSimulation sim = boundSimulation;
            bool live = sim != null && (sim.Phase == RunPhase.Combat || sim.Phase == RunPhase.Paused);
            if (!live) { areas.Clear(); fallen.Clear(); auras.Clear(); chains.Clear(); walls.Clear(); links.Clear(); ink.End(); return; }
            float now = sim.Elapsed;
            for (int i = fields.Count - 1; i >= 0; i--) { if (now - fields[i].Born > fields[i].Life) { fields.RemoveAt(i); continue; } DrawField(fields[i], sim.Champion.Position, now); }
            if (now < stillUntil) ink.Vignette(sim.Champion.Position, 30, ArsenalInk.Silver.Body, .05f, .3f);
            if (now < realmUntil) ink.Vignette(sim.Champion.Position, 30, ArsenalInk.Midnight.Glow, .04f, .26f);
            for (int i = walls.Count - 1; i >= 0; i--) { if (now > walls[i].Until) { walls.RemoveAt(i); continue; } DrawWall(walls[i], now); Walls++; }
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                Area a = areas[i]; float age = now - a.Born;
                if (age > a.Life || age < -1) { areas.RemoveAt(i); continue; }
                if (a.Gloom) { DrawGloom(a, age); DangerAreas++; }
                else if (a.Hostile) { DrawDanger(a, age); DangerAreas++; }
                else { DrawFriendly(a, age); FriendAreas++; }
            }
            foreach (KeyValuePair<int, RulesV3EntityRequest> pair in entities) { DrawEntity(pair.Value, now); Entities++; }
            foreach (KeyValuePair<int, FriendMortalityEvent> pair in fallen) { DrawFallen(pair.Value, now); Fallen++; }
            for (int i = auras.Count - 1; i >= 0; i--)
            {
                UnitState unit = sim.UnitForPresentation(auras[i].Unit);
                if (now > auras[i].Until || unit == null || !unit.Alive) { auras.RemoveAt(i); continue; }
                DrawAura(auras[i], unit, now); Auras++;
            }
            if (boundBonds != null) DrawStatuses(sim, now);
            for (int i = pops.Count - 1; i >= 0; i--)
            {
                float age = now - pops[i].Born;
                if (age > PopLife || age < 0) { pops.RemoveAt(i); continue; }
                DrawPop(pops[i], age / PopLife);
            }
            for (int i = links.Count - 1; i >= 0; i--)
            {
                UnitState from = sim.UnitForPresentation(links[i].From), to = sim.UnitForPresentation(links[i].To);
                if (now > links[i].Until || from == null || to == null || !from.Alive || !to.Alive) { links.RemoveAt(i); continue; }
                ink.Cord(from.Position + Vector2.up * .3f, to.Position + Vector2.up * .3f, BossTelegraph.Danger, .06f, true, .9f);
            }
            for (int i = chains.Count - 1; i >= 0; i--)
            {
                float age = now - chains[i].Born;
                if (age > ChainLife || age < 0) { chains.RemoveAt(i); continue; }
                DrawChain(chains[i], age); Chains++;
            }
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
                    // a hot pulsing edge that quickens as the blow nears, so the warning cannot be missed
                    float beat = .5f + .5f * Mathf.Sin(age * (8 + 18 * charge));
                    ink.Ribbon(InkCanvas.Arc(a.A, a.R, a.R * .62f, 0, 360, 36), BossTelegraph.DangerHot, .08f + .1f * charge, 1, 1, alpha * (.5f + .5f * beat), true);
                    break;
            }
        }

        /// <summary>An enemy gloom zone: a solid dark-violet ground (filled, never outline-only) with a brushed rim and its pieces.</summary>
        private void DrawGloom(Area a, float age)
        {
            InkPalette p = PaletteFor(a.Look);
            float now = boundSimulation.Elapsed, t = Mathf.Clamp01(age / a.Life), grow = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .25f)), alpha = 1 - t * t;
            ink.FloorZone(a.A, a.R * grow, a.R * .62f * grow, A(p.Ink, .62f * alpha), A(p.Body, .45f * alpha));
            Rim(a, p, grow, alpha, now);
            Pieces(a, p, grow, alpha * .8f, now);
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
                    ink.FloorZone(a.A, a.R * grow, a.R * .62f * grow, A(p.Glow, .75f * alpha), A(p.Body, .45f * alpha));
                    ink.Pool(a.A, a.R * .8f * grow, a.Life, p, a.Seed, alpha * .8f);
                    break;
            }
            if (age < .4f)
            {
                // birth burst: the area lands with a ring racing to its edge
                float k = age / .4f, rr = a.R * (.3f + .95f * Mathf.Sqrt(k));
                ink.Ribbon(InkCanvas.Arc(a.A, rr, rr * .62f, 0, 360, 32), p, .22f * (1 - k) + .04f, 1, 1, 1 - k, true);
                ink.Wash(a.A, a.R * .8f, p.Glow, .5f * (1 - k), true);
            }
            Rim(a, p, grow, alpha, now);
            Pieces(a, p, grow, alpha, now);
        }

        /// <summary>A brushed rim: dashes that turn slowly around the edge, so the border reads as ink, not a vector circle.</summary>
        private void Rim(Area a, InkPalette p, float grow, float alpha, float now)
        {
            if (a.Shape == AreaShape.Line || a.Shape == AreaShape.Cone) return;
            // a soft glowing band under brushed dashes that turn around the edge
            ink.Ribbon(InkCanvas.Arc(a.A, a.R * grow, a.R * .62f * grow, 0, 360, 40), p, .2f, 1, 1, .45f * alpha, true);
            const int dashes = 12;
            for (int i = 0; i < dashes; i++)
            {
                float from = i * 360f / dashes + now * 24 + a.Seed * 7, to = from + 360f / dashes * .6f;
                ink.Ribbon(InkCanvas.Arc(a.A, a.R * grow, a.R * .62f * grow, from, to, 6), p, .12f, 1, .5f, alpha, true);
            }
        }

        /// <summary>The element's own pieces drifting inside the area (fewer with reduced effects).</summary>
        private void Pieces(Area a, InkPalette p, float grow, float alpha, float now)
        {
            InkToken token = a.HasMotif ? a.Motif : TokenFor(a.Look);
            int n = PresentationSettings.ReducedEffects ? 4 : 11;
            for (int i = 0; i < n; i++)
            {
                float u = (i + .5f) / n, spin = now * 70 + i * 67 + a.Seed;
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
                float bob = Mathf.Abs(Mathf.Sin(now * 4 + i * 1.7f));
                ink.Token(token, p, at + Vector2.up * bob * .14f, .52f + .1f * bob, spin, alpha, false);
                if (i % 3 == 0) ink.Wash(at + Vector2.up * .1f, .28f, p.Glow, .5f * alpha * bob, true);
            }
        }

        private static Vector2 Perp(Vector2 d) { if (d.sqrMagnitude < .0001f) return Vector2.up; d.Normalize(); return new Vector2(-d.y, d.x); }
        private static float R(int seed, int i) { uint h = (uint)(seed * 7919 + i * 104729); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xffff) / 65535f; }

        /// <summary>Shield: a glassy rim turning around the friend; heal: leaves rising; buff: the element's pieces rising off
        /// the friend; untouchable: a gold rim.</summary>
        private void DrawAura(Aura a, UnitState unit, float now)
        {
            Vector2 at = unit.Position + Vector2.up * .35f;
            float left = a.Until - now, fade = Mathf.Clamp01(left / .3f) * Mathf.Clamp01((now - a.Born) / .15f);
            float r = Mathf.Max(.45f, unit.Definition.Radius * 1.6f);
            switch (a.Kind)
            {
                case AuraKind.Shield:
                {
                    InkPalette p = a.Look == ElementLook.Gold ? ArsenalInk.Glass : PaletteFor(a.Look);
                    ink.Wash(at, r * 1.15f, p.Light, .32f * fade, true);
                    ink.Ribbon(InkCanvas.Arc(at, r, r * .9f, now * 60, now * 60 + 280, 26), p, .11f, 1, .25f, fade, true);
                    ink.Ribbon(InkCanvas.Arc(at, r * .82f, r * .74f, -now * 90, -now * 90 + 200, 20), p, .07f, 1, .3f, .8f * fade, true);
                    break;
                }
                case AuraKind.Untouchable:
                    ink.Wash(at, r * 1.3f, ArsenalInk.Gold.Glow, .4f * fade, true);
                    ink.Ribbon(InkCanvas.Arc(at, r * 1.1f, r, 0, 360, 28), ArsenalInk.Gold, .13f, 1, 1, fade, true);
                    break;
                default:
                {
                    InkPalette p = PaletteFor(a.Look);
                    InkToken token = a.Kind == AuraKind.Heal ? InkToken.Leaf : TokenFor(a.Look);
                    ink.Wash(at, r, p.Glow, .3f * fade, true);
                    for (int i = 0; i < 6; i++)
                    {
                        float u = Mathf.Repeat((now - a.Born) * 1.3f + i / 6f, 1);
                        Vector2 p0 = unit.Position + new Vector2((i - 2.5f) * r * .28f + Mathf.Sin(now * 5 + i) * .08f, .1f + u * 1.3f);
                        ink.Token(token, p, p0, .4f, i * 40 + now * 60, fade * (1 - u * u), true);
                    }
                    break;
                }
            }
        }

        private const int MaxStatusEnemies = 60;
        public int StatusMarks { get; private set; }

        /// <summary>Bond stacks on enemies, so the player sees a burst or a freeze coming: a short arc over the head fills per
        /// stack (Burn amber of 3, Chill ice of 5, Poison green of 5, Charge cyan of 5); frozen enemies wear a glassy wash,
        /// marked ones a violet ring at their feet, soaked ones a water wash. At most 60 enemies per frame.</summary>
        private void DrawStatuses(CombatSimulation sim, float now)
        {
            StatusMarks = 0;
            foreach (UnitState enemy in sim.Units)
            {
                if (StatusMarks >= MaxStatusEnemies) break;
                if (enemy.Faction != Faction.Enemy || !enemy.Alive) continue;
                BondEnemyState s = boundBonds.State(enemy);
                if (s == null) continue;
                bool any = false;
                Vector2 head = enemy.Position + Vector2.up * (enemy.Definition.Radius * 2.2f + .35f);
                float w = Mathf.Max(.3f, enemy.Definition.Radius * 1.4f);
                int row = 0;
                if (s.Burn > 0) { Meter(head, w, row++, s.Burn / 3f, ArsenalInk.Amber); any = true; }
                if (s.Chill > 0) { Meter(head, w, row++, s.Chill / 5f, ArsenalInk.Glass); any = true; }
                if (s.Poison > 0) { Meter(head, w, row++, Mathf.Min(1, s.Poison / 5f), ArsenalInk.Green); any = true; }
                if (s.Charge > 0) { Meter(head, w, row++, s.Charge / 5f, ArsenalInk.Cyan); any = true; }
                if (s.FrozenUntil > now) { ink.Wash(enemy.Position + Vector2.up * enemy.Definition.Radius, enemy.Definition.Radius * 1.8f, ArsenalInk.Glass.Light, .35f, true); any = true; }
                if (s.Marked) { ink.Ribbon(InkCanvas.Arc(enemy.Position, w, w * .45f, 0, 360, 16), ArsenalInk.Violet, .05f, 1, 1, .9f, true); any = true; }
                if (s.SoakedUntil > now) { ink.Wash(enemy.Position, w * 1.2f, ArsenalInk.Water.Glow, .25f); any = true; }
                if (any) StatusMarks++;
            }
        }

        private void Meter(Vector2 head, float width, int row, float fill, InkPalette p)
        {
            Vector2 a = head + new Vector2(-width * .5f, row * .13f), b = a + Vector2.right * width;
            ink.Ribbon(InkCanvas.Line(a, b), p, .08f, 1, 1, .45f, false);
            if (fill > 0) ink.Ribbon(InkCanvas.Line(a, Vector2.Lerp(a, b, Mathf.Clamp01(fill))), p, .13f, 1, 1, 1, true);
        }

        /// <summary>A loud impact: a bright shock ring racing out, a flash at the centre and the element's pieces flung out
        /// and falling, all in under half a second.</summary>
        private void DrawPop(Pop p, float t)
        {
            InkPalette pal = PaletteFor(p.Look);
            float o = 1 - (1 - t) * (1 - t) * (1 - t), a = 1 - t, r = (.35f + 1.25f * o) * p.Size;
            Vector2 c = p.At + Vector2.up * .3f;
            ink.Wash(c, .9f * p.Size * (1 - .5f * t), pal.Glow, .55f * a, true);
            ink.Ribbon(InkCanvas.Arc(c, r, r * .7f, 0, 360, 28), pal, .16f * a + .03f, 1, 1, a, true);
            ink.Ribbon(InkCanvas.Arc(c, r * .6f, r * .42f, 0, 360, 20), pal, .08f, 1, 1, .7f * a, true);
            InkToken token = TokenFor(p.Look);
            int n = PresentationSettings.ReducedEffects ? 4 : 9;
            for (int i = 0; i < n; i++)
            {
                float ang = (i * 360f / n + R(p.Seed, i) * 25) * Mathf.Deg2Rad, d = (.3f + 1.3f * o) * p.Size * (.7f + .5f * R(p.Seed, i + 9));
                Vector2 at = c + new Vector2(Mathf.Cos(ang) * d, Mathf.Sin(ang) * d * .7f + o * .4f - t * t * .9f);
                ink.Token(token, pal, at, (.42f - .2f * t) * p.Size, ang * 57 + t * 360, a, true);
            }
        }

        /// <summary>A chained hit: a forked, flickering lightning stroke with a light core.</summary>
        private void DrawChain(Chain c, float age)
        {
            float a = 1 - age / ChainLife;
            int flick = c.Seed + Mathf.FloorToInt(age * 40);
            Vector2 from = c.A + Vector2.up * .3f, to = c.B + Vector2.up * .3f;
            ink.Ribbon(InkCanvas.Line(from, to, .22f, 7, true, flick), ArsenalInk.Cyan, .13f, 1, 1, a, true);
            ink.Ribbon(InkCanvas.Line(from, to, .3f, 5, true, flick + 17), ArsenalInk.Silver, .06f, 1, 1, .8f * a, true);
            ink.Wash(to, .55f, ArsenalInk.Cyan.Glow, .6f * a, true);
            ink.Token(InkToken.Bolt, ArsenalInk.Cyan, to, .5f, flick * 37 % 360, a, true);
        }

        /// <summary>A friendly wall: a thick inked band in its element with a lit top edge, fading out at its end.</summary>
        private void DrawWall(Wall w, float now)
        {
            InkPalette p = PaletteFor(w.Look);
            float fade = Mathf.Clamp01((w.Until - now) / .3f) * Mathf.Clamp01((now - w.Born) / .15f);
            ink.FloorLane(w.A, w.B, .22f, A(p.Body, .75f * fade), A(p.Ink, .5f * fade));
            ink.FloorStroke(w.A + Vector2.up * .18f, w.B + Vector2.up * .18f, p, .08f, 1, 1, fade, 0, 0);
            ink.FloorStroke(w.A - Vector2.up * .2f, w.B - Vector2.up * .2f, p, .06f, 1, 1, .8f * fade, 0, 1);
            // stone joints along the wall and its element's pieces on the crest, so it reads as built, not as a bar
            float length = (w.B - w.A).magnitude; int joints = Mathf.Clamp(Mathf.RoundToInt(length / .38f), 2, 16);
            for (int i = 1; i < joints; i++)
            {
                Vector2 at = Vector2.Lerp(w.A, w.B, i / (float)joints), off = Vector2.up * (i % 2 == 0 ? .1f : -.1f);
                ink.FloorStroke(at + off + Vector2.up * .1f, at + off - Vector2.up * .1f, p, .04f, 1, 1, .7f * fade, 0, i);
                if (i % 3 == 1) ink.Token(TokenFor(w.Look), p, at + Vector2.up * .3f, .28f, i * 25, fade, false);
            }
        }

        /// <summary>A fallen friend: a grey shadow at its feet and a ring that closes as the 15 s pass, gold in the last 2 s.</summary>
        private void DrawFallen(FriendMortalityEvent e, float now)
        {
            UnitState unit = boundSimulation.UnitForPresentation(e.FriendId);
            Vector2 at = unit != null ? unit.Position : e.Position;
            float total = Mathf.Max(.1f, e.RiseAt - e.Time), seen = fallenAt.TryGetValue(e.FriendId, out float at0) ? at0 : now;
            float left = Mathf.Max(0, total - (now - seen)), done = 1 - left / total;
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
                {
                    // a bond summon: its painting when it exists (Resources/BondSummons/<Kind>), on a glowing sigil in its element
                    float size = SummonSize(e.Rule), bob = Mathf.Sin(now * 3 + e.Id) * .08f;
                    ink.Wash(at, size * .9f, p.Glow, .45f, false);
                    ink.Ribbon(InkCanvas.Arc(at, size * .75f, size * .38f, now * 50, now * 50 + 300, 28), p, .1f, 1, .3f, .95f, true);
                    Sprite art = SummonArt(e.Rule);
                    if (art != null) ink.Place(art, at + Vector2.up * (size * .5f + bob), size, 0, Color.white, true);
                    else
                    {
                        ink.Orbit(at + Vector2.up * .3f, size * .6f, now, TokenFor(look), p, 5, 1);
                        ink.Token(TokenFor(look), p, at + Vector2.up * (size * .45f + bob), size * .7f, now * 30, 1);
                    }
                    break;
                }
                default:
                    InkToken token = e.Kind == RulesV3EntityKind.Snack || e.Kind == RulesV3EntityKind.Dish ? InkToken.Dumpling
                        : e.Kind == RulesV3EntityKind.Clover || e.Kind == RulesV3EntityKind.Seed ? InkToken.Leaf
                        : e.Kind == RulesV3EntityKind.Feather ? InkToken.Petal
                        : e.Kind == RulesV3EntityKind.Page ? InkToken.Page
                        : e.Kind == RulesV3EntityKind.Pumpkin ? InkToken.Citrus
                        : e.Kind == RulesV3EntityKind.Skull ? InkToken.Moon
                        : e.Kind == RulesV3EntityKind.Banner ? InkToken.Seal : TokenFor(look);
                {
                    // a pickup: a pulsing glow and a sparkle so it is easy to spot among 100 enemies
                    float pulse = .5f + .5f * Mathf.Sin(now * 5 + e.Id), bob = Mathf.Sin(now * 2.4f + e.Id) * .06f;
                    ink.Wash(at, .7f, p.Glow, .35f + .25f * pulse, false);
                    ink.Ribbon(InkCanvas.Arc(at, .5f + .1f * pulse, .25f + .05f * pulse, 0, 360, 20), p, .07f, 1, 1, .8f, true);
                    Sprite art = SummonArt(e.Kind.ToString());
                    if (art != null) ink.Place(art, at + Vector2.up * (.3f + bob), .62f, 0, Color.white, true);
                    else ink.Token(token, p, at + Vector2.up * (.25f + bob), .6f, 0, 1, true);
                    ink.Token(InkToken.Glint, ArsenalInk.Gold, at + new Vector2(.25f, .55f + bob), .3f * pulse, now * 90, pulse, true);
                    break;
                }
            }
        }

        private static Color A(Color c, float a) { c.a = a; return c; }

        private static readonly Dictionary<string, Sprite> summonArt = new Dictionary<string, Sprite>();
        private static Sprite SummonArt(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return null;
            if (!summonArt.TryGetValue(kind, out Sprite sprite)) summonArt[kind] = sprite = Resources.Load<Sprite>("BondSummons/" + kind);
            return sprite;
        }
        private static float SummonSize(string kind) => kind == "Leviathan" || kind == "GreatTree" || kind == "Colossus" ? 2.4f : kind == "Treant" ? 1.7f : 1.15f;
    }
}
