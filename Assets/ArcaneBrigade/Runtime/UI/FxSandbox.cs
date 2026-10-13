using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The effects sandbox (owner request 29/10/2026): opened from COLLECTION, it shows the arena floor, a crowd of enemies,
    /// the hero and a few friends, and plays any bond rule, weather, damage number or registered effect on demand. Presentation only:
    /// it never touches save data or starts a run. Later features add entries with <see cref="Register"/>.</summary>
    public sealed class FxSandbox : MonoBehaviour
    {
        public sealed class Entry { public string Group, Name; public int Step; public Action<Vector2> Play; public string Rule; }

        // ---- registry -------------------------------------------------------------------------------------------------------
        private static readonly List<Entry> entries = new List<Entry>();
        private static readonly List<string> groupOrder = new List<string>();
        /// <summary>Adds (or replaces) an entry. <paramref name="play"/> receives the target point (crowd centre, sandbox coordinates);
        /// draw with <see cref="Spawn"/>, <see cref="Draw"/>, <see cref="Number"/>, <see cref="Word"/>, <see cref="Enemies"/>.</summary>
        public static void Register(string group, string name, Action<Vector2> play) => Add(group, name, 0, play, null);
        private static void Add(string group, string name, int step, Action<Vector2> play, string rule)
        {
            entries.RemoveAll(e => e.Group == group && e.Name == name);
            entries.Add(new Entry { Group = group, Name = name, Step = step, Play = play, Rule = rule });
            if (!groupOrder.Contains(group)) groupOrder.Add(group);
        }
        public static IReadOnlyList<Entry> Entries { get { Builtins(); return entries; } }

        // ---- open / close -----------------------------------------------------------------------------------------------------
        public static FxSandbox Current { get; private set; }
        private static int closedFrame = -1;
        /// <summary>True while open (and on the frame it closed, so the menu does not read the same Escape).</summary>
        public static bool IsOpen => Current != null || Time.frameCount == closedFrame;
        public static FxSandbox Open()
        {
            if (Current != null) return Current;
            Builtins();
            var go = new GameObject("FxSandbox"); return go.AddComponent<FxSandbox>();
        }
        public void Close() { if (Current == this) { Current = null; closedFrame = Time.frameCount; } Destroy(gameObject); }

        // ---- the field -------------------------------------------------------------------------------------------------------
        private static readonly Vector2 O = new Vector2(0, -150); // far from the menu; inside FxBatch's mesh bounds
        public static readonly Vector2 Crowd = new Vector2(2.4f, .3f), HeroAt = new Vector2(-5f, -2.2f);
        private sealed class Unit { public SpriteRenderer R; public Vector2 Home, Pos; public float Height, Phase; public bool Enemy; public float HiddenUntil; }
        private readonly List<Unit> units = new List<Unit>();
        private readonly List<UnityEngine.Object> made = new List<UnityEngine.Object>();
        private Transform field;
        private Camera cam;
        private FxDirector director;
        private FxBatch extra;
        private CombatFeedback feedback;
        private readonly Dictionary<string, GameObject> weathers = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, FxDirector> weatherFx = new Dictionary<string, FxDirector>();
        private struct Timed { public float Born, Life; public Action<FxBatch, float, float> Draw; }
        private readonly List<Timed> draws = new List<Timed>();
        private readonly List<KeyValuePair<float, Action>> pending = new List<KeyValuePair<float, Action>>();

        public static float Now => Current != null ? Current.now : 0;
        private float now, speed = 1, lastPlay;
        private bool paused, loop, slow, walk = true;
        private Entry selected;
        private string toast; private float toastAt;
        public int Played { get; private set; }
        public int Missing { get; private set; }

        private Func<CombatEvent, Color?> oldTint, oldTop, oldInk; private Func<CombatEvent, int> oldShape;

        private void Awake()
        {
            Current = this;
            var root = new GameObject("FxSandboxWorld"); root.transform.SetParent(transform, false); made.Add(root);
            field = new GameObject("Field").transform; field.SetParent(root.transform, false); field.position = O;
            var camGo = new GameObject("FxSandboxCamera"); camGo.transform.SetParent(root.transform, false); camGo.transform.position = new Vector3(O.x + .3f, O.y, -10);
            cam = camGo.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.2f; cam.depth = 90;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.05f, .04f, .06f); cam.rect = new Rect(.285f, 0, .715f, 1);
            BuildField();
            director = new FxDirector(field);
            extra = new FxBatch(field, 2143, 2142);
            var fb = new GameObject("FxSandboxNumbers"); fb.transform.SetParent(root.transform, false);
            feedback = fb.AddComponent<CombatFeedback>(); feedback.Configure(null);
            oldTint = CombatFeedback.ElementTint; oldTop = CombatFeedback.ElementTop; oldInk = CombatFeedback.ElementInk; oldShape = CombatFeedback.ElementShape;
            CombatFeedback.ElementTint = e => SandLook(e) is ElementLook l ? RulesV3Ink.LookColour(l) : (Color?)null;
            CombatFeedback.ElementTop = e => SandLook(e) is ElementLook l ? Hidden("NumberTop", l, Color.Lerp(RulesV3Ink.LookColour(l), Color.white, .6f)) : (Color?)null;
            CombatFeedback.ElementInk = e => SandLook(e) is ElementLook l ? Hidden("NumberInk", l, RulesV3Ink.LookColour(l) * .18f) : (Color?)null;
            CombatFeedback.ElementShape = e => SandLook(e) is ElementLook l ? (int)l : 0;
        }

        private void OnDestroy()
        {
            CombatFeedback.ElementTint = oldTint; CombatFeedback.ElementTop = oldTop; CombatFeedback.ElementInk = oldInk; CombatFeedback.ElementShape = oldShape;
            foreach (UnityEngine.Object o in made) if (o != null) Destroy(o);
            if (Current == this) { Current = null; closedFrame = Time.frameCount; }
        }

        private static ElementLook? SandLook(CombatEvent e)
        {
            string a = e.Cause.AttributionId;
            return a != null && a.StartsWith("fxsandbox:", StringComparison.Ordinal) && int.TryParse(a.Substring(10), out int i) ? (ElementLook)i : (ElementLook?)null;
        }
        private static Color Hidden(string method, ElementLook look, Color fallback)
        {
            MethodInfo m = typeof(RulesV3Ink).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            try { return m != null ? (Color)m.Invoke(null, new object[] { look }) : fallback; } catch { return fallback; }
        }

        private Sprite MakeSprite(Texture2D t, float ppu) { if (t == null) return null; Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), ppu); made.Add(s); return s; }
        private void Put(Sprite s, Vector2 at, float height, bool enemy, Color tint)
        {
            if (s == null) return;
            var g = new GameObject(enemy ? "enemy" : "friend"); g.transform.SetParent(field, false); g.transform.localPosition = at;
            var r = g.AddComponent<SpriteRenderer>(); r.sprite = s; r.color = tint; r.sortingOrder = 100 - Mathf.RoundToInt(at.y * 10);
            float scale = height / Mathf.Max(.01f, s.bounds.size.y); g.transform.localScale = new Vector3(enemy ? -scale : scale, scale, 1);
            units.Add(new Unit { R = r, Home = at, Pos = at, Height = height, Enemy = enemy, Phase = units.Count * 1.7f });
        }
        private static float HeightOf(UnitDefinition u) => Mathf.Clamp(u.BodyHeight * Mathf.Max(.5f, u.BodyScale) * 1.25f, .7f, 2f);

        private void BuildField()
        {
            Sprite floor = MakeSprite(Resources.Load<Texture2D>("MienCho/ActOne/Floor_House"), 52);
            if (floor != null) for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
            {
                var g = new GameObject("floor"); g.transform.SetParent(field, false); g.transform.localPosition = new Vector3(x * 24.1f, y * 24.1f, 0);
                var sr = g.AddComponent<SpriteRenderer>(); sr.sprite = floor; sr.color = new Color(.42f, .36f, .4f); sr.sortingOrder = -10;
            }
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            if (catalog == null) return;
            var foes = new List<UnitDefinition>(); foreach (UnitDefinition u in catalog.Enemies) if (u != null && !u.IsBoss && u.Body != null) foes.Add(u);
            var rnd = new System.Random(7);
            for (int i = 0; i < 26 && foes.Count > 0; i++)
            {
                float a = (float)rnd.NextDouble() * 6.28f, d = .4f + Mathf.Sqrt((float)rnd.NextDouble()) * 2.9f;
                UnitDefinition u = foes[i % Mathf.Min(foes.Count, 12)];
                Put(u.Body, Crowd + new Vector2(Mathf.Cos(a) * d, Mathf.Sin(a) * d * .62f), HeightOf(u), true, Color.white);
            }
            UnitDefinition hero = null;
            foreach (UnitDefinition u in catalog.Champions) if (u != null && u.Body != null && (hero == null || u.Id == "LanternKnight")) hero = u;
            var friends = new List<UnitDefinition>(); foreach (UnitDefinition u in catalog.Companions) if (u != null && u.Body != null && !u.SpecialRecruit && friends.Count < 4) friends.Add(u);
            if (hero == null && friends.Count > 0) { hero = friends[0]; friends.RemoveAt(0); }
            if (hero != null) Put(hero.Body, HeroAt, HeightOf(hero) * 1.15f, false, Color.white);
            Vector2[] spots = { new Vector2(-6.4f, -.8f), new Vector2(-3.5f, -.9f), new Vector2(-6.3f, -3.5f), new Vector2(-3.6f, -3.6f) };
            for (int i = 0; i < friends.Count; i++) Put(friends[i].Body, spots[i], HeightOf(friends[i]), false, Color.white);
        }

        // ---- helpers for entries ----------------------------------------------------------------------------------------------
        public static Vector2 Hero => HeroAt;
        public static Vector2 Friend => new Vector2(-3.5f, -.9f);
        /// <summary>The enemies standing nearest a point (centre and body height, sandbox coordinates).</summary>
        public static List<KeyValuePair<Vector2, float>> Enemies(Vector2 near, int count)
        {
            var list = new List<KeyValuePair<Vector2, float>>(); if (Current == null) return list;
            var order = Current.units.FindAll(u => u.Enemy && u.HiddenUntil <= Current.now);
            order.Sort((a, b) => (a.Pos - near).sqrMagnitude.CompareTo((b.Pos - near).sqrMagnitude));
            for (int i = 0; i < order.Count && i < count; i++) list.Add(new KeyValuePair<Vector2, float>(order[i].Pos, order[i].Height));
            return list;
        }
        /// <summary>Hides the enemy nearest a point for a while (an entry that kills it draws its death there).</summary>
        public static void HideEnemy(Vector2 at, float seconds)
        {
            if (Current == null) return; Unit best = null;
            foreach (Unit u in Current.units) if (u.Enemy && (best == null || (u.Pos - at).sqrMagnitude < (best.Pos - at).sqrMagnitude)) best = u;
            if (best != null) best.HiddenUntil = Current.now + seconds;
        }
        /// <summary>Plays a bond moment through the real FxDirector. False when the rule has no effect yet.</summary>
        public static bool Spawn(BondMomentRequest m)
        {
            if (Current == null) return false;
            bool ok = Current.director.Spawn(m, Current.now);
            if (!ok) Current.Toast("No effect registered yet: " + m.Rule);
            return ok;
        }
        public static bool Spawn(string rule, int step, BondMomentKind kind, Vector2 from, Vector2 to, float radius, float delay, float duration = 0)
            => Spawn(new BondMomentRequest("sandbox", rule, step, kind, from, to, radius, delay, duration));
        /// <summary>Draws something for <paramref name="life"/> seconds: draw(batch, t since start, sandbox time).</summary>
        public static void Draw(float life, Action<FxBatch, float, float> draw) { if (Current != null) Current.draws.Add(new Timed { Born = Current.now, Life = life, Draw = draw }); }
        /// <summary>Runs an action later (sandbox seconds, so it follows slow motion and pause).</summary>
        public static void Later(float seconds, Action act) { if (Current != null) Current.pending.Add(new KeyValuePair<float, Action>(Current.now + seconds, act)); }
        /// <summary>A real damage number (CombatFeedback) in an element's colours.</summary>
        public static void Number(Vector2 at, float amount, ElementLook look, bool critical = false)
        {
            if (Current == null) return;
            Current.feedback.Play(new CombatEvent(CombatEventKind.Hit, at + O, Faction.Enemy, amount, Vector2.up, null, critical, DamageElement.Physical, 900 + Current.Played,
                null, new CombatCause(1, 0, "", 0, "fxsandbox:" + (int)look)));
        }
        public static void Word(Vector2 at, string text, Color colour) { if (Current != null) Current.feedback.Label(at + O, text, colour, .62f, 1.3f); }
        private void Toast(string text) { toast = text; toastAt = Time.unscaledTime; }

        // ---- built-in entries ----------------------------------------------------------------------------------------------------
        // bond|step|rule|kind (S strike, L line, Z zone, W weather, T trait event, D damage-only, N not an FxDirector effect)|radius
        // (trait: value)|delay|duration|count|stagger|from (h hero, f friend, o one spot, x across, X horizontal, s friend->hero, r back)|half length
        private const string Table = @"Storm|2|storm-chain|N
Storm|6|storm-thunderbolt|S|2.5|.35
Storm|8|storm-thunderstorm|W
Storm|8|storm-thunderstorm|S|1.4|.2
Windborne|2|piercing-volley|T|1
Windborne|4|piercing-volley|T|3
Windborne|6|wind-tornado|Z|1.8|0|5
Windborne|8|wind-gale|W
Windborne|8|wind-tornado|Z|1.8|0|5|3|0
Windborne|8|gale|T|2|0|0|1|0|h
Ember|2|burn-burst|T|2
Ember|4|inferno-burst|T|4
Ember|6|ember-meteor|S|1.6|.5|0|3|.15
Ember|8|phoenix-dive|L|.9|.3|0|1|0|x|5
Frost|2|freeze|T|1.5
Frost|4|frozen-hit|T|.25
Frost|4|shatter|T|2
Frost|6|frost-ice-spikes|S|2.5|.4
Frost|8|frost-blizzard|W
Frost|8|frost-blizzard-hail|S|2|.6
Tide|2|tide-splash|T|2
Tide|4|tide-splash|T|4
Tide|6|tide-whirlpool|Z|2.5|0|3
Tide|8|tide-rain|W
Tide|8|tide-leviathan|L|1.5|.6|0|1|0|X|10
Tide|8|riptide|T|1|0|0|1|0|h
Garden|2|garden-vine|S|.6|0
Garden|4|root-hit|T|1.5
Garden|6|treant-slam|S|3|.45
Garden|8|root-eruption|S|1.5|.35|0|3|.12
Venom|4|rot|T|1
Venom|6|venom-flask|S|2.2|.5|0|1|0|h
Venom|6|venom-cloud|Z|2.2|0|4
Venom|8|plague-mist|W
Venom|8|pandemic|T|1|0|0|1|0|h
Starlight|2|falling-star|D
Starlight|6|star-shower|S|1|.4|0|8|.12
Starlight|8|starry-night|W
Starlight|8|star-fall|S|1|.35
Starlight|8|star-comet|S|4|.9
Dusk|2|execute|T|1
Dusk|4|shade-execute-growth|T|1
Dusk|4|dusk-mark|T|1
Dusk|6|dusk-pit|Z|2.5|0|1.5
Dusk|8|eclipse|W
Carnival|2|carnival-burst|D
Carnival|6|carnival-confetti|S|1.2|.5|0|6|.15|h
Carnival|8|grand-finale|W|0|0|1.5
Carnival|8|carnival-confetti|S|1.2|.5|0|4|.12|h
Court|2|court-seal|S|.9|.25
Court|6|court-decree|S|2.5|1
Court|8|court-fanfare|W|0|0|1.5
Court|8|court-royal-seal|S|1.2|.5|0|4|.12|h
Archive|2|species-recorded|T|1
Archive|4|record-page|S|1.5|0|3
Archive|6|archive-giant-book|S|2.5|.8
Archive|8|archive-pages|S|1|.3|0|1|0|h
Lantern|2|lantern-stillness|T|1|0|0|1|0|h
Lantern|4|wick|T|2.4
Lantern|8|lighthouse|W
Stone|2|stone-shove|T|1
Stone|4|stone-spike|S|1.4|.2
Stone|6|stone-boulder|L|1.2|.5|0|1|0|x|7
Stone|8|colossus-stomp|S|2.5|.3
Clockwork|2|clock-gears|S|.8|.3|0|3|.08|f
Clockwork|6|time-stop|S|1.5|0|2
Clockwork|8|clock-pendulum|L|1.5|.5|0|1|0|X|10
Hearth|2|warmth|T|1|0|0|1|0|h
Hearth|4|feast|T|1|0|0|1|0|h
Hearth|6|hearth-hot-pot|Z|2.5|0|3
Hearth|8|chef-pie|S|1.3|.45|0|1|0|h
Hearth|8|hearth-feast|W|0|0|5
Bulwark|4|guard-shockwave|T|2
Bulwark|4|guard-line|T|2
Bulwark|8|bulwark-fortress|W|0|0|5
Bulwark|8|bulwark-shield-bolt|S|.8|.25|0|1|0|h
Spirit|6|spirit-swarm|S|1.5|.4|0|3|.15|h
Spirit|8|spirit-reaper|S|3|.5
Melody|2|melody-notes|S|.7|.3|0|5|.07|f
Melody|2|beat|T|1|0|0|1|0|f
Melody|4|melody-sound-wave|S|2|.5
Melody|6|melody-war-drum|S|2.5|.4|0|3|.4|o
Melody|8|melody-note-rain|S|1|.3|0|3|.1
Melody|8|orchestra-damage-note|T|1
Gravity|2|gravity-pull|T|1
Gravity|4|gravity-clump|T|2
Gravity|6|singularity-collapse|T|3
Gravity|8|gravity-heavy|W
Gravity|8|gravity-orb|S|3.5|1
Minor: Rain Script|2|rain-letters|S|1.6|.5
Minor: Rain Script|4|rain-letters|S|1.6|.5|0|3|.15
Minor: Root|2|root-still-roots|S|1.5|.35
Minor: Root|4|root-still-roots|S|2.25|.35
Minor: Cloud|2|cloud-drift|Z|2|0|4
Minor: Monolith|2|stone-monolith-stomp|S|1.8|.25
Minor: Monolith|4|stone-monolith-pull|Z|1.8|0|1
Minor: Hats|2|hat-disc|L|.6|.15|0|1|0|f
Minor: Hats|6|hat-disc|L|.6|.55|0|1|0|r
Minor: Clover|2|lucky-clover|S|1.2|.35|0|1|0|h
Minor: Clover|4|lucky-clover|S|1.2|.35|0|3|.12|h
Minor: Exhibit Hall|2|exhibit-statue|Z|1.8|0|3
Minor: Spiky Shell|2|thorn-spikes|L|.5|.1|0|1|0|f
Minor: Spiky Shell|4|thorn-spikes|L|.5|.1|0|2|.2|f
Minor: Mend|2|vine-mend|L|.4|0|0|1|0|s
Minor: Mend|4|vine-mend|L|.6|.35|0|1|0|h
Minor: Bastion|2|bastion-shield-shard|S|.8|.3|0|3|.06|f
Minor: Sweet Tooth|2|sugar-rush|T|1|0|0|1|0|h
Minor: Horns|2|horn-pile-up|T|1
Minor: Horns|6|horns-vulnerability|T|1
Minor: Masks|2|mask-double|T|1
Minor: Runes|2|rune|D
Minor: Demolitionist|2|demolitionist-stun|T|1
Minor: Big Top|2|carnival-nothing|T|1";

        public static readonly string[] WeatherRules = { "storm-thunderstorm", "wind-gale", "frost-blizzard", "tide-rain", "plague-mist", "starry-night", "eclipse", "lighthouse", "gravity-heavy", "hearth-feast", "bulwark-fortress" };
        public const string WeatherGroup = "Weather";
        private static readonly ElementLook[] Looks = { ElementLook.Fire, ElementLook.Ice, ElementLook.Water, ElementLook.Storm, ElementLook.Void, ElementLook.Light, ElementLook.Wind, ElementLook.Poison, ElementLook.Earth, ElementLook.Bloom, ElementLook.Crystal, ElementLook.Gold, ElementLook.Paper, ElementLook.Sound, ElementLook.Time, ElementLook.Dream };
        private static bool built;

        private static float F(string[] c, int i, float fallback) => c.Length > i && float.TryParse(c[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : fallback;
        private static Vector2 SpotAround(Vector2 at, int i, int n) => n <= 1 ? at : at + new Vector2(Mathf.Cos(i * 2.4f + .5f), Mathf.Sin(i * 2.4f + .5f) * .62f) * (1.2f + (i % 3) * .55f);

        private static void Builtins()
        {
            if (built) return; built = true;
            foreach (string line in Table.Split('\n'))
            {
                string[] c = line.Trim().Split('|'); if (c.Length < 4) continue;
                string group = c[0], rule = c[2], kind = c[3], from = c.Length > 10 ? c[10] : "";
                int step = (int)F(c, 1, 2), count = Mathf.Max(1, (int)F(c, 8, 1));
                float r = F(c, 4, 1), delay = F(c, 5, 0), dur = F(c, 6, 0), stagger = F(c, 9, 0), len = F(c, 11, 5);
                Action<Vector2> play = target =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 at = from == "o" ? target : SpotAround(target, i, count), src = from == "h" ? Hero : Friend;
                        float d = delay + i * stagger;
                        switch (kind)
                        {
                            case "S": Spawn(rule, step, BondMomentKind.Strike, from == "h" || from == "f" ? src : at, at, r, d, dur); break;
                            case "T": { Vector2 who = from == "h" ? Hero : from == "f" ? Friend : at; Spawn(rule, step, BondMomentKind.Strike, who, who, 1 + .25f * Mathf.Clamp(r, 0, 8), 0, 0); break; }
                            case "D": Spawn(rule, step, BondMomentKind.Strike, Friend, at, 1.2f, 0, 0); break;
                            case "W": Spawn(rule, step, BondMomentKind.Weather, Hero, Hero, 0, 0, dur > 0 ? dur : 12); break;
                            case "Z":
                                if (Spawn(rule, step, BondMomentKind.Zone, at, at, r, 0, dur) && FxDirector.Has(rule + "-burst"))
                                { Vector2 z = at; Later(dur, () => Spawn(rule + "-burst", step, BondMomentKind.Strike, z, z, r, 0, 0)); }
                                break;
                            case "L":
                            {
                                Vector2 across = (at - Hero).sqrMagnitude > .01f ? (at - Hero).normalized : Vector2.right, a, b;
                                if (from == "x") { a = at - across * len; b = at + across * len; }
                                else if (from == "X") { a = at - Vector2.right * len; b = at + Vector2.right * len; }
                                else if (from == "s") { a = Friend; b = Hero; }
                                else if (from == "h") { a = Hero; b = at; }
                                else if (from == "r") { a = at; b = Friend; }
                                else { a = Friend; b = at + (at - Friend).normalized * .5f; }
                                Spawn(rule, step, BondMomentKind.Line, a, b, r, d, 0); break;
                            }
                            default: Current?.Toast(rule + " is the chain lightning drawn by RulesV3Ink in a real fight (not an FxDirector effect)"); return;
                        }
                    }
                };
                string name = step + " · " + rule + (kind == "W" ? " (weather)" : kind == "Z" ? " (zone)" : kind == "L" ? " (line)" : kind == "T" ? " (trait)" : "") + (count > 1 ? " x" + count : "");
                Add(group, name, step, play, kind == "N" ? null : rule);
            }
            groupOrder.Add(WeatherGroup);
            // damage numbers by element and status words
            Add("Damage numbers", "All elements", 0, at => { var foes = Enemies(at, Looks.Length); for (int i = 0; i < foes.Count; i++) Number(foes[i].Key + Vector2.up * foes[i].Value * .3f, 18 + i * 13, Looks[i], i % 5 == 0); }, null);
            foreach (ElementLook look in Looks)
            {
                ElementLook l = look;
                Add("Damage numbers", l.ToString(), 0, at =>
                {
                    var foes = Enemies(at, 4); float[] amounts = { 12, 47, 133, 386 };
                    for (int i = 0; i < foes.Count; i++) { int k = i; Vector2 p = foes[i].Key + Vector2.up * foes[i].Value * .3f; Later(i * .12f, () => Number(p, amounts[k], l, k == 3)); }
                }, null);
            }
            (string word, Color colour)[] words = { ("FROZEN", new Color(.66f, .88f, 1f)), ("STUNNED", new Color(1f, .9f, .4f)), ("BURNING", new Color(1f, .55f, .25f)), ("SOAKED", new Color(.4f, .7f, 1f)),
                ("POISONED", new Color(.6f, 1f, .3f)), ("ROOTED", new Color(.55f, .85f, .4f)), ("MARKED", new Color(.8f, .5f, 1f)), ("SLOWED", new Color(.7f, .9f, 1f)), ("Dodge", new Color(.55f, 1f, .9f)) };
            Add("Status words", "All words", 0, at => { var foes = Enemies(at, words.Length); for (int i = 0; i < foes.Count; i++) Word(foes[i].Key + Vector2.up * foes[i].Value * .6f, words[i].word, words[i].colour); }, null);
            // element reactions (ElementReactions): every combo, an element's mark on enemies, an element's death
            var seenKinds = new HashSet<ElementCombos.Kind>();
            foreach (ElementLook a in Looks) foreach (ElementLook b in Looks)
            {
                if (!ElementCombos.For(a, b, out ElementCombos.Kind kind, out ElementLook other) || !seenKinds.Add(kind)) continue;
                ElementCombos.Kind k = kind; ElementLook o = other;
                Add("Element combos", k + " (" + a + "+" + b + ")", 0, at =>
                {
                    var foe = Enemies(at, 1); if (foe.Count == 0) return; Vector2 feet = foe[0].Key + Vector2.down * foe[0].Value * .46f; int seed = UnityEngine.Random.Range(1, 9999);
                    Draw(ElementCombos.Life, (bt, t, now) => ElementCombos.Draw(bt, k, o, feet, 1, t, now, seed));
                }, null);
            }
            foreach (ElementLook look in Looks)
            {
                ElementLook l = look;
                Add("Enemy states", l.ToString(), 0, at =>
                {
                    foreach (var foe in Enemies(at, 3)) { Vector2 c = foe.Key; float h = foe.Value; int seed = UnityEngine.Random.Range(1, 9999);
                        Draw(4f, (bt, t, now) => EnemyElementState.Draw(bt, l, c, h, Mathf.Clamp01((4f - t) / ElementReactions.MarkFade), t, now, seed)); }
                }, null);
                Add("Element deaths", l.ToString(), 0, at =>
                {
                    var foe = Enemies(at, 1); if (foe.Count == 0) return; Vector2 c = foe[0].Key; float h = foe[0].Value; int seed = UnityEngine.Random.Range(1, 9999);
                    HideEnemy(c, ElementDeaths.Life + 1f); Vector2 dir = (c - Hero).normalized;
                    Draw(ElementDeaths.Life, (bt, t, now) => ElementDeaths.Draw(bt, l, c, h, dir, t, now, seed));
                }, null);
            }
        }

        /// <summary>Rules with an effect that no entry plays yet (a new family's rule): listed so nothing registered is hidden.</summary>
        private void AddUnlisted()
        {
            var listed = new HashSet<string>(); foreach (Entry e in entries) if (e.Rule != null) listed.Add(e.Rule);
            foreach (string rule in FxDirector.Rules)
                if (!listed.Contains(rule) && !rule.EndsWith("-burst"))
                { string r = rule; Add("Other rules", r, 0, at => Spawn(r, 6, BondMomentKind.Strike, at, at, 1.6f, .3f, 3), r); }
        }

        private void Start() { AddUnlisted(); Missing = entries.FindAll(e => e.Rule != null && !FxDirector.Has(e.Rule)).Count; }

        // ---- playing -----------------------------------------------------------------------------------------------------------
        public void Play(Entry e)
        {
            if (e == null) return;
            selected = e; lastPlay = now; Played++;
            if (e.Rule != null && !FxDirector.Has(e.Rule)) { Toast("No effect registered yet: " + e.Rule); return; }
            e.Play(Crowd);
        }
        public Entry Find(string group, string rulePart)
        {
            Builtins(); return entries.Find(e => e.Group == group && (e.Name == rulePart || e.Name.Contains(rulePart)));
        }
        /// <summary>Plays a bond's rules step by step (2, 4, 6, 8), three seconds apart.</summary>
        public void PlayWholeBond(string group)
        {
            var list = entries.FindAll(e => e.Group == group && e.Step > 0); list.Sort((a, b) => a.Step.CompareTo(b.Step));
            float at = 0; int lastStep = -1;
            foreach (Entry e in list) { if (lastStep >= 0) at += e.Step != lastStep ? 3.2f : 1.4f; lastStep = e.Step; Entry x = e; Later(at, () => { Play(x); Toast(group + " step " + x.Step + ": " + x.Name); }); }
        }
        public void ToggleWeather(string rule)
        {
            if (weathers.TryGetValue(rule, out GameObject g)) { Destroy(g); weathers.Remove(rule); weatherFx.Remove(rule); return; }
            if (!FxDirector.Has(rule)) { Toast("No effect registered yet: " + rule); return; }
            g = new GameObject("weather " + rule); g.transform.SetParent(field, false);
            var fx = new FxDirector(g.transform); fx.Spawn(new BondMomentRequest("sandbox", rule, 8, BondMomentKind.Weather, Hero, Hero, 0, 0, 999), now);
            weathers[rule] = g; weatherFx[rule] = fx;
        }
        public bool WeatherOn(string rule) => weathers.ContainsKey(rule);
        public void SetSlow(bool on) { slow = on; }
        public void SetPaused(bool on) { paused = on; }
        public void ClearAll()
        {
            director.Clear(); draws.Clear(); pending.Clear(); feedback.Clear();
            foreach (GameObject g in weathers.Values) Destroy(g); weathers.Clear(); weatherFx.Clear();
            foreach (Unit u in units) u.HiddenUntil = 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { Close(); return; }
            if (Input.GetKeyDown(KeyCode.Space)) Play(selected);
            if (Input.GetKeyDown(KeyCode.P)) paused = !paused;
            speed = paused ? 0 : slow ? .25f : 1;
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f) * speed;
            now += dt;
            for (int i = 0; i < pending.Count; i++) if (pending[i].Key <= now) { Action a = pending[i].Value; pending.RemoveAt(i--); a(); }
            if (loop && selected != null && director.Live == 0 && draws.Count == 0 && pending.Count == 0 && now - lastPlay > .6f) Play(selected);
            if (dt > 0) feedback.Advance(dt);
            foreach (Unit u in units)
            {
                if (u.Enemy && walk) { float p = u.Phase + now * .55f; u.Pos = u.Home + new Vector2(Mathf.Cos(p), Mathf.Sin(p) * .6f) * .35f; }
                u.R.transform.localPosition = u.Pos; u.R.sortingOrder = 100 - Mathf.RoundToInt(u.Pos.y * 10);
                u.R.enabled = u.HiddenUntil <= now;
            }
        }
        private void LateUpdate()
        {
            director.Draw(now);
            foreach (FxDirector w in weatherFx.Values) w.Draw(now);
            extra.Begin();
            for (int i = draws.Count - 1; i >= 0; i--) { float t = now - draws[i].Born; if (t > draws[i].Life) { draws.RemoveAt(i); continue; } draws[i].Draw(extra, t, now); }
            extra.End();
        }

        // ---- panel ---------------------------------------------------------------------------------------------------------------
        private Vector2 scroll; private readonly HashSet<string> open = new HashSet<string>();
        private GUIStyle small, head, btn, on;
        private void OnGUI()
        {
            GUISkin previousSkin = GUI.skin; GUI.skin = ArcadeSkin.Skin;
            if (small == null)
            {
                small = new GUIStyle(GUI.skin.button) { fontSize = 13, alignment = TextAnchor.MiddleLeft, wordWrap = false };
                head = new GUIStyle(GUI.skin.button) { fontSize = 15, alignment = TextAnchor.MiddleLeft, font = ArcadeTypography.Heading };
                btn = new GUIStyle(GUI.skin.button) { fontSize = 14, font = ArcadeTypography.Heading };
                on = new GUIStyle(btn); on.normal.textColor = on.hover.textColor = new Color(1f, .85f, .3f);
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            GUI.DrawTexture(new Rect(-400, -50, 765, 820), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(.04f, .035f, .06f, .96f), 0, 0);
            GUI.Label(new Rect(12, 6, 250, 30), "FX SANDBOX", ArcadeTypography.Style(20, true));
            if (GUI.Button(new Rect(262, 6, 92, 30), "BACK", btn)) { Close(); }
            if (GUI.Button(new Rect(10, 40, 170, 30), "LIVE FIGHT", liveTab ? on : btn)) liveTab = true;
            if (GUI.Button(new Rect(184, 40, 170, 30), "PREVIEW", liveTab ? btn : on)) liveTab = false;
            if (liveTab) LivePanel(); else PreviewPanel();
            if (toast != null && Time.unscaledTime - toastAt < 3)
                GUI.Label(new Rect(380, 680, 880, 30), toast, ArcadeTypography.Style(16));
            GUI.Label(new Rect(380, 8, 880, 24), liveTab ? "LIVE FIGHT: a real wave-" + FxSandboxLive.Wave + " fight, only the chosen bond firing · Esc: back"
                : (paused ? "PAUSED · " : slow ? "SLOW 0.25x · " : "") + "Space: play again · P: pause · Esc: back", ArcadeTypography.Style(13));
            GUI.matrix = previous; GUI.skin = previousSkin;
        }

        private void PreviewPanel()
        {
            if (GUI.Button(new Rect(10, 76, 84, 30), "PLAY", btn)) Play(selected);
            if (GUI.Button(new Rect(98, 76, 84, 30), "LOOP", loop ? on : btn)) loop = !loop;
            if (GUI.Button(new Rect(186, 76, 84, 30), "0.25x", slow ? on : btn)) slow = !slow;
            if (GUI.Button(new Rect(274, 76, 80, 30), "PAUSE", paused ? on : btn)) paused = !paused;
            if (GUI.Button(new Rect(10, 110, 84, 30), "CLEAR", btn)) ClearAll();
            if (GUI.Button(new Rect(98, 110, 84, 30), "WALK", walk ? on : btn)) walk = !walk;
            GUI.enabled = selected != null && selected.Step > 0;
            if (GUI.Button(new Rect(186, 110, 168, 30), "WHOLE BOND 2→8", btn)) PlayWholeBond(selected.Group);
            GUI.enabled = true;
            GUI.Label(new Rect(12, 142, 345, 22), selected != null ? selected.Group + " · " + selected.Name : "Pick an effect below", ArcadeTypography.Style(13));
            float y = 0, rowH = 26; var rows = new List<(string group, Entry e)>();
            foreach (string g in groupOrder) { rows.Add((g, null)); if (!open.Contains(g)) continue; if (g == WeatherGroup) foreach (string w in WeatherRules) rows.Add((g, new Entry { Group = g, Name = w, Rule = w })); else foreach (Entry e in entries) if (e.Group == g) rows.Add((g, e)); }
            Rect view = new Rect(6, 168, 352, 546);
            scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, 330, rows.Count * rowH + 4));
            foreach (var row in rows)
            {
                Rect r = new Rect(0, y, 330, rowH - 2); y += rowH;
                if (r.yMax < scroll.y || r.y > scroll.y + view.height) continue;
                if (row.e == null) { if (GUI.Button(r, (open.Contains(row.group) ? "▾ " : "▸ ") + row.group, head)) { if (!open.Remove(row.group)) open.Add(row.group); } continue; }
                bool has = row.e.Rule == null || FxDirector.Has(row.e.Rule);
                GUI.color = has ? Color.white : new Color(1, 1, 1, .45f);
                if (row.group == WeatherGroup) { if (GUI.Button(new Rect(14, r.y, 316, r.height), (WeatherOn(row.e.Rule) ? "■ " : "□ ") + row.e.Rule, WeatherOn(row.e.Rule) ? on : small)) ToggleWeather(row.e.Rule); }
                else if (GUI.Button(new Rect(14, r.y, 316, r.height), row.e.Name + (has ? "" : "  (no effect yet)"), selected == row.e ? on : small)) Play(row.e);
                GUI.color = Color.white;
            }
            GUI.EndScrollView();
        }

        // ---- LIVE FIGHT tab: picks a bond, a step and a crowd, then runs the real fight (FxSandboxLive) -------------------------------
        private Vector2 liveScroll;
        private void LivePanel()
        {
            string house = FxSandboxLive.House; int step = FxSandboxLive.Fit(house, FxSandboxLive.Step);
            GUI.Label(new Rect(12, 74, 345, 20), "BOND (friends that carry it)", ArcadeTypography.Style(13));
            string[] houses = FxSandboxLive.Houses;
            Rect view = new Rect(6, 96, 352, 360);
            liveScroll = GUI.BeginScrollView(view, liveScroll, new Rect(0, 0, 330, houses.Length * 26 + 4));
            for (int i = 0; i < houses.Length; i++)
            {
                string h = houses[i]; bool any = false; foreach (int s in FxSandboxLive.Steps(h)) any |= FxSandboxLive.CanRun(h, s);
                GUI.enabled = any;
                if (GUI.Button(new Rect(14, i * 26, 316, 24), h + "   (" + FxSandboxLive.Friends(h) + ")", h == house ? on : small)) { FxSandboxLive.House = h; FxSandboxLive.Step = FxSandboxLive.Fit(h, step); }
            }
            GUI.enabled = true; GUI.EndScrollView();
            float y = 462; int[] steps = { 2, 4, 6, 8 };
            GUI.Label(new Rect(12, y, 345, 20), "STEP", ArcadeTypography.Style(13)); y += 22;
            for (int i = 0; i < 4; i++)
            {
                GUI.enabled = FxSandboxLive.CanRun(house, steps[i]);
                if (GUI.Button(new Rect(10 + i * 86, y, 82, 30), steps[i] + " · " + FxSandboxLive.Team(house, steps[i]) + "f", step == steps[i] ? on : btn)) FxSandboxLive.Step = steps[i];
            }
            GUI.enabled = true; y += 36;
            GUI.Label(new Rect(12, y, 345, 20), "ENEMIES", ArcadeTypography.Style(13)); y += 22;
            for (int i = 0; i < 3; i++) if (GUI.Button(new Rect(10 + i * 115, y, 111, 30), FxSandboxLive.CrowdNames[i], FxSandboxLive.Crowd == i ? on : btn)) FxSandboxLive.Crowd = i;
            y += 36;
            GUI.Label(new Rect(12, y, 345, 20), "SPEED", ArcadeTypography.Style(13)); y += 22;
            if (GUI.Button(new Rect(10, y, 170, 30), "1x", !FxSandboxLive.Slow ? on : btn)) FxSandboxLive.Slow = false;
            if (GUI.Button(new Rect(184, y, 170, 30), "0.25x", FxSandboxLive.Slow ? on : btn)) FxSandboxLive.Slow = true;
            y += 38;
            if (GUI.Button(new Rect(10, y, 344, 40), "START FIGHT · " + house.ToUpperInvariant() + " " + step, on)) StartLive(house, step);
            y += 44;
            GUI.Label(new Rect(12, y, 345, 40), FxSandboxLive.LastFailure ?? "Pause, restart and the same picks are on the fight's own panel.", ArcadeTypography.Style(11));
        }
        /// <summary>Starts the LIVE FIGHT for a bond at a step (the sandbox closes; leaving the fight opens it again).</summary>
        public FxSandboxLive StartLive(string house, int step) => FxSandboxLive.Begin(house, step);
        private bool liveTab = true;
        public bool LiveTab => liveTab;
        public void ShowLiveTab() { liveTab = true; }
        public void ShowPreviewTab() { liveTab = false; }

        /// <summary>Opens a group in the list (the capture uses it to show the list).</summary>
        public void Expand(string group) { open.Add(group); }
    }
}
