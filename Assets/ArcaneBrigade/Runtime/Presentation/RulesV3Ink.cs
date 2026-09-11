using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private struct Area { public AreaShape Shape; public bool Hostile, Gloom, HasMotif, Landed; public InkToken Motif; public Vector2 A, B; public float R, Born, Warn, Life; public ElementLook Look; public int Seed; }
        private enum FieldKind { Blizzard, Eclipse, Gale, Riptide, Inferno, Pandemic, Realm, Lost }
        private struct Field { public FieldKind Kind; public float Born, Life; public Vector2 Dir; }
        private readonly List<Field> fields = new List<Field>();
        private struct Pop { public Vector2 At; public ElementLook Look; public float Born, Size; public int Seed; public bool Hostile; }
        private readonly List<Pop> pops = new List<Pop>();
        private struct Throw { public Vector2 From, To; public float Born, Life; public ElementLook Look; public InkToken Token; public int Seed; public bool Straight; }
        private readonly List<Throw> throws = new List<Throw>();
        private const int MaxThrows = 16;
        private const int MaxPops = 40, MaxPopsPerFrame = 6;
        private const float PopLife = .5f;
        private int popsThisFrame;
        private struct Link { public int From, To; public float Until; }
        private readonly List<Link> links = new List<Link>();

        private enum AuraKind { Shield, Heal, Buff, Untouchable, Stun, Sleep, Root }
        private struct Aura { public int Unit; public AuraKind Kind; public float Born, Until; public ElementLook Look; }
        private struct Chain { public Vector2 A, B; public float Born; public int Seed; public bool Violet; }
        private struct Wall { public Vector2 A, B; public float Born, Until; public ElementLook Look; }

        private const int MaxAreas = 48, MaxBurstsPerFrame = 6, MaxAuras = 90, MaxChains = 24, MaxChainsPerFrame = 8, MaxWalls = 12;
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
        private InkCanvas light;
        private RulesVfx vfx;
        public RulesVfx Vfx => vfx;
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
            vfx = gameObject.AddComponent<RulesVfx>();
            cartoon = gameObject.AddComponent<CartoonFx>();
            CombatFeedback.ElementTint = ElementColour;
            ink = new ArsenalInk(transform);
            light = new InkCanvas(transform, "Rules light", 2141, true);
        }

        private CartoonFx cartoon;
        public CartoonFx Cartoon => cartoon;
        private readonly Dictionary<int, float> nextHitAt = new Dictionary<int, float>();
        private float hitBudgetAt; private int hitBudget; private float nextWordAt;

        /// <summary>The element a hit's dealer fights with: a friend's main bond, the hero's own look, else none.</summary>
        private ElementLook DealerLook(CombatEvent e)
        {
            UnitState source = boundSimulation?.UnitForPresentation(e.Cause.SourceUnitId);
            if (source == null) return ElementLook.None;
            return source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : source.Faction == Faction.Champion ? ElementInk.For(source.Definition.Id) : ElementLook.None;
        }

        /// <summary>A cartoon hit (Cartoon FX) where our side strikes an enemy, in the dealer's element: a fire, frost,
        /// lightning, light or smoke puff, a red star on a critical. Each enemy shows one hit at a time and a crowd at most
        /// about twenty a second, so hundreds of enemies never bury the field.</summary>
        public void OnCombat(CombatEvent e)
        {
            if (cartoon == null || e.Kind != CombatEventKind.Hit || e.Faction != Faction.Enemy || e.Value <= 0) return;
            float now = Time.time;
            if (now >= hitBudgetAt) { hitBudgetAt = now + .1f; hitBudget = PresentationSettings.ReducedEffects ? 1 : 2; }
            if (!e.Critical && (hitBudget <= 0 || nextHitAt.TryGetValue(e.SubjectId, out float until) && now < until)) return;
            if (nextHitAt.Count > 256) nextHitAt.Clear();
            nextHitAt[e.SubjectId] = now + .25f; hitBudget--;
            ElementLook look = DealerLook(e);
            string key = look switch
            {
                ElementLook.Fire => "hit_fire", ElementLook.Ice or ElementLook.Crystal => "hit_ice", ElementLook.Storm => "hit_storm",
                ElementLook.Light or ElementLook.Gold or ElementLook.Time or ElementLook.Sound => "hit_light",
                ElementLook.Earth or ElementLook.Paper or ElementLook.Poison or ElementLook.Void or ElementLook.Dream => "hit_smoke", _ => "hit_misc",
            };
            Vector2 at = e.Position + Vector2.up * .3f;
            cartoon.Play(key, at, e.Critical ? .55f : .4f);
            if (e.Critical) cartoon.Play("hit_crit", at, .45f, Random.value * 360);
        }

        /// <summary>The colour of a hit's element: its source companion's main bond, the hero's own look, else none.</summary>
        private Color? ElementColour(CombatEvent e)
        {
            ElementLook look = DealerLook(e);
            if (look == ElementLook.None) return null;
            Color c = Saturate(PaletteFor(look).Glow);
            return Color.Lerp(c, Color.white, .15f);
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
                entities.Clear(); fallen.Clear(); areas.Clear(); auras.Clear(); chains.Clear(); walls.Clear(); links.Clear(); fields.Clear(); pops.Clear(); throws.Clear(); stillUntil = realmUntil = 0;
            }
            if (sim == null) return;
            // package runtimes are replaced on every start: follow them
            if (sim.Bonds != boundBonds)
            {
                if (boundBonds != null) { boundBonds.RuleTriggered -= OnBondRule; boundBonds.DamageRequested -= OnBondDamage; boundBonds.StrengthStolen -= OnStolen; boundBonds.EmbersCreated -= OnEmbers; }
                boundBonds = sim.Bonds;
                if (boundBonds != null) { boundBonds.RuleTriggered += OnBondRule; boundBonds.DamageRequested += OnBondDamage; boundBonds.StrengthStolen += OnStolen; boundBonds.EmbersCreated += OnEmbers; }
            }
            if (sim.SkillsV3 != boundSkills)
            {
                if (boundSkills != null) { boundSkills.CastRequested -= OnCast; boundSkills.FriendRequested -= OnFriend; boundSkills.ConstructRequested -= OnConstruct; boundSkills.EnemyEffectRequested -= OnEnemyEffect; }
                boundSkills = sim.SkillsV3;
                if (boundSkills != null) { boundSkills.CastRequested += OnCast; boundSkills.FriendRequested += OnFriend; boundSkills.ConstructRequested += OnConstruct; boundSkills.EnemyEffectRequested += OnEnemyEffect; }
            }
            if (sim.EnemyRulesV3 != boundEnemies)
            {
                if (boundEnemies != null) { boundEnemies.DangerRequested -= OnDanger; boundEnemies.Requested -= OnEnemyRule; }
                boundEnemies = sim.EnemyRulesV3;
                if (boundEnemies != null) { boundEnemies.DangerRequested += OnDanger; boundEnemies.Requested += OnEnemyRule; }
            }
            if (sim.Mortality != boundMortality)
            {
                if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; boundMortality.WaveLost -= OnWaveLost; boundMortality.ShieldBroken -= OnShieldBroken; }
                boundMortality = sim.Mortality; fallen.Clear();
                if (boundMortality != null) { boundMortality.Fell += OnFell; boundMortality.Rose += OnRose; boundMortality.WaveLost += OnWaveLost; boundMortality.ShieldBroken += OnShieldBroken; }
            }
        }

        private void OnDestroy()
        {
            if (CombatFeedback.ElementTint == (System.Func<CombatEvent, Color?>)ElementColour) CombatFeedback.ElementTint = null;
            if (boundSimulation != null) { boundSimulation.RulesV3EntityChanged -= OnEntity; boundSimulation.RulesV3SupportApplied -= OnSupport; boundSimulation.RulesV3HeroApplied -= OnHero; }
            if (boundBonds != null) { boundBonds.RuleTriggered -= OnBondRule; boundBonds.DamageRequested -= OnBondDamage; boundBonds.StrengthStolen -= OnStolen; boundBonds.EmbersCreated -= OnEmbers; }
            if (boundSkills != null) { boundSkills.CastRequested -= OnCast; boundSkills.FriendRequested -= OnFriend; boundSkills.ConstructRequested -= OnConstruct; boundSkills.EnemyEffectRequested -= OnEnemyEffect; }
            if (boundEnemies != null) { boundEnemies.DangerRequested -= OnDanger; boundEnemies.Requested -= OnEnemyRule; }
            if (boundMortality != null) { boundMortality.Fell -= OnFell; boundMortality.Rose -= OnRose; boundMortality.WaveLost -= OnWaveLost; boundMortality.ShieldBroken -= OnShieldBroken; }
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
                owner?.Feel.Accent(.35f, .15f);
                Sound("rules.field", e.Position);
            }
            // a bond step fires: a burst in its element on the subject, capped per frame for big crowds
            if (popsThisFrame < MaxPopsPerFrame && Loud(e.Rule, out float size))
            {
                popsThisFrame++;
                if (pops.Count >= MaxPops) pops.RemoveAt(0);
                float delay = 0;
                if (e.Rule != null && e.Rule.IndexOf("star", StringComparison.OrdinalIgnoreCase) >= 0 && throws.Count < MaxThrows)
                {
                    // a falling star streaks down from the sky and bursts where it lands
                    delay = .28f;
                    throws.Add(new Throw { From = e.Position + new Vector2(2.4f, 7.5f), To = e.Position, Born = Now, Life = delay, Look = ElementLook.Light, Token = InkToken.Star, Seed = ++seed, Straight = true });
                }
                AddPop(e.Position, LookFor(e.Rule), size, delay);
                Sound(LookFor(e.Rule), "burst", e.Position);
                if (size >= 1.5f) owner?.Feel.Accent(.3f, .1f); else if (size >= 1.1f) owner?.Feel.Accent(.12f, 0);
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
            // the cast leaves the companion: a flash on it, its piece lobbed to the spot, and the area lands when it arrives
            UnitState caster = boundSimulation?.UnitForPresentation(c.SourceId);
            float flight = 0;
            if (caster != null && caster.Alive)
            {
                EnemyPop(caster.Position, look, .7f);
                Sound(look, "cast", caster.Position);
                float distance = (c.Position - caster.Position).magnitude;
                if (distance > 1.2f)
                {
                    flight = Mathf.Clamp(distance * .05f, .16f, .38f);
                    if (throws.Count >= MaxThrows) throws.RemoveAt(0);
                    throws.Add(new Throw { From = caster.Position + Vector2.up * .5f, To = c.Position, Born = Now, Life = flight, Look = look, Token = motif ? token : TokenFor(look), Seed = ++seed });
                }
            }
            Add(new Area { Shape = shape, Hostile = false, A = c.Position, B = c.End, R = Mathf.Max(.4f, c.Radius), Born = Now + flight, Warn = c.WarningTime, Life = Mathf.Max(.45f, c.WarningTime + Mathf.Max(.35f, c.Duration)), Look = look, Seed = ++seed, HasMotif = motif, Motif = token });
        }

        public void OnDanger(EnemyDangerRequest d)
        {
            AreaShape shape = d.Shape == EnemyDangerShape.Line ? AreaShape.Line : d.Shape == EnemyDangerShape.Cone ? AreaShape.Cone : d.Shape == EnemyDangerShape.Ring ? AreaShape.Ring : d.Shape == EnemyDangerShape.Arena ? AreaShape.Arena : AreaShape.Disc;
            Sound("rules.warn", d.Position);
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
            Sound("rules.chain", d.Target.Position);
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
        /// <summary>A skill's status on an enemy, shown on that enemy for its length: stars circling a stunned head, moons
        /// drifting off a sleeper, leaves gripping a rooted one's feet, the element rising off burning, chilled or poisoned ones.</summary>
        public void OnEnemyEffect(SkillEnemyEffectRequest e)
        {
            if (e.Target == null || !e.Target.Alive) return;
            float until = Now + Mathf.Clamp(e.Duration, .6f, 6);
            SkillEnemyEffect f = e.Effects;
            if ((f & (SkillEnemyEffect.Stun | SkillEnemyEffect.Freeze)) != 0) AddAura(e.Target, AuraKind.Stun, until, (f & SkillEnemyEffect.Freeze) != 0 ? ElementLook.Ice : ElementLook.Light);
            else if ((f & SkillEnemyEffect.Sleep) != 0) AddAura(e.Target, AuraKind.Sleep, until, ElementLook.Dream);
            else if ((f & SkillEnemyEffect.Root) != 0) AddAura(e.Target, AuraKind.Root, until, ElementLook.Bloom);
            else if ((f & SkillEnemyEffect.Burn) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Fire);
            else if ((f & SkillEnemyEffect.Poison) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Poison);
            else if ((f & (SkillEnemyEffect.Chill | SkillEnemyEffect.Slow | SkillEnemyEffect.AttackSlow)) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Ice);
            else if ((f & (SkillEnemyEffect.Mark | SkillEnemyEffect.DuskMark | SkillEnemyEffect.Weaken)) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Void);
            else if ((f & SkillEnemyEffect.Charge) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Storm);
            else if ((f & (SkillEnemyEffect.Lit | SkillEnemyEffect.Blind)) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Light);
            else if ((f & SkillEnemyEffect.Soak) != 0) AddAura(e.Target, AuraKind.Buff, until, ElementLook.Water);
        }

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
            if (unit.Faction != Faction.Enemy && (kind == AuraKind.Heal || kind == AuraKind.Shield)) Sound(kind == AuraKind.Heal ? "rules.heal" : "rules.shield", unit.Position);
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
                // the enemy's own moments, loud enough to answer in a crowd
                case EnemyRuleAction.HealEnemy: AddAura(boundSimulation?.UnitForPresentation(e.TargetId != 0 ? e.TargetId : e.SourceId), AuraKind.Heal, Now + .9f, ElementLook.Bloom); return;
                case EnemyRuleAction.ShieldEnemy: AddAura(boundSimulation?.UnitForPresentation(e.TargetId != 0 ? e.TargetId : e.SourceId), AuraKind.Shield, Now + Mathf.Max(1, e.Duration), ElementLook.Void); return;
                case EnemyRuleAction.MarkHero: AddAura(boundSimulation?.Champion, AuraKind.Buff, Now + Mathf.Max(1, e.Duration), ElementLook.Fire); return;
                case EnemyRuleAction.ReviveEnemy: EnemyPop(e.Position, ElementLook.Bloom, 1.4f); return;
                case EnemyRuleAction.Spawn: EnemyPop(e.Position, ElementLook.Void, 1.2f); return;
                case EnemyRuleAction.Swallow: EnemyPop(e.Position, ElementLook.Void, 1.6f); return;
                case EnemyRuleAction.Reflect: EnemyPop(e.Position, ElementLook.Crystal, 1f); return;
                case EnemyRuleAction.ArmorBreak: EnemyPop(boundSimulation?.UnitForPresentation(e.TargetId)?.Position ?? e.Position, ElementLook.Earth, 1f); return;
                case EnemyRuleAction.Phase:
                    EnemyPop(e.Position, ElementLook.Fire, 2.6f);
                    owner?.Feel.Accent(.55f, .35f);
                    Sound("rules.phase", e.Position);
                    fields.Add(new Field { Kind = FieldKind.Realm, Born = Now, Life = 1.2f });
                    return;
            }
        }

        /// <summary>Bond capstone moments that change the whole field get a field-wide look for their length.</summary>
        private void EnemyPop(Vector2 at, ElementLook look, float size)
        {
            if (popsThisFrame >= MaxPopsPerFrame) return;
            popsThisFrame++;
            AddPop(at, look, size, 0);
        }

        /// <summary>Bond moments that deserve a loud impact on the spot (bursts, freezes, shatters, discharges, executes...).</summary>
        private static bool Loud(string rule, out float size)
        {
            size = 1;
            if (string.IsNullOrEmpty(rule)) return false;
            string k = rule.ToLowerInvariant();
            if (Has(k, "inferno", "shatter", "execute", "pandemic", "colossus", "leviathan", "falling-star", "star")) { size = 1.6f; return true; }
            if (Has(k, "burst", "freeze", "frozen", "discharge", "rot", "crack", "pile-up", "collision", "sugar-rush", "mask-double", "chain", "splash", "eclipse", "gale", "riptide", "blizzard", "feast", "beat", "rune", "carnival", "horn", "thorn", "root-hit")) { size = 1.1f; return true; }
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
                    for (int i = 0; i < 26; i++)
                    {
                        float u = Mathf.Repeat(now * .35f + R(9, i), 1);
                        Glint(centre + new Vector2((R(5, i) - .5f) * 20 - u * 2, 6 - u * 12), .07f, ArsenalInk.Glass, a * Mathf.Sin(u * Mathf.PI));
                    }
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
                    for (int i = 0; i < 16; i++) ElementStroke(ElementLook.Wind, centre + new Vector2((R(30, i) - .5f) * 18, (R(31, i) - .5f) * 10), centre + new Vector2((R(30, i + 1) - .5f) * 18, (R(31, i + 1) - .5f) * 10), i, 30, now, 1.2f, a);
                    ink.Vignette(centre, 28, ArsenalInk.Cotton.Light, .12f * a, .35f * a);
                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 p = centre + new Vector2(Mathf.Repeat(R(3, i) * 22 + now * 14, 22) - 11, R(5, i) * 12 - 6);
                        ink.Streaks(p, f.Dir.sqrMagnitude > .01f ? f.Dir : Vector2.right, a);
                    }
                    break;
                case FieldKind.Riptide:
                    for (int i = 0; i < 16; i++) ElementStroke(ElementLook.Water, centre + new Vector2((R(30, i) - .5f) * 18, (R(31, i) - .5f) * 10), centre + new Vector2((R(30, i + 1) - .5f) * 18, (R(31, i + 1) - .5f) * 10), i, 30, now, 1.2f, a);
                {
                    float x = Mathf.Lerp(-12, 12, t);
                    Vector2 a0 = centre + new Vector2(x, -7), a1 = centre + new Vector2(x, 7);
                    ink.FloorLane(a0, a1, 1.4f, A(ArsenalInk.Water.Body, .45f * a), A(ArsenalInk.Water.Glow, .1f * a));
                    ink.FloorStroke(a0 + Vector2.right * 1.3f, a1 + Vector2.right * 1.3f, ArsenalInk.Water, .1f, 1, 1, a, .15f, 3);
                    break;
                }
                case FieldKind.Inferno:
                    for (int i = 0; i < 16; i++) ElementStroke(ElementLook.Fire, centre + new Vector2((R(24, i) - .5f) * 18, (R(25, i) - .5f) * 10), centre + new Vector2((R(24, i + 1) - .5f) * 18, (R(25, i + 1) - .5f) * 10), i, 24, now, 1.2f, a);
                    ink.Vignette(centre, 28, ArsenalInk.Amber.Glow, .22f * a, .6f * a);
                    break;
                case FieldKind.Lost:
                    ink.Vignette(centre, 28, ArsenalInk.Midnight.Ink, .5f * Mathf.Min(1, t * 3), .95f * Mathf.Min(1, t * 3));
                    break;
                case FieldKind.Realm:
                    // a boss changes phase: the arena flashes red and settles
                    ink.Vignette(centre, 28, BossTelegraph.Danger.Body, .3f * a, .8f * a);
                    break;
                case FieldKind.Pandemic:
                    for (int i = 0; i < 16; i++) ElementStroke(ElementLook.Poison, centre + new Vector2((R(24, i) - .5f) * 18, (R(25, i) - .5f) * 10), centre + new Vector2((R(24, i + 1) - .5f) * 18, (R(25, i + 1) - .5f) * 10), i, 24, now, 1.2f, a);
                    ink.Vignette(centre, 28, ArsenalInk.Green.Body, .22f * a, .6f * a);
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 p = centre + new Vector2(R(11, i) * 20 - 10, R(13, i) * 12 - 6 + t * 1.5f);
                        ink.Token(InkToken.Bubble, ArsenalInk.Green, p, .2f, 0, a, true);
                    }
                    break;
            }
        }

        /// <summary>Dusk takes strength from a marked enemy: a violet ribbon from the enemy to the friend that gains it.</summary>
        public void OnStolen(UnitState enemy, UnitState friend, float fraction)
        {
            if (enemy == null || friend == null || chainsThisFrame >= MaxChainsPerFrame) return;
            chainsThisFrame++;
            if (chains.Count >= MaxChains) chains.RemoveAt(0);
            chains.Add(new Chain { A = enemy.Position, B = friend.Position, Born = Now, Seed = ++seed, Violet = true });
        }

        /// <summary>Ember leaves embers where a burning enemy died: a small firepit for their lifetime.</summary>
        public void OnEmbers(Vector2 at, float lifetime)
            => Add(new Area { Shape = AreaShape.Disc, A = at, R = .8f, Born = Now, Life = Mathf.Max(.6f, lifetime), Look = ElementLook.Fire, Seed = ++seed, HasMotif = true, Motif = InkToken.Ember });

        /// <summary>A shield breaking on a friend: a loud glass pop.</summary>
        public void OnShieldBroken(UnitState friend) { if (friend != null) { EnemyPop(friend.Position, ElementLook.Crystal, 1.1f); Sound("rules.shieldbreak", friend.Position); } }

        /// <summary>Every friend down at once: the arena darkens before the wave ends.</summary>
        public void OnWaveLost(FriendWaveLostEvent e) => fields.Add(new Field { Kind = FieldKind.Lost, Born = Now, Life = 2.5f });

        public void OnFell(FriendMortalityEvent e) { fallen[e.FriendId] = e; fallenAt[e.FriendId] = Now; Sound("rules.fall", e.Position); }
        // the mortality clock may differ from the combat clock: the countdown runs from when the fall was seen
        private readonly Dictionary<int, float> fallenAt = new Dictionary<int, float>();
        public void OnRose(FriendMortalityEvent e) { Sound("rules.rise", e.Position); fallen.Remove(e.FriendId); if (bursts != null) bursts.Burst(ElementLook.Light, e.Position, Vector2.up, 1.1f); }

        public void OnEntity(RulesV3EntityRequest e)
        {
            if (e.Change == RulesV3EntityChange.Created) entities[e.Id] = e;
            else
            {
                entities.Remove(e.Id);
                if (e.Change == RulesV3EntityChange.Collected) Sound("rules.pickup", e.Position);
                if (e.Change == RulesV3EntityChange.Collected && bursts != null) bursts.Burst(LookFor(e.Rule), e.Position, Vector2.up, .8f);
            }
        }

        private string MainBond(int unitId)
        {
            UnitState unit = boundSimulation?.UnitForPresentation(unitId);
            if (unit == null || unit.Definition.Tags == null || unit.Definition.Tags.Length == 0) return null;
            // tags are bond ids ("static-club", "tidal", "orchard"...): read the bond's name, which names its element
            string id = unit.Definition.Tags[0];
            foreach (BondDefinition bond in BondTuning.V3Definitions) if (bond != null && string.Equals(bond.Id, id, StringComparison.OrdinalIgnoreCase)) return bond.Name;
            return id;
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
            ink.Begin(); vfx.Begin(); light.Clear();
            CombatSimulation sim = boundSimulation;
            bool live = sim != null && (sim.Phase == RunPhase.Combat || sim.Phase == RunPhase.Paused);
            if (!live) { areas.Clear(); fallen.Clear(); auras.Clear(); chains.Clear(); walls.Clear(); links.Clear(); ink.End(); vfx.End(); light.Commit(); vfx.ClearParticles(); return; }
            float now = sim.Elapsed;
            for (int i = fields.Count - 1; i >= 0; i--) { if (now - fields[i].Born > fields[i].Life) { fields.RemoveAt(i); continue; } DrawField(fields[i], sim.Champion.Position, now); }
            if (now < stillUntil) ink.Vignette(sim.Champion.Position, 30, ArsenalInk.Silver.Body, .05f, .3f);
            if (now < realmUntil) ink.Vignette(sim.Champion.Position, 30, ArsenalInk.Midnight.Glow, .04f, .26f);
            for (int i = walls.Count - 1; i >= 0; i--) { if (now > walls[i].Until) { walls.RemoveAt(i); continue; } DrawWall(walls[i], now); Walls++; }
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                Area a = areas[i]; float age = now - a.Born;
                if (age > a.Life || age < -1) { areas.RemoveAt(i); continue; }
                if (age < 0) continue; // still in flight
                if (!a.Landed && !a.Hostile) { Land(a); a.Landed = true; areas[i] = a; }
                if (!a.Landed && a.Hostile && !a.Gloom && age >= a.Warn) { Strike(a); a.Landed = true; areas[i] = a; }
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
            for (int i = throws.Count - 1; i >= 0; i--)
            {
                float k = (now - throws[i].Born) / throws[i].Life;
                if (k > 1 || k < 0) { throws.RemoveAt(i); continue; }
                DrawThrow(throws[i], k);
            }
            for (int i = pops.Count - 1; i >= 0; i--)
            {
                float age = now - pops[i].Born;
                if (age > PopLife) { pops.RemoveAt(i); continue; }
                if (age < 0) continue;
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
            ink.End(); vfx.End(); light.Commit();
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

        /// <summary>An enemy gloom zone: one solid dark colour from centre to edge (no rim), its edge boiling slowly, its pieces
        /// drifting inside.</summary>
        private void DrawGloom(Area a, float age)
        {
            InkPalette p = PaletteFor(a.Look);
            float now = boundSimulation.Elapsed, t = Mathf.Clamp01(age / a.Life), grow = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .25f)), alpha = 1 - t * t;
            Color fill = Color.Lerp(p.Ink, p.Body, .35f); fill.a = .82f;
            vfx.Area(RulesVfx.Shape.Ellipse, a.A, a.B, a.R, 0, fill, fill, grow, alpha, .16f, a.Seed % 97, true, Ground(ElementLook.Void));
            Pieces(a, p, grow, alpha * .8f, now);
        }

        /// <summary>A friendly skill shows as one living body of its element where it acts, never a target zone and never a
        /// scatter of separate spots: a disc is a mass burning densest at its heart and thinning to its edge (a bonfire,
        /// a whirlpool, lightning struck out from the heart, frost grown from it); a lane is one unbroken jet or bolt from end
        /// to end; a cone is a spray from its point. The creature's own piece rides once at the heart.</summary>
        private void DrawFriendly(Area a, float age)
        {
            InkPalette p = PaletteFor(a.Look);
            float now = boundSimulation.Elapsed, t = Mathf.Clamp01(age / a.Life), fadeOut = Mathf.Clamp01((1 - t) / .2f);
            float grow = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .3f)), show = grow * fadeOut;
            Vector2 heart;
            if (a.Shape == AreaShape.Line) { Jet(a, p, show, grow, now); heart = Vector2.Lerp(a.A, a.B, .5f); }
            else if (a.Shape == AreaShape.Cone) { Spray(a, p, show, grow, now); heart = Vector2.Lerp(a.A, a.B, .55f); }
            else { Mass(a, p, show, grow, now); heart = a.A; }
            if (a.HasMotif) ink.Token(a.Motif, p, heart + new Vector2(0, .45f + Mathf.Abs(Mathf.Sin(now * 3)) * .12f), .4f, now * 60, fadeOut, true);
        }

        private static Vector2 Flat(Vector2 v) => new Vector2(v.x, v.y * .62f);

        /// <summary>A lightning path from a to b: short steps that wander off the line at random and are pulled back to it,
        /// pinned at both ends, so no two bolts share a regular saw-tooth.</summary>
        private List<Vector2> Bolt(Vector2 a, Vector2 b, float jag, int seed)
        {
            path.Clear();
            float len = Vector2.Distance(a, b);
            int steps = Mathf.Clamp(Mathf.CeilToInt(len * 6), 4, 40);
            Vector2 side = len > 1e-4f ? new Vector2(-(b - a).y, (b - a).x) / len : Vector2.zero;
            float off = 0;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                off = off * .6f + (R(seed, i) - .5f) * jag * 2.2f;
                path.Add(Vector2.Lerp(a, b, t) + side * off * Mathf.Clamp01(Mathf.Sin(t * Mathf.PI) * 3));
            }
            return path;
        }
        private int Samples(float span) => PresentationSettings.ReducedEffects ? 3 : Mathf.Clamp(Mathf.RoundToInt(4 + span * 2.5f), 5, 14);
        private readonly List<Vector2> path = new List<Vector2>(32);

        /// <summary>A disc skill as one mass: its matter is born every frame at fresh points, most at the heart and few at the
        /// edge, so it reads as one body; its light is one connected shape grown from the heart.</summary>
        private void Mass(Area a, InkPalette p, float show, float grow, float now)
        {
            Vector2 c = a.A;
            float r = a.R * (.35f + .65f * grow);
            int samples = Samples(a.R * 1.5f), frame = Mathf.FloorToInt(now * 18);
            float rate = Mathf.Clamp((a.R * a.R * 1.5f + 3) * 1.4f, 4, 30) / samples;
            for (int i = 0; i < samples; i++)
            {
                float u = Mathf.Pow(Random.value, 1.7f), ang = Random.value * 360;
                FieldParticles(a.Look, c + Flat(Dir(ang)) * r * u, show * (1 - u * .6f), rate, 1.5f - u);
            }
            switch (a.Look)
            {
                case ElementLook.Storm:
                {
                    // lightning struck out from the heart, new forks about nine times a second, each flickering
                    int strike = Mathf.FloorToInt(now * 9), bolts = Mathf.Clamp(3 + Mathf.RoundToInt(a.R * 1.5f), 4, 8);
                    for (int k = 0; k < bolts; k++)
                    {
                        float ang = R(a.Seed + strike, k) * 360, reach = r * (.55f + .45f * R(a.Seed + strike, k + 20));
                        Vector2 end = c + Flat(Dir(ang)) * reach;
                        Light(Bolt(c, end, .06f + reach * .05f, a.Seed + frame * 7 + k), p, .05f, show);
                        Vector2 mid = Vector2.Lerp(c, end, .5f + .2f * R(strike, k));
                        Light(Bolt(mid, mid + Flat(Dir(ang + (R(strike, k + 9) - .5f) * 100)) * reach * .4f, .05f, frame * 3 + k), p, .03f, show * .8f);
                    }
                    if (frame % 3 != 0) Glint(c, .3f, p, show);
                    break;
                }
                case ElementLook.Ice: case ElementLook.Crystal:
                {
                    // one frost crystal grown from the heart to the edge, twigs along each arm, small crystals at the tips
                    float turn = a.Seed * 17 % 60 + now * (a.Look == ElementLook.Crystal ? 14 : 4), shine = .65f + .35f * Mathf.Sin(now * 3);
                    for (int k = 0; k < 6; k++)
                    {
                        float ang = turn + k * 60; Vector2 tip = c + Flat(Dir(ang)) * r * .95f;
                        Light(InkCanvas.Line(c, tip), p, .05f, show * shine);
                        for (int j = 1; j <= 3; j++)
                        {
                            Vector2 at = Vector2.Lerp(c, tip, j / 4f); float len = r * (.36f - j * .07f);
                            Light(InkCanvas.Line(at, at + Flat(Dir(ang + 50)) * len), p, .03f, show * shine * .85f);
                            Light(InkCanvas.Line(at, at + Flat(Dir(ang - 50)) * len), p, .03f, show * shine * .85f);
                        }
                        ElementStroke(a.Look, tip, tip, k, a.Seed, now, .8f, show);
                    }
                    break;
                }
                case ElementLook.Water: Spiral(c, r, 3, now * 2.4f, 3.6f, p, .05f, show); break;
                case ElementLook.Wind: Spiral(c, r, 4, now * 4f, 2.6f, p, .03f, show * .85f); break;
                case ElementLook.Void: case ElementLook.Dream: Spiral(c, r, 3, -now * 1.8f, -4f, p, .045f, show); break;
                case ElementLook.Bloom: Spiral(c, r * .85f, 5, now * .8f, 2.2f, p, .035f, show * .8f); break;
                case ElementLook.Fire: break; // the cartoon flames are the whole bonfire
                case ElementLook.Light: case ElementLook.Gold: case ElementLook.Sound:
                {
                    // rays from the heart, turning slowly; sound's rays ripple
                    bool sound = a.Look == ElementLook.Sound;
                    for (int k = 0; k < 7; k++)
                    {
                        float ang = now * 20 + k * 360f / 7 + a.Seed;
                        Light(InkCanvas.Line(c, c + Flat(Dir(ang)) * r, sound ? .06f : 0, sound ? 5 : 0, false, a.Seed + k), p, .035f, show * (.6f + .4f * Mathf.Sin(now * 5 + k)));
                    }
                    break;
                }
                case ElementLook.Time:
                    ElementStroke(ElementLook.Time, c, c, 0, a.Seed, now, r * 5, show);
                    break;
                case ElementLook.Earth: case ElementLook.Paper:
                    // cracks run out from the heart
                    for (int k = 0; k < 6; k++)
                    {
                        float ang = a.Seed * 31 % 360 + k * 60 + R(a.Seed, k) * 25;
                        ink.FloorStroke(c, c + Flat(Dir(ang)) * r * (.7f + .3f * R(a.Seed, k + 5)), p, .05f, 1, 1, show, .12f, a.Seed + k);
                    }
                    break;
                default:
                    ElementStroke(a.Look, c, c, 0, a.Seed, now, 1.4f, show);
                    break;
            }
        }

        /// <summary>Spiral arms of light turning about a centre: a whirlpool, a gust, a void drawing in, a swirl of petals.</summary>
        private void Spiral(Vector2 c, float r, int arms, float turn, float twist, InkPalette p, float width, float alpha)
        {
            for (int k = 0; k < arms; k++)
            {
                path.Clear();
                for (int q = 0; q <= 18; q++)
                {
                    float u = q / 18f, ang = turn + k * Mathf.PI * 2 / arms + u * twist, rad = r * (.1f + .9f * u);
                    path.Add(c + new Vector2(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad * .62f));
                }
                Light(path, p, width, alpha, 1, 0);
            }
        }

        /// <summary>A lane skill as one unbroken jet from end to end, growing out from its start: a lightning bolt with forks,
        /// a frost spine with twigs, or braided strands of the element's light (flame, water, venom...) with its matter along it.</summary>
        private void Jet(Area a, InkPalette p, float show, float grow, float now)
        {
            Vector2 d = a.B - a.A; float len = d.magnitude;
            if (len < .05f) { Mass(a, p, show, grow, now); return; }
            Vector2 dir = d / len, side = Perp(d), head = Vector2.Lerp(a.A, a.B, grow);
            int frame = Mathf.FloorToInt(now * 18), samples = Samples(len * .7f);
            float rate = Mathf.Clamp((len / .75f + 1) * 1.4f, 4, 30) / samples;
            for (int i = 0; i < samples; i++)
                FieldParticles(a.Look, Vector2.Lerp(a.A, head, Random.value) + side * (Random.value - .5f) * a.R * .7f, show, rate, 1);
            switch (a.Look)
            {
                case ElementLook.Storm:
                    Light(Bolt(a.A, head, .14f, a.Seed + frame), p, .06f, show);
                    Light(Bolt(a.A, head, .22f, a.Seed + frame + 997), p, .035f, show * .55f);
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 at = Vector2.Lerp(a.A, head, R(frame, k));
                        Light(Bolt(at, at + (dir + side * (R(frame, k + 4) > .5f ? 1 : -1)).normalized * .55f, .06f, frame * 5 + k), p, .03f, show * .8f);
                    }
                    break;
                case ElementLook.Ice: case ElementLook.Crystal:
                {
                    Light(InkCanvas.Line(a.A, head), p, .05f, show);
                    int twigs = Mathf.Max(2, Mathf.FloorToInt(len * grow / .4f));
                    for (int j = 1; j <= twigs; j++)
                    {
                        Vector2 at = Vector2.Lerp(a.A, head, j / (twigs + 1f)); float s = j % 2 == 0 ? 1 : -1;
                        Light(InkCanvas.Line(at, at + (dir * .6f + side * s).normalized * a.R * .55f), p, .03f, show * .85f);
                    }
                    break;
                }
                default:
                {
                    // two or three strands braided along the lane, swelling in the middle
                    InkPalette pal = a.Look == ElementLook.Fire ? Ember : p;
                    int strands = a.Look == ElementLook.Wind ? 2 : 3;
                    for (int k = 0; k < strands; k++)
                    {
                        path.Clear();
                        // a few long waves over the whole lane (never more waves than the points can draw)
                        float waves = Mathf.Clamp(len * .45f, 1, 3);
                        for (int q = 0; q <= 32; q++)
                        {
                            float u = q / 32f;
                            // a nearly straight core and two strands of different wave and speed, so they never braid into a helix
                            float wave = Mathf.Sin(u * waves * (1 + k * .6f) * Mathf.PI * 2 - now * (5 + k * 2.5f) + k * 1.3f);
                            path.Add(Vector2.Lerp(a.A, head, u) + side * wave * a.R * (k == 0 ? .08f : k == 1 ? .38f : .24f) * (.3f + .7f * Mathf.Sin(u * Mathf.PI)));
                        }
                        Light(path, pal, k == 0 ? .055f : .035f, show * (k == 0 ? 1 : .7f));
                    }
                    if (a.Look == ElementLook.Fire) Light(InkCanvas.Line(a.A, head), FlameHeart, .03f, show * .8f);
                    break;
                }
            }
        }

        /// <summary>A cone skill as a spray from its point: strands of the element fanning out and its matter flying along
        /// them (a flamethrower of filmed fireballs, forked lightning, a fan of frost or water).</summary>
        private void Spray(Area a, InkPalette p, float show, float grow, float now)
        {
            Vector2 aim = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
            float reach = Mathf.Max(a.R, (a.B - a.A).magnitude) * grow, aimAng = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, half = 28;
            int frame = Mathf.FloorToInt(now * 18), samples = Samples(reach);
            float rate = Mathf.Clamp(reach * 2.2f * 1.4f, 4, 30) / samples;
            for (int i = 0; i < samples; i++)
            {
                float u = Mathf.Sqrt(Random.value);
                FieldParticles(a.Look, a.A + Dir(aimAng + (Random.value - .5f) * 2 * half * u) * reach * u, show, rate, .6f + u * .5f);
            }
            if (a.Look == ElementLook.Fire && Random.value < Time.deltaTime * 16 * show)
                vfx.EmitFlip("UL_FireBall01", "flame", a.A + aim * .25f, Dir(aimAng + (Random.value - .5f) * 1.6f * half) * reach / .55f, .45f + Random.value * .2f, .55f, 0);
            InkPalette pal = a.Look == ElementLook.Fire ? Ember : p;
            for (int k = 0; k < 5; k++)
            {
                float ang = aimAng + (k / 4f - .5f) * 2 * half * .85f + Mathf.Sin(now * 3 + k) * 3;
                Vector2 end = a.A + Dir(ang) * reach * (.75f + .25f * R(a.Seed, k));
                if (a.Look == ElementLook.Storm) Light(Bolt(a.A, end, .1f, a.Seed + frame * 11 + k), p, .045f, show);
                else Light(InkCanvas.Line(a.A, end, .05f, 3, false, a.Seed + k + now * 2), pal, k == 2 ? .05f : .035f, show * (k == 2 ? 1 : .75f));
            }
        }

        /// <summary>An area lands: a painted burst at its centre and its bits thrown out (no ring).</summary>
        private void Land(Area a)
        {
            Vector2 c = a.Shape == AreaShape.Line ? (a.A + a.B) * .5f : a.A;
            float size = Mathf.Clamp(a.R * .9f, .9f, 2.2f);
            AddPop(c, a.Look, size, 0);
            Sound(a.Look, "land", c);
        }

        private Sprite[] Bits(ElementLook look) => vfx.Library?.Get(look + "_Bits") ?? vfx.Library?.Get("Gold_Bits");
        private Sprite Ground(ElementLook look) { Sprite[] g = vfx.Library?.Get(look + "_Ground"); return g != null && g.Length > 0 ? g[0] : null; }

        /// <summary>The element's own pieces drifting inside the area (fewer with reduced effects).</summary>
        private void Pieces(Area a, InkPalette p, float grow, float alpha, float now)
        {
            InkToken token = a.HasMotif ? a.Motif : TokenFor(a.Look);
            // big areas carry more pieces so they never read as an empty wash
            int n = PresentationSettings.ReducedEffects ? 4 : Mathf.Clamp(8 + Mathf.RoundToInt(a.R * 3), 10, 26);
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
            }
        }

        /// <summary>The hero-skill standard for energy: light, not paint. A soft halo, a brighter body and a hot thread along the
        /// path, each fading to nothing at its sides and ends; no outline. Widths stay thin so many strokes never turn to mush.</summary>
        private void Light(List<Vector2> path, InkPalette p, float width, float alpha, float head = 1, float tail = 0)
        {
            if (alpha <= .01f || path == null || path.Count < 2) return;
            if (PresentationSettings.ReducedEffects) alpha *= .75f;
            light.GlowRibbon(path, head, tail, Mathf.Min(width * 3.2f, .45f), A(p.Glow, .3f * alpha));
            light.GlowRibbon(path, head, tail, Mathf.Min(width * 1.4f, .26f), A(Color.Lerp(p.Body, p.Light, .35f), .75f * alpha));
            light.GlowRibbon(path, head, tail, width * .5f, A(Color.Lerp(p.Light, Color.white, .45f), .92f * alpha));
        }
        private void Glint(Vector2 at, float size, InkPalette p, float alpha)
        {
            Light(InkCanvas.Line(at - Vector2.right * size, at + Vector2.right * size), p, size * .22f, alpha);
            Light(InkCanvas.Line(at - Vector2.up * size, at + Vector2.up * size), p, size * .22f, alpha);
        }
        private readonly List<Vector2> flame = new List<Vector2>(12);
        // flame light: a deep orange body with a red-orange halo, and a yellow-white heart
        private static readonly InkPalette Ember = new InkPalette(new Color(.3f, .06f, .02f), new Color(1f, .42f, .08f), new Color(1f, .78f, .35f), new Color(1f, .3f, .05f), new Color(1f, .6f, .2f));
        private static readonly InkPalette FlameHeart = new InkPalette(new Color(.3f, .2f, .05f), new Color(1f, .85f, .3f), new Color(1f, .97f, .75f), new Color(1f, .7f, .2f), new Color(1f, .9f, .5f));
        private static Vector2 Dir(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));

        /// <summary>One element's living stroke at a spot of a skill, drawn as light and redrawn every frame: lightning forks to the
        /// next spot and flickers 18 times a second, flame tongues lick upward with embers, water curls, frost shards glint,
        /// wind streaks, bubbles rise, leaves turn, notes wave, clock hands turn. Sizes are small; a skill reads from many of them.</summary>
        private void ElementStroke(ElementLook look, Vector2 at, Vector2 next, int i, int seed, float now, float s, float alpha)
        {
            if (alpha <= .01f || s <= .01f) return;
            InkPalette p = PaletteFor(look);
            int frame = Mathf.FloorToInt(now * 18), k0 = seed + i * 13;
            float r1 = R(seed, i), r2 = R(seed, i + 31), wob = Mathf.Sin(now * 7 + i * 1.9f);
            switch (look)
            {
                case ElementLook.Storm:
                {
                    Vector2 d = next - at; float len = d.magnitude;
                    if (len > .2f && len < 3.2f) Light(Bolt(at, next, .08f + len * .05f, k0 + frame * 7), p, .045f, alpha);
                    Vector2 mid = Vector2.Lerp(at, next, .4f + .2f * r1);
                    if ((frame + i) % 3 != 0) Light(Bolt(mid, mid + Dir(r2 * 360 + frame * 23) * .3f * s, .05f, k0 + frame * 7 + 5), p, .03f, alpha * .8f);
                    if ((frame + i) % 4 == 0) Glint(at, .09f * s, p, alpha);
                    break;
                }
                case ElementLook.Fire:
                    // flame tongues: a deep orange body that narrows to a curling tip, a yellow heart inside, embers rising
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 b = at + new Vector2((k - 1) * .09f * s, 0);
                        float h = (k == 1 ? .42f : .28f) * s * (.8f + .2f * Mathf.Sin(now * 10 + k * 2.1f + i)) * (.85f + .3f * R(seed, i * 3 + k));
                        flame.Clear();
                        for (int q = 0; q <= 8; q++)
                        {
                            float u = q / 8f, sway = Mathf.Sin(now * 7 + k * 1.7f + i - u * 3) * .07f * s * u;
                            flame.Add(b + new Vector2(sway, u * h));
                        }
                        Light(flame, Ember, .09f * s * (k == 1 ? 1.15f : .9f), alpha, 1, 0);
                        flame.RemoveRange(6, flame.Count - 6);
                        Light(flame, FlameHeart, .045f * s, alpha, 1, 0);
                    }
                    for (int e = 0; e < 2; e++)
                    {
                        float u = Mathf.Repeat(now * .8f + R(seed, i + e * 7), 1);
                        Vector2 ember = at + new Vector2(Mathf.Sin(now * 3 + e + i) * .12f, .35f + u * .6f) * s;
                        Light(InkCanvas.Line(ember, ember + Vector2.up * .035f), Ember, .03f, alpha * (1 - u));
                    }
                    break;
                case ElementLook.Water:
                {
                    // a wave crest rolling over: a rising line that curls into its tip, drops flicked off the lip
                    float roll = Mathf.Repeat(now * .7f + r1, 1), lift = Mathf.Sin(roll * Mathf.PI);
                    Vector2 b = at + new Vector2((roll - .5f) * .3f * s, 0);
                    flame.Clear();
                    for (int q = 0; q <= 9; q++)
                    {
                        float u = q / 9f, ang = 200 - u * 230;
                        flame.Add(u < .55f ? b + new Vector2(u * .42f * s, Mathf.Sin(u * 2.8f) * .16f * s * lift) : b + new Vector2(.23f * s, .14f * s * lift) + Dir(ang) * .09f * s * lift);
                    }
                    Light(flame, p, .05f * s, alpha * (.4f + .6f * lift));
                    for (int d = 0; d < 3; d++)
                    {
                        float u = Mathf.Repeat(now * 1.4f + d * .33f + r2, 1);
                        Vector2 drop = b + new Vector2(.28f * s + d * .05f, .2f * s * lift) + new Vector2(u * .18f, u * .15f - u * u * .45f) * s;
                        Light(InkCanvas.Line(drop, drop + new Vector2(.02f, -.04f)), p, .03f, alpha * (1 - u) * lift);
                    }
                    break;
                }
                case ElementLook.Ice: case ElementLook.Crystal:
                {
                    // a frost crystal: six arms with side twigs, slowly turning, a glint running along it
                    float turn = r1 * 60 + (look == ElementLook.Crystal ? now * 25 : now * 6), arm = (.13f + .07f * r2) * s;
                    float shine = .55f + .45f * Mathf.Sin(now * 4 + i * 1.3f);
                    for (int k = 0; k < 6; k++)
                    {
                        Vector2 d = Dir(turn + k * 60), tip = at + d * arm, twig = at + d * arm * .55f;
                        Light(InkCanvas.Line(at, tip), p, .028f * s, alpha * shine);
                        Light(InkCanvas.Line(twig, twig + Dir(turn + k * 60 + 45) * arm * .35f), p, .02f * s, alpha * shine * .85f);
                        Light(InkCanvas.Line(twig, twig + Dir(turn + k * 60 - 45) * arm * .35f), p, .02f * s, alpha * shine * .85f);
                    }
                    if ((frame + i) % 7 == 0) Glint(at + Dir(turn) * arm, .08f * s, p, alpha);
                    break;
                }
                case ElementLook.Wind:
                {
                    Vector2 dir = Dir(15 + r1 * 20); float u = Mathf.Repeat(now * .9f + r2, 1);
                    Vector2 c = at + dir * (u - .5f) * .6f * s;
                    Light(InkCanvas.Line(c - dir * .3f * s, c + dir * .3f * s, .06f * s, 2.5f, false, k0), p, .03f * s, alpha * Mathf.Sin(u * Mathf.PI));
                    break;
                }
                case ElementLook.Poison:
                    for (int k = 0; k < 2; k++)
                    {
                        float u = Mathf.Repeat(now * .55f + k * .5f + r1, 1), rr = (.04f + .03f * k) * s;
                        Vector2 c = at + new Vector2((k - .5f) * .12f * s + Mathf.Sin(now * 2 + k) * .03f, u * .45f * s);
                        Light(InkCanvas.Arc(c, rr, rr, 20, 250, 8), p, .025f, alpha * (1 - u));
                    }
                    Light(InkCanvas.Line(at, at + new Vector2(.08f * wob, .3f) * s, .05f, 2, false, k0), p, .03f * s, alpha * .6f);
                    break;
                case ElementLook.Bloom:
                {
                    float ang = now * 60 + r1 * 360, fall = Mathf.Repeat(now * .4f + r2, 1);
                    Vector2 c = at + new Vector2(Mathf.Sin(now + i) * .1f, (.5f - fall) * .4f) * s;
                    Light(InkCanvas.Arc(c, .1f * s, .045f * s, ang, ang + 180, 8), p, .03f * s, alpha);
                    Light(InkCanvas.Arc(c, .1f * s, .045f * s, ang + 180, ang + 360, 8), p, .03f * s, alpha * .7f);
                    break;
                }
                case ElementLook.Void: case ElementLook.Dream:
                {
                    Vector2 curl = Dir(now * 80 + r1 * 360) * .25f * s;
                    Light(InkCanvas.Line(at, at + curl, .06f * s, 2, false, k0 + frame / 6), p, .035f * s, alpha);
                    if ((frame + i) % 6 == 0) Glint(at + curl, .05f, PaletteFor(ElementLook.Light), alpha);
                    break;
                }
                case ElementLook.Sound:
                {
                    Vector2 dir = Dir(r1 * 360); float u = Mathf.Repeat(now * 1.2f + r2, 1);
                    Vector2 a0 = at + dir * u * .3f * s;
                    Light(InkCanvas.Line(a0, a0 + dir * .3f * s, .04f * s, 6, false, k0), p, .03f * s, alpha * (1 - u));
                    break;
                }
                case ElementLook.Time:
                    Light(InkCanvas.Line(at, at + Dir(90 - now * 160 - r1 * 360) * .16f * s), p, .035f * s, alpha);
                    Light(InkCanvas.Line(at, at + Dir(90 - now * 14 - r2 * 360) * .1f * s), p, .045f * s, alpha);
                    if ((frame + i) % 6 == 0) Glint(at, .05f, p, alpha);
                    break;
                case ElementLook.Earth: case ElementLook.Paper:
                {
                    // dust and scraps are matter, not light: thin ink strokes that drift and settle
                    float u = Mathf.Repeat(now * .7f + r1, 1);
                    Vector2 c = at + new Vector2((r2 - .5f) * .3f, u * .35f) * s;
                    ink.FloorStroke(c, c + Dir(r2 * 180) * .12f * s, p, .04f, 1, 1, alpha * (1 - u), .02f, k0);
                    break;
                }
                default:
                    if ((frame + i) % 3 == 0) Glint(at + new Vector2(0, .15f) * s, (.06f + .05f * r1) * s, p, alpha);
                    break;
            }
        }

        /// <summary>An element burst in the hero standard: rays of the element's light thrown from the point (forked for storm,
        /// tongues for fire, shards for frost...) and a short hot cross; no painted sprite, no ring.</summary>
        private void LightBurst(Vector2 c, ElementLook look, float size, float t, int seed)
        {
            InkPalette p = PaletteFor(look);
            float grow = 1 - (1 - t) * (1 - t) * (1 - t), fade = 1 - t * t;
            int rays = PresentationSettings.ReducedEffects ? 5 : 9, frame = Mathf.FloorToInt(Now * 18);
            for (int i = 0; i < rays; i++)
            {
                float ang = seed * 37 % 360 + i * 360f / rays + R(seed, i) * 20, reach = (.35f + .45f * R(seed, i + 9)) * size;
                Vector2 end = c + Dir(ang) * reach * grow;
                bool zig = look == ElementLook.Storm;
                float amp = look == ElementLook.Fire || look == ElementLook.Water ? .05f : zig ? reach * .12f : 0;
                Light(zig ? Bolt(c, end, amp, seed * 31 + i + frame * 7) : InkCanvas.Line(c, end, amp, 3, false, seed + i), p, .045f * Mathf.Sqrt(size), fade, 1, Mathf.Max(0, t * 1.2f - .2f));
            }
            if (t < .35f) Glint(c, .22f * size * (1 - t), p, 1 - t / .35f);
        }

        /// <summary>The particle layers of an element where a skill acts, emitted at a rate (per second) so the field keeps
        /// burning: fire is flame tongues rising through orange to red with embers and a little smoke; storm is lightning
        /// flashes and white-blue sparks; frost is glinting stars and drifting crystals; water is swirls and leaping drops;
        /// venom is green fumes and bubbles; and so on. Every layer is a CC0 particle texture tinted by its ramp.</summary>
        private static readonly string[] Flames = { "UL_Flame02", "UL_Flame03" };
        private void FieldParticles(ElementLook look, Vector2 at, float alpha, float rateScale, float scale = 1)
        {
            if (alpha <= .05f) return;
            float dt = Time.deltaTime * rateScale * alpha * (PresentationSettings.ReducedEffects ? .4f : 1);
            Vector2 jitter = new Vector2((Random.value - .5f) * .25f, (Random.value - .5f) * .12f);
            switch (look)
            {
                case ElementLook.Fire:
                    // cartoon flames standing on the spot: a flipbook's moving shape painted in flat bands (dark red outline,
                    // red, orange, yellow heart), overlapping into one fire; embers fly up and cartoon smoke puffs off the top
                    if (Random.value < dt * 5) vfx.EmitFlip(Flames[Random.Range(0, Flames.Length)], "flame", at + jitter * .5f, new Vector2((Random.value - .5f) * .15f, .15f), (.35f + Random.value * Random.value * .7f) * scale, .55f + Random.value * .5f, 0, null, true);
                    if (Random.value < dt * 5) vfx.Emit(vfx.Tex("spark"), "ember", at + jitter, new Vector2((Random.value - .5f) * 1.2f, 1.2f + Random.value * 1.4f), .06f + Random.value * .05f, .7f + Random.value * .5f, 200);
                    if (Random.value < dt * .3f) vfx.EmitFlip("UL_WispySmoke01", "toonsmoke", at + jitter + Vector2.up * .8f * scale, new Vector2((Random.value - .5f) * .2f, .5f), (.4f + Random.value * .25f) * scale, 1.2f, (Random.value - .5f) * 30);
                    break;
                case ElementLook.Storm:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("trace"), "storm", at + jitter, Vector2.zero, .5f + Random.value * .4f, .1f + Random.value * .08f, 0);
                    if (Random.value < dt * 8) vfx.Emit(vfx.Tex("spark"), "storm", at + jitter, Random.insideUnitCircle * 2.4f, .05f + Random.value * .04f, .25f + Random.value * .2f, 0);
                    if (Random.value < dt * 2) vfx.Emit(vfx.Tex("light"), "glow", at, Vector2.zero, .55f, .18f, 0, new Color(.55f, .85f, 1f, .7f));
                    break;
                case ElementLook.Ice: case ElementLook.Crystal:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("star"), "ice", at + jitter, new Vector2(0, .15f), .14f + Random.value * .14f, .55f + Random.value * .3f, (Random.value - .5f) * 90);
                    if (Random.value < dt * 4) vfx.Emit(vfx.Tex("spark"), "ice", at + jitter + Vector2.up * .3f, new Vector2((Random.value - .5f) * .4f, -.2f), .05f, 1f, 30);
                    if (Random.value < dt * .8f) vfx.Emit(vfx.Tex("magic"), "ice", at, Vector2.zero, .4f, .5f, 40, new Color(1, 1, 1, .6f));
                    break;
                case ElementLook.Water:
                    if (Random.value < dt * 2) vfx.Emit(vfx.Tex("twirl"), "water", at + jitter, new Vector2(0, .1f), .35f + Random.value * .2f, .55f, (Random.value > .5f ? 1 : -1) * 260);
                    if (Random.value < dt * 7) vfx.Emit(vfx.Tex("light"), "water", at + jitter, new Vector2((Random.value - .5f) * 1.2f, 1.4f + Random.value), .06f + Random.value * .04f, .6f, 0);
                    break;
                case ElementLook.Poison:
                    if (Random.value < dt * 2.2f) vfx.EmitFlip("UL_WispySmoke01", "toonpoison", at + jitter, new Vector2((Random.value - .5f) * .2f, .35f), .3f + Random.value * .2f, 1f, (Random.value - .5f) * 50);
                    if (Random.value < dt * 4) vfx.Emit(vfx.Tex("light"), "poison", at + jitter, new Vector2((Random.value - .5f) * .2f, .6f + Random.value * .4f), .06f + Random.value * .05f, .8f, 0);
                    break;
                case ElementLook.Bloom:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("magic"), "bloom", at + jitter, new Vector2((Random.value - .5f) * .3f, .3f), .18f + Random.value * .1f, .8f, 90);
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("star"), "bloom", at + jitter + Vector2.up * .4f, new Vector2((Random.value - .5f) * .5f, -.1f), .08f, 1.1f, 120);
                    break;
                case ElementLook.Void: case ElementLook.Dream:
                    if (Random.value < dt * 2.2f) vfx.EmitFlip("UL_WispySmoke01", "toonvoid", at + jitter, new Vector2((Random.value - .5f) * .2f, .25f), .32f + Random.value * .2f, .9f, (Random.value - .5f) * 60);
                    if (Random.value < dt * 2) vfx.Emit(vfx.Tex("star"), "light", at + jitter + Vector2.up * .3f, Vector2.zero, .07f, .5f, 90);
                    break;
                case ElementLook.Light: case ElementLook.Gold:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("star"), "light", at + jitter, new Vector2(0, .3f), .14f + Random.value * .12f, .5f, 120);
                    if (Random.value < dt * 1) vfx.Emit(vfx.Tex("flare"), "light", at, Vector2.zero, .5f, .25f, 0);
                    break;
                case ElementLook.Sound:
                    if (Random.value < dt * 2.5f) vfx.Emit(vfx.Tex("magic"), "sound", at + jitter, new Vector2((Random.value - .5f) * .6f, .5f), .16f + Random.value * .1f, .7f, 140);
                    break;
                case ElementLook.Time:
                    if (Random.value < dt * 2) vfx.Emit(vfx.Tex("magic"), "time", at + jitter, Vector2.zero, .3f, .6f, 180);
                    break;
                case ElementLook.Earth: case ElementLook.Paper:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("dirt"), "dust", at + jitter, new Vector2((Random.value - .5f) * .8f, .8f + Random.value * .6f), .18f + Random.value * .12f, .7f, (Random.value - .5f) * 200);
                    if (Random.value < dt * 1) vfx.EmitFlip("UL_WispySmoke01", "toondust", at + jitter, new Vector2(0, .3f), .35f, 1f, 30);
                    break;
                case ElementLook.Wind:
                    if (Random.value < dt * 3) vfx.Emit(vfx.Tex("trace"), "time", at + jitter, new Vector2(1.6f, .2f), .35f, .35f, 0, new Color(1, 1, 1, .5f));
                    break;
            }
        }

        /// <summary>The particle burst of an element: a glow in its colour, a hot flash, then its own matter thrown out (flames and
        /// smoke, lightning flashes and sparks, frost stars, water drops, fumes, dust...). Emitted once when the burst begins.</summary>
        private void BurstParticles(ElementLook look, Vector2 c, float size)
        {
            int n = PresentationSettings.ReducedEffects ? 4 : 10;
            Color tint = Saturate(PaletteFor(look).Glow); tint.a = .8f;
            vfx.Emit(vfx.Tex("light"), "glow", c, Vector2.zero, 1.5f * size, .4f, 0, tint);
            if (cartoon != null)
            {
                // the cartoon blast of the element (Cartoon FX), and now and then a comic word over a big one
                string blast = look switch
                {
                    ElementLook.Fire => "burst_fire", ElementLook.Storm => "burst_storm", ElementLook.Water => "burst_water",
                    ElementLook.Earth or ElementLook.Paper => "burst_boom", ElementLook.Void or ElementLook.Dream => "burst_souls",
                    ElementLook.Ice or ElementLook.Crystal => "hit_ice", ElementLook.Light or ElementLook.Gold => "burst_flash", _ => "burst_poof",
                };
                cartoon.Play(blast, c, (blast == "burst_flash" || blast == "burst_boom" ? .3f : .45f) * size);
                if (size >= 1.6f && Time.time >= nextWordAt && !PresentationSettings.ReducedEffects)
                {
                    nextWordAt = Time.time + 3f;
                    string word = look switch
                    {
                        ElementLook.Ice or ElementLook.Crystal => "word_frozen", ElementLook.Poison => "word_poisoned",
                        ElementLook.Fire or ElementLook.Earth => "word_boom", ElementLook.Storm or ElementLook.Light => "word_pow", _ => "word_wham",
                    };
                    cartoon.Play(word, c + Vector2.up * (.8f + .3f * size), .5f);
                }
            }
            if (look == ElementLook.Fire)
            {
                // a cartoon fireball puff, then a cartoon smoke puff rolling up off it
                vfx.EmitFlip("UL_FireBall01", "flame", c, Vector2.zero, 1.1f * size, .5f, 0);
                vfx.EmitFlip("UL_WispySmoke01", "toonsmoke", c + Vector2.up * .35f * size, Vector2.up * .4f, 1f * size, 1.1f, 20);
            }
            else vfx.Emit(vfx.Tex("flare"), "light", c, Vector2.zero, .8f * size, .22f, 0);
            for (int i = 0; i < n; i++)
            {
                Vector2 v = Random.insideUnitCircle.normalized * (1.6f + Random.value * 2f) * Mathf.Sqrt(size);
                switch (look)
                {
                    case ElementLook.Fire:
                        if (i % 2 == 0) vfx.EmitFlip("UL_FireBall01", "flame", c, v * .5f + Vector2.up * .5f, .35f * size, .45f + Random.value * .2f, 0);
                        vfx.Emit(vfx.Tex("spark"), "ember", c, v * 1.3f, .07f, .7f, 200);
                        if (i % 4 == 0) vfx.EmitFlip("UL_WispySmoke01", "toonsmoke", c, v * .3f + Vector2.up * .4f, .5f * size, 1.2f, 40);
                        break;
                    case ElementLook.Storm:
                        if (i % 3 == 0) vfx.Emit(vfx.Tex("trace"), "storm", c + v * .12f, Vector2.zero, .7f * size, .14f, 0);
                        vfx.Emit(vfx.Tex("spark"), "storm", c, v * 1.4f, .06f, .35f, 0);
                        break;
                    case ElementLook.Ice: case ElementLook.Crystal:
                        vfx.Emit(vfx.Tex("star"), "ice", c, v * .8f, .18f * size, .6f, 120);
                        vfx.Emit(vfx.Tex("spark"), "ice", c, v * 1.2f, .05f, .6f, 0);
                        break;
                    case ElementLook.Water:
                        vfx.Emit(vfx.Tex("light"), "water", c, v + Vector2.up * 1.2f, .08f, .7f, 0);
                        if (i % 4 == 0) vfx.Emit(vfx.Tex("twirl"), "water", c, v * .2f, .5f * size, .5f, 300);
                        break;
                    case ElementLook.Poison:
                        vfx.EmitFlip("UL_WispySmoke01", "toonpoison", c, v * .4f, .35f * size, .9f, 50);
                        vfx.Emit(vfx.Tex("light"), "poison", c, v, .07f, .7f, 0);
                        break;
                    case ElementLook.Earth: case ElementLook.Paper:
                        vfx.Emit(vfx.Tex("dirt"), "dust", c, v + Vector2.up * .8f, .2f * size, .7f, 200);
                        if (i % 3 == 0) vfx.EmitFlip("UL_WispySmoke01", "toondust", c, v * .3f, .5f * size, 1f, 30);
                        break;
                    case ElementLook.Void: case ElementLook.Dream:
                        vfx.EmitFlip("UL_WispySmoke01", "toonvoid", c, v * .5f, .35f * size, .8f, 60);
                        break;
                    default:
                        vfx.Emit(vfx.Tex("star"), look == ElementLook.Sound ? "sound" : look == ElementLook.Bloom ? "bloom" : look == ElementLook.Time ? "time" : "light", c, v, .14f * size, .55f, 160);
                        break;
                }
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
                    for (int i = 0; i < 4; i++)
                    {
                        float ang = now * 80 + i * 90; Vector2 pt = at + new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad) * r, Mathf.Sin(ang * Mathf.Deg2Rad) * r * .55f);
                        Light(InkCanvas.Line(pt, pt + Dir(ang + 90) * .14f), p, .035f, fade);
                    }
                    break;
                }
                case AuraKind.Stun:
                {
                    // stars (or ice shards) circling over the head
                    InkPalette p = PaletteFor(a.Look);
                    Vector2 head = unit.Position + Vector2.up * (unit.Definition.Radius * 2.2f + .3f);
                    for (int i = 0; i < 4; i++)
                    {
                        float ang = (now * 240 + i * 90) * Mathf.Deg2Rad;
                        ink.Token(a.Look == ElementLook.Ice ? InkToken.Shard : InkToken.Star, p, head + new Vector2(Mathf.Cos(ang) * r * .7f, Mathf.Sin(ang) * r * .22f), .32f, now * 200, fade, true);
                    }
                    break;
                }
                case AuraKind.Sleep:
                {
                    InkPalette p = PaletteFor(ElementLook.Dream);
                    for (int i = 0; i < 3; i++)
                    {
                        float u = Mathf.Repeat(now * .8f + i / 3f, 1);
                        ink.Token(InkToken.Moon, p, unit.Position + new Vector2(.2f + u * .4f, unit.Definition.Radius * 2 + u * .9f), .26f + .18f * u, -20, fade * (1 - u), true);
                    }
                    break;
                }
                case AuraKind.Root:
                {
                    InkPalette p = PaletteFor(ElementLook.Bloom);
                    for (int i = 0; i < 5; i++)
                    {
                        float ang = (i * 72 + a.Unit * 13) * Mathf.Deg2Rad;
                        ink.Token(InkToken.Leaf, p, unit.Position + new Vector2(Mathf.Cos(ang) * r * .85f, Mathf.Sin(ang) * r * .38f + .1f), .3f, i * 72, fade, false);
                    }
                    break;
                }
                case AuraKind.Untouchable:
                    for (int i = 0; i < 4; i++)
                    {
                        float u = Mathf.Repeat(now * 1.4f + i * .25f, 1);
                        ink.Token(InkToken.Star, ArsenalInk.Gold, unit.Position + new Vector2((i - 1.5f) * r * .4f, .2f + u * 1.2f), .36f, now * 120, fade * (1 - u), true);
                    }
                    break;
                default:
                {
                    InkPalette p = PaletteFor(a.Look);
                    InkToken token = a.Kind == AuraKind.Heal ? InkToken.Leaf : TokenFor(a.Look);
                    for (int i = 0; i < 3; i++)
                    {
                        float u = Mathf.Repeat(now * .9f + i / 3f + a.Unit * .17f, 1);
                        Glint(unit.Position + new Vector2((i - 1) * r * .35f, .25f + u * 1.1f), .06f * (1 - u * .5f), p, fade * (1 - u));
                    }
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
                if (s.FrozenUntil > now) { ElementStroke(ElementLook.Ice, enemy.Position + Vector2.up * enemy.Definition.Radius, enemy.Position, enemy.Id, enemy.Id, now, 1.3f, 1); any = true; }
                if (s.Marked) { ink.Token(InkToken.Moon, ArsenalInk.Violet, head + Vector2.up * (.18f + row * .13f), .34f, 0, 1, true); any = true; }
                if (s.SoakedUntil > now) { float u = Mathf.Repeat(now * 1.5f + enemy.Id * .3f, 1); Vector2 drip = enemy.Position + Vector2.up * (enemy.Definition.Radius * 2 - u * .5f); Light(InkCanvas.Line(drip, drip + Vector2.down * .07f), ArsenalInk.Water, .03f, 1 - u); any = true; }
                if (any) StatusMarks++;
            }
        }

        private void Meter(Vector2 head, float width, int row, float fill, InkPalette p)
        {
            Vector2 a = head + new Vector2(-width * .5f, row * .13f), b = a + Vector2.right * width;
            ink.Ribbon(InkCanvas.Line(a, b), p, .08f, 1, 1, .45f, false);
            if (fill > 0) ink.Ribbon(InkCanvas.Line(a, Vector2.Lerp(a, b, Mathf.Clamp01(fill))), p, .13f, 1, 1, 1, true);
        }

        /// <summary>A loud impact: the element's painted burst plays big, a flash at the centre and the element's pieces flung
        /// out and falling, all in under half a second (no ring).</summary>
        private void DrawPop(Pop p, float t)
        {
            Vector2 c = p.At + Vector2.up * .3f;
            if (p.Hostile)
            {
                // an enemy blow: dark red ink cracks and dust thrown up, matter not light
                float o = 1 - (1 - t) * (1 - t), a = 1 - t;
                for (int i = 0; i < 7; i++)
                {
                    float ang = p.Seed * 29 % 360 + i * 51 + R(p.Seed, i) * 20;
                    ink.FloorStroke(p.At, p.At + Dir(ang) * (.4f + .6f * R(p.Seed, i + 4)) * p.Size * o, BossTelegraph.Danger, .07f, 1, 0, a, .03f, p.Seed + i);
                }
                return;
            }
            LightBurst(c, p.Look, p.Size, t, p.Seed);
        }

        /// <summary>An enemy blow lands when its warning ends: the painted enemy burst over the struck ground and stone chips
        /// thrown up, along the lane for a charge.</summary>
        private void Strike(Area a)
        {
            float size = Mathf.Clamp(a.R * .8f, 1, 2.4f);
            owner?.Feel.Accent(.2f + .06f * size, 0);
            Sound("rules.strike", a.A);
            if (a.Shape == AreaShape.Line)
                for (int i = 0; i <= 3; i++) AddHostilePop(Vector2.Lerp(a.A, a.B, i / 3f), size * .7f);
            else AddHostilePop(a.Shape == AreaShape.Cone ? Vector2.Lerp(a.A, a.B, .6f) : a.A, size);
        }

        private void AddHostilePop(Vector2 at, float size)
        {
            for (int i = 0; i < (PresentationSettings.ReducedEffects ? 4 : 10); i++) vfx.Emit(vfx.Tex(i % 2 == 0 ? "dirt" : "smoke"), "dust", at, Random.insideUnitCircle * 2.2f * size + Vector2.up, (i % 2 == 0 ? .2f : .45f) * size, .8f, 120);
            if (pops.Count >= MaxPops) pops.RemoveAt(0);
            pops.Add(new Pop { At = at, Look = ElementLook.Fire, Born = Now, Size = size, Seed = ++seed, Hostile = true });
        }

        private void AddPop(Vector2 at, ElementLook look, float size, float delay)
        {
            if (pops.Count >= MaxPops) pops.RemoveAt(0);
            pops.Add(new Pop { At = at, Look = look, Born = Now + delay, Size = size, Seed = ++seed });
            if (delay <= 0 && vfx != null) BurstParticles(look, at + Vector2.up * .3f, size);
        }

        /// <summary>A skill's piece in flight: a glowing arc trail behind the spinning piece.</summary>
        private void DrawThrow(Throw w, float k)
        {
            InkPalette p = PaletteFor(w.Look);
            float height = w.Straight ? 0 : Mathf.Clamp((w.To - w.From).magnitude * .35f, .8f, 3f);
            var path = w.Straight ? InkCanvas.Line(w.From, w.To) : InkCanvas.Lob(w.From, w.To, height, 18);
            Light(path, p, w.Straight ? .07f : .05f, 1, k, Mathf.Max(0, k - .4f));
            Vector2 head = w.Straight ? Vector2.Lerp(w.From, w.To, k) : InkCanvas.Sample(InkCanvas.Lob(w.From, w.To, height, 18), k);
            ink.Token(w.Token, p, head, .42f, -k * 540, 1, true);
            FieldParticles(w.Look, head, 1, 3);
        }

        /// <summary>A chained hit: a forked, flickering lightning stroke with a light core.</summary>
        private void DrawChain(Chain c, float age)
        {
            float a = 1 - age / ChainLife;
            int flick = c.Seed + Mathf.FloorToInt(age * 40);
            Vector2 from = c.A + Vector2.up * .3f, to = c.B + Vector2.up * .3f;
            if (c.Violet)
            {
                // strength flowing: a smooth violet ribbon with motes sliding toward the friend
                ink.Ribbon(InkCanvas.Line(from, to, .12f, 2, false, c.Seed), ArsenalInk.Violet, .12f, 1, 1, a, true);
                for (int i = 0; i < 3; i++) ink.Token(InkToken.Moon, ArsenalInk.Violet, Vector2.Lerp(from, to, Mathf.Repeat(age * 3 + i / 3f, 1)), .3f, 0, a, true);
                return;
            }
            // a chain of lightning in the hero standard: a bright wandering bolt and a fainter twin, re-struck as it flickers
            Light(Bolt(from, to, .16f, flick * 13), ArsenalInk.Cyan, .055f, a);
            Light(Bolt(from, to, .24f, flick * 13 + 7), ArsenalInk.Cyan, .03f, .55f * a);
            Glint(to, .16f * a, ArsenalInk.Cyan, a);
            ink.Token(InkToken.Bolt, ArsenalInk.Cyan, to, .5f, flick * 37 % 360, a, true);
        }

        /// <summary>A friendly wall: a thick inked band in its element with a lit top edge, fading out at its end.</summary>
        private void DrawWall(Wall w, float now)
        {
            InkPalette p = PaletteFor(w.Look);
            float fade = Mathf.Clamp01((w.Until - now) / .3f) * Mathf.Clamp01((now - w.Born) / .15f);
            int stones = Mathf.Clamp(Mathf.RoundToInt((w.B - w.A).magnitude / .45f), 2, 16);
            for (int i = 0; i <= stones; i++) ElementStroke(ElementLook.Crystal, Vector2.Lerp(w.A, w.B, i / (float)stones) + Vector2.up * .2f, w.B, i, 3, now, .9f, fade);
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
            Vector2 a0 = at + new Vector2(-.45f, 1.05f), a1 = at + new Vector2(.45f, 1.05f);
            ink.Ribbon(InkCanvas.Line(a0, a1), ArsenalInk.Midnight, .12f, 1, 1, .7f, false);
            ink.Ribbon(InkCanvas.Line(a0, Vector2.Lerp(a0, a1, done)), p, .1f, 1, 1, 1, true);
            if (left < 2) ink.Token(InkToken.Glint, ArsenalInk.Gold, a1 + Vector2.up * .12f, .3f, now * 200, 1, true);
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
                    for (int i = 0; i < 4; i++) ElementStroke(look, at + new Vector2((i - 1.5f) * size * .3f, .05f), at + new Vector2((i - .5f) * size * .3f, .05f), i, e.Id, now, .8f, .9f);
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
                    Sprite art = SummonArt(e.Kind.ToString());
                    if (art != null) ink.Place(art, at + Vector2.up * (.3f + bob), .62f, 0, Color.white, true);
                    else ink.Token(token, p, at + Vector2.up * (.25f + bob), .6f, 0, 1, true);
                    ink.Token(InkToken.Glint, ArsenalInk.Gold, at + new Vector2(.25f, .55f + bob), .3f * pulse, now * 90, pulse, true);
                    break;
                }
            }
        }

        private static Color A(Color c, float a) { c.a = a; return c; }

        /// <summary>The rules sound for an element moment (rules.&lt;look&gt;.cast / land / burst) or a shared one (rules.heal...),
        /// placed in the world so off-screen ones are culled; cue cooldowns keep a crowd from stacking them.</summary>
        private static void Sound(string id, Vector2 at) => CheckpointAudio.PlayWorldId(id, at);
        private static void Sound(ElementLook look, string kind, Vector2 at) { if (look != ElementLook.None) Sound("rules." + look.ToString().ToLowerInvariant() + "." + kind, at); }
        /// <summary>The element colour pushed toward full saturation, so an added glow stays its colour instead of going white.</summary>
        private static Color Saturate(Color c) { Color.RGBToHSV(c, out float h, out float s, out float v); return Color.HSVToRGB(h, Mathf.Min(1, s * 1.5f + .15f), v); }

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
