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
            CombatFeedback.ElementTint = ElementColour; CombatFeedback.ElementTop = ElementNumberTop; CombatFeedback.ElementShape = ElementNumberShape; CombatFeedback.ElementInk = ElementNumberInk;
            ink = new ArsenalInk(transform);
            light = new InkCanvas(transform, "Rules light", 2141, true);
        }



        /// <summary>The element a hit's dealer fights with: a friend's main bond, the hero's own look, else none.</summary>
        private ElementLook DealerLook(CombatEvent e)
        {
            UnitState source = boundSimulation?.UnitForPresentation(e.Cause.SourceUnitId);
            if (source == null) return ElementLook.None;
            return source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : source.Faction == Faction.Champion ? ElementInk.For(source.Definition.Id) : ElementLook.None;
        }

        // ------------------------------------------------------------------ the two kinds of our effects
        /// <summary>Our side's effects come in two kinds only (requirement 28/10/2026). A targeted effect is a strike from its
        /// source to each target, like Storm Fencer's chained lightning (the hero renderer's line strike in the element's hero
        /// brush). An area effect is a blast over its area, like fire or frost bursting (the hero renderer's disc strike with
        /// the element's hero signature where it lands).</summary>
        private void Targeted(ElementLook look, Vector2 from, Vector2 to, float delay = 0)
        {
            if ((to - from).sqrMagnitude < .04f) { Blast(look, to, .5f, delay); return; }
            float at = Now + delay;
            HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = MorphGeometry.Line, Position = from + Vector2.up * .3f, End = to + Vector2.up * .3f,
                Direction = (to - from).normalized, Width = .35f, Created = at, At = at, Until = at + .5f, Seed = ++seed }, HeroIdentityVisual.GuestStyle(look));
        }

        private CartoonFx cartoon;
        private struct Burst { public string Key; public Vector2 At; public float Scale, When; }
        private readonly List<Burst> blastQueue = new List<Burst>();

        /// <summary>The element's explosion from the Cartoon FX library under an area blast, sized to the area.</summary>
        private static string BlastKey(ElementLook look) => look switch
        {
            ElementLook.Fire => "burst_fire", ElementLook.Ice or ElementLook.Crystal => "hit_ice", ElementLook.Storm => "burst_storm",
            ElementLook.Water => "burst_water", ElementLook.Poison => "burst_poison", ElementLook.Earth or ElementLook.Paper => "burst_boom",
            ElementLook.Void or ElementLook.Dream => "burst_souls", ElementLook.Light or ElementLook.Gold => "burst_flash", _ => "burst_poof",
        };

        private static float BlastSize(string key) => key switch
        {
            "burst_fire" => 1.6f, "hit_ice" => .95f, "burst_storm" => .7f, "burst_water" => .8f, "burst_poison" => .2f,
            "burst_boom" => .28f, "burst_souls" => .55f, "burst_flash" => .32f, _ => .8f,
        };

        /// <summary>The step a trait event belongs to: its moments grow with the step so each step reads differently.</summary>
        private static int RuleTier(string rule)
        {
            switch (rule)
            {
                case "freeze": case "frozen-hit": case "root-hit": case "wick": case "horn-pile-up": case "discharge": case "mask-double":
                case "shade-execute-growth": case "gravity-pull": case "sugar-rush": case "tide-splash": return 4;
                case "rot": case "horns-vulnerability": case "shatter": case "demolitionist-stun": case "feast": case "species-recorded": case "lightning-rod": return 6;
                case "inferno": case "blizzard": case "eclipse": case "pandemic": case "riptide": case "gale": case "leviathan": return 8;
                default: return 2;
            }
        }

        /// <summary>A trait step fires, sized by its step: step 2 a small burst on its enemy; step 4 an area blast with the
        /// element's explosion; step 6 a big blast with its hero signature and strikes leaping out to four enemies around;
        /// step 8 the element's finale falling from the sky onto the crowds.</summary>
        private void TierMoment(string rule, ElementLook look, Vector2 at)
        {
            switch (RuleTier(rule))
            {
                case 2:
                    HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = MorphGeometry.Disc, Position = at, End = at, Direction = Vector2.right,
                        Radius = .35f, Created = Now, At = Now + .06f, Until = Now + .5f, Seed = ++seed }, HeroIdentityVisual.GuestStyle(look));
                    break;
                case 4: Blast(look, at, .75f); break;
                case 6:
                {
                    Blast(look, at, 1.3f);
                    int n = 0;
                    if (boundSimulation != null)
                        foreach (UnitState unit in boundSimulation.Units)
                        {
                            if (unit == null || !unit.Alive || unit.Faction != Faction.Enemy || (unit.Position - at).sqrMagnitude > 9 || (unit.Position - at).sqrMagnitude < .1f) continue;
                            Targeted(look, at, unit.Position, .1f + n * .05f);
                            if (++n >= 4) break;
                        }
                    break;
                }
                default: Finale(look, at); BlastCrowds(look, at, false); break;
            }
        }

        /// <summary>A step-8 moment: the element's hero finale (its sign falling from the sky, then its finale burst) at the spot.</summary>
        private void Finale(ElementLook look, Vector2 at)
        {
            float now = Now;
            HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = MorphGeometry.Disc, Position = at, End = at, Direction = Vector2.right,
                Radius = 2.2f, Created = now, At = now + .45f, Until = now + 1.6f, Seed = ++seed, Big = true, Finale = true }, HeroIdentityVisual.GuestStyle(look));
        }

        private void Blast(ElementLook look, Vector2 at, float radius, float delay = 0, float gather = .12f)
        {
            float when = Now + delay;
            string key = BlastKey(look);
            // sized to the area, never too small to read; each Cartoon FX effect has its own natural size (the earth blast and
            // the venom cloud are huge, the fire and frost bursts small), so each is scaled to the same footprint
            float footprint = Mathf.Max(.55f, .35f + Mathf.Min(radius, 3) * .28f);
            if (blastQueue.Count < 48) blastQueue.Add(new Burst { Key = key, At = at + Vector2.up * .2f, Scale = footprint * BlastSize(key), When = when + gather });
            HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = MorphGeometry.Disc, Position = at, End = at, Direction = Vector2.right,
                Radius = Mathf.Max(.35f, radius), Created = when, At = when + gather, Until = when + gather + .6f, Seed = ++seed, Big = radius >= 1.5f }, HeroIdentityVisual.GuestStyle(look));
        }

        /// <summary>A big trait step (6 and 8) blasts the enemies where they crowd: up to six blasts on enemies spread over the
        /// arena, a moment apart; a chaining trait instead strikes from enemy to enemy.</summary>
        private void BlastCrowds(ElementLook look, Vector2 near, bool chain)
        {
            if (boundSimulation == null) return;
            var picked = new List<Vector2>();
            foreach (UnitState unit in boundSimulation.Units)
            {
                if (unit == null || !unit.Alive || unit.Faction != Faction.Enemy) continue;
                bool apart = true;
                foreach (Vector2 q in picked) if ((q - unit.Position).sqrMagnitude < 2.25f) { apart = false; break; }
                if (apart) picked.Add(unit.Position);
                if (picked.Count >= 6) break;
            }
            if (picked.Count == 0) { Blast(look, near, 1.6f); return; }
            Vector2 from = near;
            for (int i = 0; i < picked.Count; i++)
            {
                if (chain) { Targeted(look, from, picked[i], i * .09f); from = picked[i]; }
                else Blast(look, picked[i], 1.3f, i * .08f);
            }
        }


        /// <summary>Basic attacks draw nothing of their own under the rules v3 (only skills, traits and chains do).</summary>
        public void OnCombat(CombatEvent e) { }

        /// <summary>Our ordinary shots under the rules v3 fly as the shooter's own painted token in its element's hero palette,
        /// the way the hero's identity bolts do (HeroIdentityVisual.Missile), never as an old bullet painting.</summary>
        public Sprite ShotSprite(ProjectileState shot, float time, out float turn, out float size)
        {
            ElementLook look = LookOfUnit(shot.SourceUnitId);
            InkToken token = TokenFor(look);
            HeroMechanicStyle style = HeroIdentityVisual.GuestStyle(look);
            bool spin = token == InkToken.Star || token == InkToken.Coin || token == InkToken.Spark || token == InkToken.Shard || token == InkToken.Petal || token == InkToken.Leaf;
            turn = spin && !PresentationSettings.ReducedMotion ? time * 520 + shot.Id * 47 : -90;
            size = .3f;
            return MechanicInk.Token(token, style.Palette, 0);
        }



        private ElementLook LookOfUnit(int id)
        {
            UnitState source = boundSimulation?.UnitForPresentation(id);
            if (source == null) return ElementLook.Gold;
            ElementLook look = source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : source.Faction == Faction.Champion ? ElementInk.For(source.Definition.Id) : ElementLook.None;
            return look == ElementLook.None ? ElementLook.Gold : look;
        }




        /// <summary>The colour of a hit's element: its source companion's main bond, the hero's own look, else none.</summary>
        private readonly Dictionary<int, ElementLook> lastSkillLook = new Dictionary<int, ElementLook>();

        /// <summary>The element a hit was dealt in: a trait's own element for trait damage (a burn is fire, a chain is
        /// lightning, whoever carries it), the element of the skill its friend just cast for skill damage, else the
        /// dealer's main bond (or the hero's own look).</summary>
        private ElementLook HitLook(CombatEvent e)
        {
            string attribution = e.Cause.AttributionId;
            if (!string.IsNullOrEmpty(attribution) && attribution.StartsWith("bond:", StringComparison.Ordinal))
            {
                ElementLook rule = LookFor(attribution.Substring(5));
                if (rule != ElementLook.Gold) return rule;
            }
            if (e.Cause.ItemId == "skill-v3" && lastSkillLook.TryGetValue(e.Cause.SourceUnitId, out ElementLook skill)) return skill;
            return DealerLook(e);
        }

        /// <summary>Damage numbers read in clearly different colours per element (frost pale cyan, lightning yellow, water deep
        /// blue, fire orange...), outlined in the element's own dark ink, with its token beside them.</summary>
        private static Color NumberColour(ElementLook look) => look switch
        {
            ElementLook.Fire => new Color(1f, .14f, .04f), ElementLook.Ice or ElementLook.Crystal => new Color(.72f, .95f, 1f),
            ElementLook.Storm => new Color(.22f, .55f, 1f), ElementLook.Water => new Color(.08f, .28f, .85f),
            ElementLook.Poison => new Color(.55f, 1f, .2f), ElementLook.Bloom => new Color(1f, .5f, .75f),
            ElementLook.Void or ElementLook.Dream => new Color(.78f, .45f, 1f), ElementLook.Light => new Color(1f, .98f, .85f),
            ElementLook.Gold => new Color(1f, .78f, .25f), ElementLook.Wind => new Color(.7f, 1f, .85f),
            ElementLook.Earth or ElementLook.Paper => new Color(.9f, .66f, .4f), ElementLook.Time => new Color(.92f, .86f, .62f),
            ElementLook.Sound => new Color(1f, .6f, 1f), _ => Color.white,
        };
        private static Color NumberInk(ElementLook look) { Color c = NumberColour(look); return new Color(c.r * .18f, c.g * .14f, c.b * .2f); }
        /// <summary>The top colour of an element's digits (the bottom is NumberColour): each element its own pair.</summary>
        private static Color NumberTop(ElementLook look) => look switch
        {
            ElementLook.Fire => new Color(1f, .78f, .2f), ElementLook.Ice or ElementLook.Crystal => Color.white,
            ElementLook.Storm => new Color(.85f, .97f, 1f), ElementLook.Water => new Color(.45f, .75f, 1f),
            ElementLook.Poison => new Color(.9f, 1f, .5f), ElementLook.Bloom => new Color(1f, .88f, .92f),
            ElementLook.Void or ElementLook.Dream => new Color(1f, .75f, 1f), ElementLook.Light => Color.white,
            ElementLook.Gold => new Color(1f, .97f, .65f), ElementLook.Wind => Color.white,
            ElementLook.Earth or ElementLook.Paper => new Color(1f, .88f, .65f), ElementLook.Time => new Color(1f, .98f, .85f),
            ElementLook.Sound => new Color(1f, .88f, 1f), _ => Color.white,
        };

        private Color? ElementColour(CombatEvent e)
        {
            ElementLook look = HitLook(e);
            if (look == ElementLook.None) return null;
            return NumberColour(look);
        }
        private Color? ElementNumberInk(CombatEvent e) { ElementLook look = HitLook(e); return look == ElementLook.None ? (Color?)null : NumberInk(look); }
        private int ElementNumberShape(CombatEvent e) => HitLook(e) switch
        {
            ElementLook.Fire => 1, ElementLook.Storm => 2, ElementLook.Ice or ElementLook.Crystal => 3, ElementLook.Water => 4,
            ElementLook.Poison => 5, ElementLook.Void or ElementLook.Dream => 6, _ => 0,
        };
        private Color? ElementNumberTop(CombatEvent e) { ElementLook look = HitLook(e); return look == ElementLook.None ? (Color?)null : NumberTop(look); }

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
                if (boundSimulation != null) { boundSimulation.RulesV3EntityChanged -= OnEntity; boundSimulation.RulesV3SupportApplied -= OnSupport; boundSimulation.RulesV3HeroApplied -= OnHero; boundSimulation.RulesV3AllyActed -= OnAllyActed; }
                boundSimulation = sim;
                if (sim != null) { sim.RulesV3EntityChanged += OnEntity; sim.RulesV3SupportApplied += OnSupport; sim.RulesV3HeroApplied += OnHero; sim.RulesV3AllyActed += OnAllyActed; }
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
            if (CombatFeedback.ElementTint == (System.Func<CombatEvent, Color?>)ElementColour) { CombatFeedback.ElementTint = null; CombatFeedback.ElementTop = null; CombatFeedback.ElementShape = null; CombatFeedback.ElementInk = null; }
            if (boundSimulation != null) { boundSimulation.RulesV3EntityChanged -= OnEntity; boundSimulation.RulesV3SupportApplied -= OnSupport; boundSimulation.RulesV3HeroApplied -= OnHero; boundSimulation.RulesV3AllyActed -= OnAllyActed; }
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
                if (field == FieldKind.Realm)
                {
                    for (int i = fields.Count - 1; i >= 0; i--) if (fields[i].Kind == field) fields.RemoveAt(i);
                    fields.Add(new Field { Kind = field, Born = Now, Life = life });
                }
                else { Finale(LookFor(e.Rule), e.Position); BlastCrowds(LookFor(e.Rule), e.Position, false); }
                Sound("rules.field", e.Position);
            }
            // a bond step fires: a burst in its element on the subject, capped per frame for big crowds
            if (popsThisFrame < MaxPopsPerFrame && Loud(e.Rule, out float size))
            {
                popsThisFrame++;
                TierMoment(e.Rule, LookFor(e.Rule), e.Position);
                Sound(LookFor(e.Rule), "burst", e.Position);
            }
        }



        public void OnCast(SkillCastRequest c)
        {
            ElementLook look = LookFor(c.SkillId);
            if (look == ElementLook.Gold) look = LookFor(MainBond(c.SourceId));
            if (lastSkillLook.Count > 256) lastSkillLook.Clear();
            lastSkillLook[c.SourceId] = look;
            if (c.Shape == SkillShape.Species)
            {
                // a skill on a cluster or a species is targeted: struck from the caster to its spot
                UnitState from = boundSimulation?.UnitForPresentation(c.SourceId);
                Targeted(look, from != null ? from.Position : c.Position, c.Position);
                return;
            }
            UnitState caster = boundSimulation?.UnitForPresentation(c.SourceId);
            if (caster != null && caster.Alive) { Sound(look, "cast", caster.Position); UnitVisual.Live(caster.Id)?.PlayCast(.55f); }
            Vector2 source = caster != null ? caster.Position : c.Position;
            float warn = Mathf.Max(0, c.WarningTime);
            int strikes = c.Duration > 1.5f ? Mathf.Min(5, Mathf.FloorToInt(c.Duration)) : 1; // a lasting skill bursts again each second
            for (int k = 0; k < strikes; k++)
            {
                float delay = warn + k;
                switch (c.Shape)
                {
                    case SkillShape.Line:
                    {
                        // a lane bursts along its length, start to end
                        float length = (c.End - c.Position).magnitude; int n = Mathf.Clamp(Mathf.RoundToInt(length / 1.6f), 2, 4);
                        for (int i = 0; i < n; i++) Blast(look, Vector2.Lerp(c.Position, c.End, (i + .5f) / n), Mathf.Max(.6f, c.Radius * 1.2f), delay + i * .07f);
                        break;
                    }
                    case SkillShape.Cone:
                    {
                        float reach = Mathf.Max(c.Radius, (c.End - c.Position).magnitude);
                        Blast(look, Vector2.Lerp(c.Position, c.End, .6f), reach * .55f, delay);
                        break;
                    }
                    default: Blast(look, c.Position, Mathf.Max(.5f, c.Radius), delay); break;
                }
            }
        }

        public void OnDanger(EnemyDangerRequest d)
        {
            AreaShape shape = d.Shape == EnemyDangerShape.Line ? AreaShape.Line : d.Shape == EnemyDangerShape.Cone ? AreaShape.Cone : d.Shape == EnemyDangerShape.Ring ? AreaShape.Ring : d.Shape == EnemyDangerShape.Arena ? AreaShape.Arena : AreaShape.Disc;
            Sound("rules.warn", d.Position);
            Add(new Area { Shape = shape, Hostile = true, A = d.Position, B = d.End, R = Mathf.Max(.4f, d.Radius), Born = Now, Warn = Mathf.Max(.1f, d.WarningTime), Life = Mathf.Max(.1f, d.WarningTime) + Mathf.Max(.2f, d.Duration), Look = ElementLook.Fire, Seed = ++seed });
        }

        /// <summary>A chained bond hit (Storm, discharge): a forked lightning stroke from the attacker to the enemy it reached,
        /// a few per frame at most.</summary>
        private int chainAttacker = -1, chainHop; private float chainAt = -10; private Vector2 chainLast;
        public void OnBondDamage(BondDamageRequest d)
        {
            if (d.Attacker == null || d.Target == null || chainsThisFrame >= MaxChainsPerFrame) return;
            string r = d.Rule ?? string.Empty;
            if (r.IndexOf("chain", StringComparison.OrdinalIgnoreCase) < 0 && r.IndexOf("discharge", StringComparison.OrdinalIgnoreCase) < 0 && r.IndexOf("storm", StringComparison.OrdinalIgnoreCase) < 0) return;
            chainsThisFrame++;
            if (r.IndexOf("discharge", StringComparison.OrdinalIgnoreCase) >= 0) { Blast(ElementLook.Storm, d.Target.Position, 1.1f); Sound("rules.chain", d.Target.Position); return; }
            // the chain leaps from the friend to its first enemy, then from each enemy struck to the next, a beat apart
            float now = Now;
            bool sameChain = chainAttacker == d.Attacker.Id && now - chainAt < .05f;
            Vector2 from = sameChain ? chainLast : d.Attacker.Position;
            chainHop = sameChain ? chainHop + 1 : 0;
            Targeted(ElementLook.Storm, from, d.Target.Position, chainHop * .08f);
            chainAttacker = d.Attacker.Id; chainAt = now; chainLast = d.Target.Position;
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
            MorphGeometry geometry = h.Shape == HeroRuleShape.Line ? MorphGeometry.Line : h.Shape == HeroRuleShape.Circle ? MorphGeometry.Disc : MorphGeometry.Ring;
            HeroMechanicStyle style = hero != null ? HeroMechanicStyle.For(hero.Definition.Id) : HeroIdentityVisual.GuestStyle(look);
            float now = Now;
            HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = geometry, Position = h.Position, End = h.End, Direction = (h.End - h.Position).sqrMagnitude > .001f ? (h.End - h.Position).normalized : Vector2.right,
                Radius = Mathf.Min(h.Radius, 6), Width = .4f, Created = now, At = now + .18f, Until = now + .18f + Mathf.Clamp(h.Duration, .5f, 1f), Seed = ++seed, Big = h.Radius >= 2 }, style);
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

        /// <summary>A field moment tints the arena in its element (a boss changing phase flashes it red).</summary>
        private void DrawField(Field f, Vector2 centre, float now)
        {
            float t = Mathf.Clamp01((now - f.Born) / f.Life), a = Mathf.Sin(t * Mathf.PI);
            if (PresentationSettings.ReducedEffects) a *= .5f;
            switch (f.Kind)
            {
                case FieldKind.Blizzard: ink.Vignette(centre, 28, ArsenalInk.Glass.Light, .25f * a, .7f * a); break;
                case FieldKind.Eclipse: ink.Vignette(centre, 28, ArsenalInk.Violet.Ink, .4f * a, .8f * a); break;
                case FieldKind.Gale: ink.Vignette(centre, 28, ArsenalInk.Cotton.Light, .12f * a, .35f * a); break;
                case FieldKind.Riptide: ink.Vignette(centre, 28, ArsenalInk.Water.Body, .14f * a, .4f * a); break;
                case FieldKind.Inferno: ink.Vignette(centre, 28, ArsenalInk.Amber.Glow, .22f * a, .6f * a); break;
                case FieldKind.Lost: ink.Vignette(centre, 28, ArsenalInk.Midnight.Ink, .5f * Mathf.Min(1, t * 3), .95f * Mathf.Min(1, t * 3)); break;
                case FieldKind.Realm: ink.Vignette(centre, 28, BossTelegraph.Danger.Body, .3f * a, .8f * a); break;
                case FieldKind.Pandemic: ink.Vignette(centre, 28, ArsenalInk.Green.Body, .22f * a, .6f * a); break;
            }
        }


        /// <summary>Dusk takes strength from a marked enemy: a violet ribbon from the enemy to the friend that gains it.</summary>
        public void OnStolen(UnitState enemy, UnitState friend, float fraction)
        {
            if (enemy == null || friend == null || chainsThisFrame >= MaxChainsPerFrame) return;
            chainsThisFrame++;
            Targeted(ElementLook.Void, enemy.Position, friend.Position);
        }

        /// <summary>Ember leaves embers where a burning enemy died: a small firepit for their lifetime.</summary>
        public void OnEmbers(Vector2 at, float lifetime) => AddPop(at, ElementLook.Fire, 1, 0);

        /// <summary>A shield breaking on a friend: a loud glass pop.</summary>
        public void OnShieldBroken(UnitState friend) { if (friend != null) { EnemyPop(friend.Position, ElementLook.Crystal, 1.1f); Sound("rules.shieldbreak", friend.Position); } }

        /// <summary>Every friend down at once: the arena darkens before the wave ends.</summary>
        public void OnWaveLost(FriendWaveLostEvent e) => fields.Add(new Field { Kind = FieldKind.Lost, Born = Now, Life = 2.5f });

        public void OnFell(FriendMortalityEvent e) { fallen[e.FriendId] = e; fallenAt[e.FriendId] = Now; Sound("rules.fall", e.Position); }
        // the mortality clock may differ from the combat clock: the countdown runs from when the fall was seen
        private readonly Dictionary<int, float> fallenAt = new Dictionary<int, float>();
        public void OnRose(FriendMortalityEvent e) { Sound("rules.rise", e.Position); fallen.Remove(e.FriendId); AddPop(e.Position, ElementLook.Light, 1.1f, 0); }

        public void OnEntity(RulesV3EntityRequest e)
        {
            if (e.Change == RulesV3EntityChange.Created) { entities[e.Id] = e; entityBorn[e.Id] = Now; }
            else
            {
                entities.Remove(e.Id);
                if (e.Change == RulesV3EntityChange.Collected) Sound("rules.pickup", e.Position);
                if (e.Change == RulesV3EntityChange.Collected) AddPop(e.Position, LookFor(e.Rule), .7f, 0);
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
                if (!a.Landed && a.Hostile && !a.Gloom && age >= a.Warn) { Strike(a); a.Landed = true; areas[i] = a; }
                if (a.Gloom) { DrawGloom(a, age); DangerAreas++; }
                else if (a.Hostile) { DrawDanger(a, age); DangerAreas++; }
                else FriendAreas++;
            }
            for (int i = blastQueue.Count - 1; i >= 0; i--)
                if (now >= blastQueue[i].When) { if (cartoon != null && now - blastQueue[i].When < .5f) cartoon.Play(blastQueue[i].Key, blastQueue[i].At, blastQueue[i].Scale); blastQueue.RemoveAt(i); }
            // summons move in the simulation: follow them every frame
            for (int i = 0; i < RulesV3Tuning.ConstructCapacity; i++)
                if (sim.TryGetRulesV3Entity(i, out RulesV3EntityRequest liveEntity) && entities.ContainsKey(liveEntity.Id)) entities[liveEntity.Id] = liveEntity;
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
                    break;
                case AreaShape.Cone:
                    Vector2 aim = (a.B - a.A).sqrMagnitude > .001f ? (a.B - a.A).normalized : Vector2.right;
                    BossTelegraph.FanGround(ink, a.A, aim, .5f, Mathf.Max(a.R, (a.B - a.A).magnitude), charge, alpha);
                    break;
                case AreaShape.Arena:
                    ink.Vignette(a.A, a.R, BossTelegraph.Danger.Body, .15f * alpha * charge, .45f * alpha * charge);
                    break;
                default:
                    BossTelegraph.Zone(ink, a.A, a.R, a.R * .62f, charge, alpha); // one solid fill, no lines inside
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


        /// <summary>The particle layers of an element where a skill acts, emitted at a rate (per second) so the field keeps
        /// burning: fire is flame tongues rising through orange to red with embers and a little smoke; storm is lightning
        /// flashes and white-blue sparks; frost is glinting stars and drifting crystals; water is swirls and leaping drops;
        /// venom is green fumes and bubbles; and so on. Every layer is a CC0 particle texture tinted by its ramp.</summary>




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
                return; // an enemy blow lands in its solid zone; no cracks are drawn
            }
        }

        /// <summary>An enemy blow lands when its warning ends: the painted enemy burst over the struck ground and stone chips
        /// thrown up, along the lane for a charge.</summary>
        private void Strike(Area a)
        {
            float size = Mathf.Clamp(a.R * .8f, 1, 2.4f);
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
            Blast(look, at, .45f * size, delay);
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
                    // a bond summon acts out what it does: it pops in, stands breathing, steps when it moves with its owner, winds
                    // up, strikes and recovers each time it hits the crowd, and fades out at its end. Its animation sheet when drawn
                    // (SA_<Kind>: appear, idle, idle, move, windup, strike, recovery, vanish), else its painting squashed and stretched
                    float size = SummonSize(e.Rule), age = now - (entityBorn.TryGetValue(e.Id, out float born) ? born : now);
                    float acted = now - (allyActedAt.TryGetValue(e.Id, out float hit) ? hit : -10);
                    bool moving = entityLast.TryGetValue(e.Id, out Vector2 was) && (was - at).sqrMagnitude > .0004f; entityLast[e.Id] = at;
                    int cell = age < .3f ? 0 : e.Duration < .35f ? 7 : acted < .12f ? 4 : acted < .28f ? 5 : acted < .5f ? 6 : moving ? (Mathf.FloorToInt(now * 6) % 2 == 0 ? 3 : 1) : 1 + Mathf.FloorToInt(now * 2.5f + e.Id) % 2;
                    Sprite[] sheet = vfx.Library?.Get("SA_" + e.Rule);
                    Sprite art = sheet != null && sheet.Length >= 8 ? sheet[cell] : SummonArt(e.Rule);
                    float pop = age < .3f ? OutBackPop(age / .3f) : 1, fade = e.Duration < .35f ? Mathf.Clamp01(e.Duration / .35f) : 1;
                    float squash = sheet != null ? 1 : acted < .12f ? .88f : acted < .28f ? 1.14f : 1;
                    float bob = sheet != null ? 0 : Mathf.Sin(now * 3 + e.Id) * .08f;
                    if (art != null) ink.Place(art, at + Vector2.up * (size * .5f * squash + bob), size * pop * (sheet != null ? 1 : 1 / Mathf.Sqrt(squash)), 0, new Color(1, 1, 1, fade), true);
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
        private static float OutBackPop(float x) { x = Mathf.Clamp01(x); const float c = 1.7f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private readonly Dictionary<int, float> entityBorn = new Dictionary<int, float>(), allyActedAt = new Dictionary<int, float>();
        private readonly Dictionary<int, Vector2> entityLast = new Dictionary<int, Vector2>();

        /// <summary>The element a summon fights in.</summary>
        private static ElementLook SummonLook(string kind) => kind switch
        {
            "Phoenix" or "Chef" => ElementLook.Fire, "IceSoldier" => ElementLook.Ice, "Shade" => ElementLook.Void, "LampWisp" => ElementLook.Light,
            "Leviathan" => ElementLook.Water, "WindBeast" => ElementLook.Wind, "Sprout" or "Sapling" or "Treant" or "GreatTree" => ElementLook.Bloom,
            "Colossus" => ElementLook.Earth, "Turret" => ElementLook.Storm, "Spirit" => ElementLook.Dream, "Orchestra" => ElementLook.Sound, _ => LookFor(kind),
        };

        /// <summary>A summon strikes the crowd it guards: a targeted strike in its element from it to the crowd's centre.</summary>
        private void OnAllyActed(int id, Vector2 from, Vector2 crowd, string kind)
        {
            allyActedAt[id] = Now;
            if (allyActedAt.Count > 256) allyActedAt.Clear();
            Targeted(SummonLook(kind), from + Vector2.up * SummonSize(kind) * .5f, crowd, .12f);
        }

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
