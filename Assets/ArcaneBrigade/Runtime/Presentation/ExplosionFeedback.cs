using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Explosions you can feel: a warm flash, an inked shock ring at the real blast radius, paper-and-ink chips and
    /// a settling smudge of ink. Plays on Detonation events (fuse and echo blasts, hero area strikes), boss landings and
    /// boss deaths (and the closing blast of every boss finale), and every ordinary enemy kill as a short ink POP sized by
    /// the body, the overkill and the crit. Pops are pooled and rate-limited so hundreds of kills a second stay cheap.
    /// Presentation only: sizes come from the event, nothing here touches the simulation.</summary>
    public sealed class ExplosionFeedback : MonoBehaviour
    {
        private enum Kind { Blast, Burst, Smudge, Shock, Pop }
        private const float MaxBlastRadius = 4.5f;
        private sealed class Blast
        {
            public Kind Kind;
            public Vector2 Position;
            public float Radius, Age = 10, Duration;
            public Color Color;
            public bool Important, Hostile, Big;
            public float Spin;
            public SpriteRenderer Flash, Ring, Rim, Core, Bloom;
            public readonly SpriteRenderer[] Chips = new SpriteRenderer[12];
            public readonly Vector2[] ChipVelocity = new Vector2[12];
            public readonly float[] ChipSpin = new float[12];
            public readonly SpriteRenderer[] Puffs = new SpriteRenderer[3];
        }
        private readonly List<Blast> pool = new List<Blast>();
        private Sprite glow, ring, chip, puff, white, spark;
        private Sprite[] deathBlast = System.Array.Empty<Sprite>();
        /// <summary>The one big blast a large body goes up in (requirement 09/11/2026: one big explosion, never a cluster of small ones).</summary>
        private Sprite[] bigBlast = System.Array.Empty<Sprite>();
        private Material bloom;
        private PaintedAnimationLibrary painted;
        public int ActiveCount { get; private set; }
        public const int Capacity = 120;
        /// <summary>Most enemy pops started per tenth of a second (the squashing body still shows for the rest).</summary>
        public const int PopsPerWindow = 26, ReducedPopsPerWindow = 9;
        private float popWindow;
        private int popsInWindow;
        public int PopsShown { get; private set; }
        public static ExplosionFeedback Active { get; private set; }
        private float skidWindow;
        private int skidsInWindow;

        // ---- what a death leaves behind (all code-drawn and pooled): sparks, flame tongues, smoke, dust, vapour, a burn on the
        // floor, an elite's pillar of light; the chain of rim bursts after a big blast; the shove it gives the bodies round it ----
        private enum MoteKind { Ember, Flame, Spark, Smoke, Dust, Vapor, Glow, Pillar }
        private sealed class Mote
        {
            public SpriteRenderer View; public MoteKind Kind;
            public Vector2 Position, Velocity; public float Height, Lift, Age = 1, Life = 1, Size, Width, Phase;
            public Color Hot, Cool;
        }
        private readonly List<Mote> motes = new List<Mote>();
        /// <summary>Most death motes alive at once; a big blast spends about forty, a small one four to six.</summary>
        public const int MoteCapacity = 520;
        private int moteCursor;
        private sealed class Scorch { public SpriteRenderer View; public Vector2 Position; public float Age = 1, Life = 1, Size, Alpha; }
        private readonly List<Scorch> scorches = new List<Scorch>();
        public const int ScorchCapacity = 48;
        private int scorchCursor;
        private readonly List<(Vector2 at, float radius, float strength, float born)> shoves = new List<(Vector2, float, float, float)>();
        private float clock;
        private Sprite burn;
        /// <summary>Most big death blasts started per fifth of a second (the rest go up as small ones).</summary>
        public const int BigPerWindow = 4, ReducedBigPerWindow = 2;
        public const float PopRadius = 0.5f, BigPopRadius = 0.9f, MaxPopRadius = 0.7f, MaxBigPopRadius = 1.05f;
        /// <summary>Share of a BigBlast frame's width its painting spans at the peak (the canvas keeps a margin).</summary>
        public const float BigBlastInk = 0.85f;
        private float bigWindow;
        private int bigInWindow;
        public int BigPopsShown { get; private set; }
        /// <summary>Kills in an unbroken run (no gap over a second): each one lands a little bigger and brighter.</summary>
        public int Streak { get; private set; }
        private float streakAt = -10;
        public const float ShoveSeconds = 0.34f;
        /// <summary>Nothing of a death outlives its blast by long (requirement 09/11/2026: when the blast is over nothing is left):
        /// every mote, burn and pillar is gone this long after it starts.</summary>
        public const float AftermathSeconds = 0.6f;
        /// <summary>How an ordinary death looks (review 09/11/2026): the golden blast, or the body flashing white and breaking into
        /// pieces of its own painting (DeathFeedback) with no fireball, or with a small spark of fire.</summary>
        public enum DeathLook { Blast, Shatter, ShatterSpark }
        public static DeathLook Look = DeathLook.Blast;
        private void OnDestroy() { if (Active == this) Active = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetActive() => Active = null;
        /// <summary>A puff of floor dust where a knocked-back body skids.</summary>
        public void Skid(Vector2 position, Vector2 direction)
        {
            if (PresentationSettings.ReducedEffects) return;
            if (Time.unscaledTime > skidWindow) { skidWindow = Time.unscaledTime + 0.1f; skidsInWindow = 0; }
            if (++skidsInWindow > 8) return;
            Spawn(Kind.Smudge, position - direction * 0.05f, 0.32f, new Color(0.66f, 0.58f, 0.47f), false);
        }

        private void Awake()
        {
            Active = this;
            glow = Make("ExplosionGlow", 128, (d, a) => Mathf.Pow(Mathf.Clamp01(1 - d), 1.6f));
            // the shock ring: a soft painted band with a darker inked outer edge
            ring = Make("ExplosionRing", 192, (d, a) => { float band = Mathf.Clamp01(1 - Mathf.Abs(d - 0.82f) / 0.16f); float ink = Mathf.Clamp01(1 - Mathf.Abs(d - 0.96f) / 0.04f); return Mathf.Max(band * band * 0.9f, ink); });
            chip = Make("ExplosionChip", 32, (d, a) => { float shape = 1 - Mathf.Clamp01((d * (1.1f + 0.35f * Mathf.Cos(a * 3))) - 0.45f) * 3; return Mathf.Clamp01(shape); });
            white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f, Texture2D.whiteTexture.width);
            bloom = Resources.Load<Material>("MienCho/WaitingGlow");
            painted = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            // a hot streak: a long thin diamond, brightest along its spine, drawn along each spark's flight
            spark = Make("ExplosionSpark", 64, (d, a) => { float x = Mathf.Abs(d * Mathf.Cos(a)), y = Mathf.Abs(d * Mathf.Sin(a)); return Mathf.Clamp01((1 - x - y * 3.4f) * 1.8f); });
            deathBlast = LoadDeathBlast();
            var big = new List<Sprite>();
            for (int i = 1; i <= 8; i++)
            {
                var texture = Resources.Load<Texture2D>("Effects/BigBlast_f" + i.ToString("00"));
                if (texture == null) break;
                big.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width, 0, SpriteMeshType.FullRect));
            }
            bigBlast = big.Count == 8 ? big.ToArray() : System.Array.Empty<Sprite>();
            // a burn on the floor: an ink blot with ragged lobes, soft at its rim
            burn = Make("ExplosionBurn", 128, (d, a) => { float lobes = 1 + 0.18f * Mathf.Sin(a * 5 + 1.3f) + 0.1f * Mathf.Cos(a * 11) + 0.06f * Mathf.Sin(a * 17 + 0.4f); return Mathf.Clamp01((1 - d * lobes) * 2.4f) * (0.55f + 0.45f * Mathf.Clamp01(1 - d * 1.5f)); });
            puff = Make("ExplosionSmudge", 96, (d, a) => { float wobble = 1 + 0.12f * Mathf.Sin(a * 5) + 0.06f * Mathf.Cos(a * 9); return Mathf.Clamp01((1 - d * wobble) * 1.6f) * (0.65f + 0.35f * Mathf.Clamp01(1 - d * 1.4f)); });
        }
        /// <summary>The painted golden blast every ordinary kill goes up in (Resources/Effects/DeathBlast_f0..3, one world
        /// unit wide). Loaded as textures so the import type never matters; empty when the art is missing.</summary>
        private static Sprite[] LoadDeathBlast()
        {
            var frames = new List<Sprite>();
            for (int i = 0; i < 4; i++)
            {
                var texture = Resources.Load<Texture2D>("Effects/DeathBlast_f" + i);
                if (texture == null) break;
                frames.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width, 0, SpriteMeshType.FullRect));
            }
            return frames.ToArray();
        }
        private static Sprite Make(string name, int size, System.Func<float, float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half, dy = (y - half) / half;
                    float value = Mathf.Clamp01(alpha(Mathf.Sqrt(dx * dx + dy * dy), Mathf.Atan2(dy, dx)));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(value * 255));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
        }

        public void Clear()
        {
            foreach (Blast blast in pool) { blast.Age = blast.Duration; Hide(blast); }
            foreach (Mote mote in motes) { mote.Age = mote.Life; mote.View.enabled = false; }
            foreach (Scorch scorch in scorches) { scorch.Age = scorch.Life; scorch.View.enabled = false; }
            shoves.Clear(); Streak = 0;
            ActiveCount = 0;
        }

        /// <summary>Called by the arena for each event; deathShown tells whether a death remnant was available.</summary>
        public void Observe(CombatEvent value, bool deathShown)
        {
            bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
            switch (value.Kind)
            {
                case CombatEventKind.Detonation when value.Cause.ItemId != null && value.Cause.ItemId.StartsWith("hero-identity-", System.StringComparison.Ordinal):
                    break; // a hero skill paints its own strike (requirement 23/10/2026: the blast disc over it read as film)
                case CombatEventKind.Detonation:
                    // the painted blast stays a local burst: a blast never washes over the whole screen, whatever reach it reports
                    Spawn(Kind.Blast, value.Position, Mathf.Clamp(value.Value, 0.8f, MaxBlastRadius), Tint(value.Element, value.Faction), true, value.Faction == Faction.Enemy);
                    break;
                case CombatEventKind.BossLanded:
                    Spawn(Kind.Shock, value.Position, 2.6f, new Color(0.86f, 0.74f, 0.55f), true, true);
                    break;
                case CombatEventKind.Kill when boss:
                    Spawn(Kind.Shock, value.Position, 4.4f, new Color(1f, 0.92f, 0.74f), true);
                    Spawn(Kind.Blast, value.Position, 2.2f, new Color(1f, 0.82f, 0.48f), true);
                    break;
                case CombatEventKind.Kill when value.Faction == Faction.Enemy && value.Subject?.Flung != true:
                    // the blast goes off at once, while the body acts its death inside it (requirement 09/11/2026: no waiting)
                    Pop(value.Subject?.Definition, value.Position, value.Critical, value.Value, value.Element, deathShown, value.Subject?.Elite == true);
                    break;
            }
        }
        /// <summary>An enemy goes up in one of two blasts, or drifts apart if it is a thing of air (a wisp, a moth, a shade).
        /// Small (an ordinary body): a white-hot snap, the golden fireball, sparks, motes in the killing element's colour drifting
        /// up, a burn on the floor that glows and cools to ink. Big (an elite, a large body, or a crit with real overkill): a bigger
        /// fireball cracking open in a chain of three rim bursts, flame tongues, hot sparks thrown up that fall and bounce, a dust
        /// skirt along the floor, a smoke column, a wide burn, a deep boom, and a shove that rocks the bodies round it. An elite
        /// also sends up a pillar of gold light. Kills in an unbroken run land a little bigger each time; 10, 25 and 50 ring out.</summary>
        public void Pop(UnitDefinition unit, Vector2 position, bool critical, float overkill, DamageElement element, bool bodyShown = true, bool elite = false)
        {
            float now = Time.unscaledTime;
            Streak = now - streakAt > 1f ? 1 : Streak + 1; streakAt = now;
            float surge = 1f + Mathf.Min(Streak, 40) * 0.004f; // at most 16% bigger in a long run
            if (now > popWindow) { popWindow = now + 0.1f; popsInWindow = 0; }
            bool budget = ++popsInWindow <= (PresentationSettings.ReducedEffects ? ReducedPopsPerWindow : PopsPerWindow);
            bool big = elite || unit != null && (unit.Radius >= 0.4f || unit.BodyHeight >= 1.4f) || critical && overkill > 0.6f;
            if (big)
            {
                if (now > bigWindow) { bigWindow = now + 0.2f; bigInWindow = 0; }
                big = ++bigInWindow <= (PresentationSettings.ReducedEffects ? ReducedBigPerWindow : BigPerWindow);
            }
            float height = unit != null ? Mathf.Max(0.3f, unit.BodyHeight) : 0.8f;
            // a unit position is the middle of its body (UnitVisual): the blast goes off a little below it, where a slumped body lies,
            // and its burn, dust and thrown sparks start at the feet (requirement 09/11/2026: big bodies blew up too high)
            Vector2 centre = position - Vector2.up * height * 0.08f, feet = position - Vector2.up * height * 0.45f;
            if (Streak == 10 || Streak == 25 || Streak == 50) Milestone(centre, Streak);
            // a crit or a big body always lands; when no body could be shown the burst has to carry the death on its own
            if (!budget && !critical && !big && bodyShown) return;
            // one size for every body (requirement 09/11/2026: a blast never grows with the monster, it stays a crisp local pop): small
            // 0.5, a crit 0.6, big 0.85, nudged by overkill and a long run of kills but capped
            float radius = (big ? BigPopRadius : critical ? 0.6f : PopRadius) * (1f + Mathf.Clamp01(overkill) * 0.1f) * surge;
            radius = Mathf.Min(radius, big ? MaxBigPopRadius : MaxPopRadius);
            Color elementColour = Tint(element, Faction.Companion);
            if (Drifts(unit))
            {
                if (Vapour(feet, centre, radius, elementColour, big)) PopsShown++;
            }
            else if (Look != DeathLook.Blast && bodyShown && !big)
            {
                // the body's own pieces carry the death; a spark of fire at most
                if (Look == DeathLook.ShatterSpark) Spawn(Kind.Pop, centre, 0.3f, critical ? new Color(1f, 0.93f, 0.7f) : Color.Lerp(new Color(1f, 0.86f, 0.62f), elementColour, 0.35f), critical);
                PopsShown++;
            }
            else
            {
                Color tint = critical ? new Color(1f, 0.93f, 0.7f) : Color.Lerp(new Color(1f, 0.86f, 0.62f), elementColour, 0.35f);
                if (!Spawn(Kind.Pop, centre, radius, tint, critical || !bodyShown || big, false, big)) return;
                PopsShown++;
                if (big) BigAftermath(feet, centre, radius, elementColour);
                else SmallAftermath(feet, centre, radius, elementColour, critical);
            }
            if (big)
            {
                BigPopsShown++;
                if (!PresentationSettings.ReducedMotion) shoves.Add((centre, 2.2f * Mathf.Sqrt(radius) + 0.8f, 0.22f * Mathf.Sqrt(radius), clock));
                CheckpointAudio.PlayWorldId("sig.alarm.boom", position);
                if (!Drifts(unit)) CheckpointAudio.PlayWorldId("fx.burn", position);
            }
            if (elite) Pillar(feet, centre, radius);
        }

        /// <summary>Things of air (wisps, moths, wraiths, shades, bats, lost souls) do not burn: they come apart into vapour.</summary>
        private static readonly string[] AirWords = { "Wisp", "Moth", "Wraith", "Soul", "Shade", "Ghost", "Phantom", "Spirit" };
        public static bool Drifts(UnitDefinition unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.Id)) return false;
            if (unit.Id.EndsWith("Bat", System.StringComparison.Ordinal)) return true;
            foreach (string word in AirWords) if (unit.Id.IndexOf(word, System.StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        /// <summary>The body bursts into pale vapour round a soft cold glow: curls that swell and swirl up, then thin away,
        /// with a few motes in the killing element's colour drifting up through them. No fire and no burn.</summary>
        private bool Vapour(Vector2 feet, Vector2 centre, float r, Color element, bool big)
        {
            bool sparse = PresentationSettings.ReducedEffects, still = PresentationSettings.ReducedMotion;
            float root = Mathf.Sqrt(r);
            int seed = Mathf.Abs(Mathf.RoundToInt(centre.x * 47 + centre.y * 23));
            Color pale = new Color(0.9f, 0.95f, 1f), haze = new Color(0.62f, 0.58f, 0.86f);
            // a cold white flash where the body was
            Mote flash = Spawn(MoteKind.Glow, centre, Vector2.zero, big ? 0.28f : 0.2f, (big ? 2.6f : 2f) * r, new Color(0.95f, 0.97f, 1f, PresentationSettings.ReducedFlashes ? 0.3f : 0.85f), Color.Lerp(pale, element, 0.35f));
            // curls rising in a twisting column, each a moment after the last, the lowest widest
            int curls = sparse ? 3 : big ? 9 : 6;
            for (int i = 0; i < curls; i++)
            {
                float side = (i % 2 == 0 ? 1 : -1) * (0.25f + 0.1f * (i % 3));
                Vector2 velocity = still ? Vector2.up * 0.4f : new Vector2(side * 1.3f, 1.1f + 0.35f * i) * root;
                Mote m = Spawn(MoteKind.Vapor, centre + new Vector2(side * 0.3f * r, 0.12f * i * r), velocity, 0.6f + 0.05f * i, (0.75f - 0.04f * i) * r, pale, Color.Lerp(haze, element, 0.25f));
                if (m != null) { m.Phase = (i % 2 == 0 ? 1 : -1) * (110 + 30 * (i % 3)); m.Age = -0.035f * i; }
            }
            // wisp streaks: thin pale threads curling up out of the column
            if (!still)
                for (int i = 0, n = sparse ? 2 : big ? 6 : 4; i < n; i++)
                {
                    float angle = (60 + 60f * i / Mathf.Max(1, n - 1)) * Mathf.Deg2Rad + Mathf.Sin(seed + i) * 0.2f;
                    Mote m = Spawn(MoteKind.Flame, centre, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 4.2f * root, 0.34f, 0.32f * root, new Color(1f, 1f, 1f), Color.Lerp(pale, element, 0.5f));
                    if (m != null) m.Phase = i * 1.3f;
                }
            for (int i = 0, n = sparse ? 2 : big ? 7 : 4; i < n; i++) Ember(centre, r, Color.Lerp(pale, element, 0.6f), i + seed, 0.55f, 0.95f);
            return flash != null;
        }

        /// <summary>An elite's last light: a pillar of gold shooting up out of the body, motes racing up it, then thinning away.</summary>
        private void Pillar(Vector2 feet, Vector2 centre, float r)
        {
            Color gold = new Color(1f, 0.95f, 0.72f), deep = new Color(1f, 0.62f, 0.2f);
            Mote shaft = Spawn(MoteKind.Pillar, feet, Vector2.zero, 0.7f, 3.4f + r * 1.4f, new Color(1f, 0.82f, 0.45f), deep);
            if (shaft != null) { shaft.Width = 1.1f + 0.25f * r; shaft.View.sprite = glow; shaft.View.sortingOrder = 2453; }
            Mote beam = Spawn(MoteKind.Pillar, feet, Vector2.zero, 0.62f, 3.2f + r * 1.4f, gold, deep);
            if (beam != null) beam.Width = 0.5f + 0.12f * r;
            Spawn(MoteKind.Glow, feet + Vector2.up * 0.2f, Vector2.zero, 0.35f, 1.8f * r + 0.6f, new Color(1f, 0.9f, 0.6f, PresentationSettings.ReducedFlashes ? 0.2f : 0.5f), deep);
            if (PresentationSettings.ReducedMotion) return;
            for (int i = 0; i < (PresentationSettings.ReducedEffects ? 2 : 5); i++)
            {
                Mote m = Spawn(MoteKind.Ember, feet + new Vector2((i - 2) * 0.07f, 0.2f + i * 0.25f), new Vector2((i - 2) * 0.05f, 3.4f + 0.5f * i), 0.5f + 0.06f * i, 0.13f, new Color(1f, 1f, 0.9f), gold);
                if (m != null) m.Phase = i * 2.1f;
            }
        }

        /// <summary>An unbroken run of kills reaching 10, 25 or 50: a warm swell of light over the kill and a rising note.</summary>
        private void Milestone(Vector2 centre, int streak)
        {
            Spawn(MoteKind.Glow, centre, Vector2.zero, 0.42f, 3.4f + streak * 0.04f, new Color(1f, 0.86f, 0.55f, PresentationSettings.ReducedFlashes ? 0.15f : 0.38f), new Color(1f, 0.7f, 0.35f));
            CheckpointAudio.PlayInterfaceId(streak >= 50 ? "up.note.5" : streak >= 25 ? "up.note.3" : "up.note.2");
        }

        private void SmallAftermath(Vector2 feet, Vector2 centre, float r, Color element, bool critical)
        {
            bool sparse = PresentationSettings.ReducedEffects;
            Burn(feet, r * 1.5f, 0.9f, 0.32f);
            int embers = sparse ? 2 : critical ? 6 : 4;
            for (int i = 0; i < embers; i++) Ember(centre, r, element, i + Mathf.RoundToInt(centre.x * 7), 0.45f, 0.75f);
        }

        private void BigAftermath(Vector2 feet, Vector2 centre, float r, Color element)
        {
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float root = Mathf.Sqrt(r);
            int seed = Mathf.Abs(Mathf.RoundToInt(centre.x * 53 + centre.y * 29));
            Burn(feet, r * 2.4f, 1.8f, 0.46f);
            Color hot = new Color(1f, 0.97f, 0.85f), ember = new Color(1f, 0.42f, 0.12f), smoke = new Color(0.17f, 0.12f, 0.18f), dust = new Color(0.62f, 0.53f, 0.44f);
            if (!still)
            {
                // flame tongues licking out of the blast, fast, then dragged to nothing
                for (int i = 0, n = sparse ? 4 : 7; i < n; i++)
                {
                    float angle = (i + (seed % 5) * 0.2f) * Mathf.PI * 2f / n + Mathf.Sin(seed + i) * 0.3f;
                    Mote m = Spawn(MoteKind.Flame, centre, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 6.2f * root * (0.8f + 0.3f * Mathf.Abs(Mathf.Sin(seed * 0.7f + i))), 0.26f + 0.04f * (i % 3), 0.42f * root, hot, Color.Lerp(ember, element, 0.3f));
                    if (m != null) m.Phase = i;
                }
                // hot sparks thrown up: they arc, fall, bounce once on the floor and fizzle
                for (int i = 0, n = sparse ? 5 : 11; i < n; i++)
                {
                    float angle = (i + 0.5f + (seed % 3) * 0.3f) * Mathf.PI * 2f / n + Mathf.Cos(seed + i * 1.7f) * 0.4f;
                    Mote m = Spawn(MoteKind.Spark, feet, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.55f) * 2.6f * root * (0.7f + 0.5f * Mathf.Abs(Mathf.Sin(seed * 0.3f + i))), 0.75f + 0.1f * (i % 3), 0.2f * root, new Color(1f, 0.9f, 0.55f), ember);
                    if (m != null) { m.Height = r * 0.6f; m.Lift = (3.2f + 1.6f * Mathf.Abs(Mathf.Sin(seed + i * 2.1f))) * root; }
                }
                // a dust skirt kicked out low along the floor
                for (int i = 0, n = sparse ? 3 : 5; i < n; i++)
                {
                    float angle = (i + 0.25f) * Mathf.PI * 2f / n + (seed % 7) * 0.1f;
                    Spawn(MoteKind.Dust, feet + Vector2.up * 0.05f, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.35f) * 2.4f * root, 0.55f, 0.75f * r, dust, dust);
                }
            }
            // a column of smoke rising after the fire, each curl a moment after the last
            for (int i = 0, n = sparse ? 2 : 3; i < n; i++)
            {
                Mote m = Spawn(MoteKind.Smoke, centre + new Vector2((i - 1) * 0.18f * r, 0.1f * r), new Vector2((i - 1) * 0.15f, 0.55f + 0.2f * i) * root, 0.95f + 0.1f * i, 0.95f * r, smoke, smoke);
                if (m != null) m.Age = -0.12f - i * 0.08f;
            }
            for (int i = 0, n = sparse ? 3 : 8; i < n; i++) Ember(centre, r, element, i + seed, 0.6f, 1.05f);
        }

        /// <summary>A mote in the killing element's colour (gold for plain hits) thrown out and drifting up as it flickers out.</summary>
        private void Ember(Vector2 centre, float r, Color element, int index, float shortest, float longest)
        {
            float angle = index * 2.399963f + 0.4f, root = Mathf.Sqrt(r);
            Mote m = Spawn(MoteKind.Ember, centre, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.6f + 0.5f) * (1.1f + 0.5f * Mathf.Abs(Mathf.Sin(index * 1.3f))) * root,
                Mathf.Lerp(shortest, longest, Mathf.Abs(Mathf.Sin(index * 0.77f))), 0.11f * root, new Color(1f, 0.97f, 0.82f), Color.Lerp(element, new Color(1f, 0.45f, 0.15f), 0.3f));
            if (m != null) m.Phase = index * 1.7f;
        }

        private Mote Spawn(MoteKind kind, Vector2 position, Vector2 velocity, float life, float size, Color hot, Color cool)
        {
            Mote mote = null;
            for (int n = 0; n < motes.Count; n++)
            {
                Mote candidate = motes[(moteCursor + n) % motes.Count];
                if (candidate.Age >= candidate.Life) { mote = candidate; moteCursor = (moteCursor + n + 1) % motes.Count; break; }
            }
            if (mote == null)
            {
                if (motes.Count >= MoteCapacity) return null;
                mote = new Mote { View = Part("Mote" + motes.Count, glow, 2456) };
                motes.Add(mote);
            }
            life = Mathf.Min(life, AftermathSeconds);
            mote.Kind = kind; mote.Position = position; mote.Velocity = velocity; mote.Height = 0; mote.Lift = 0; mote.Age = 0; mote.Life = life; mote.Size = size; mote.Width = 0; mote.Phase = 0; mote.Hot = hot; mote.Cool = cool;
            mote.View.sprite = kind == MoteKind.Flame || kind == MoteKind.Spark || kind == MoteKind.Pillar ? spark : kind == MoteKind.Ember || kind == MoteKind.Glow ? glow : puff;
            mote.View.sortingOrder = kind == MoteKind.Spark ? 2458 : kind == MoteKind.Smoke || kind == MoteKind.Vapor ? 2432 : kind == MoteKind.Dust ? 2425 : kind == MoteKind.Glow ? 2452 : kind == MoteKind.Pillar ? 2457 : 2456;
            mote.View.enabled = false;
            return mote;
        }

        /// <summary>A burn on the floor under the body: it glows ember-hot for a moment, cools to ink, then fades.</summary>
        private void Burn(Vector2 position, float size, float life, float alpha)
        {
            Scorch scorch = null;
            for (int n = 0; n < scorches.Count; n++)
            {
                Scorch candidate = scorches[(scorchCursor + n) % scorches.Count];
                if (candidate.Age >= candidate.Life) { scorch = candidate; scorchCursor = (scorchCursor + n + 1) % scorches.Count; break; }
            }
            if (scorch == null)
            {
                if (scorches.Count >= ScorchCapacity) { scorch = scorches[scorchCursor]; scorchCursor = (scorchCursor + 1) % scorches.Count; } // the oldest burn gives way
                else { scorch = new Scorch { View = Part("Burn" + scorches.Count, burn, 60) }; scorches.Add(scorch); } // on the floor, under every body
            }
            scorch.Position = position; scorch.Size = size; scorch.Life = Mathf.Min(life, AftermathSeconds); scorch.Alpha = alpha; scorch.Age = 0;
            scorch.View.transform.rotation = Quaternion.Euler(0, 0, Mathf.Repeat(position.x * 131 + position.y * 71, 360));
        }

        /// <summary>How far a big blast rocks the drawing of a body standing at <paramref name="position"/> (out from the blast, then
        /// back): the picture only, the body itself never moves.</summary>
        public static Vector2 Shove(Vector2 position)
        {
            ExplosionFeedback self = Active;
            if (self == null || self.shoves.Count == 0) return Vector2.zero;
            Vector2 total = Vector2.zero;
            foreach (var shove in self.shoves)
            {
                Vector2 away = position - shove.at; float distance = away.magnitude;
                if (distance >= shove.radius || distance < 0.05f) continue;
                float t = (self.clock - shove.born) / ShoveSeconds;
                if (t < 0 || t >= 1) continue;
                float envelope = t < 0.22f ? Ease(t / 0.22f) : 1 - Mathf.SmoothStep(0, 1, (t - 0.22f) / 0.78f);
                total += away / distance * shove.strength * (1 - distance / shove.radius) * envelope;
            }
            return Vector2.ClampMagnitude(total, 0.35f);
        }

        private void AdvanceAftermath(float delta)
        {
            clock += delta;
            shoves.RemoveAll(shove => clock - shove.born >= ShoveSeconds);
            foreach (Mote m in motes)
            {
                if (m.Age >= m.Life) continue;
                m.Age += delta;
                if (m.Age >= m.Life || m.Age < 0) { m.View.enabled = false; continue; }
                float t = m.Age / m.Life;
                Color c = Color.Lerp(m.Hot, m.Cool, Mathf.Clamp01(t * 1.4f));
                Transform view = m.View.transform;
                switch (m.Kind)
                {
                    case MoteKind.Ember:
                        m.Velocity *= Mathf.Max(0, 1 - 2.6f * delta); m.Velocity.y += 1.4f * delta; m.Position += m.Velocity * delta;
                        view.position = m.Position; view.rotation = Quaternion.identity;
                        view.localScale = Vector3.one * m.Size * (1 - 0.45f * t);
                        c.a = (1 - t) * (0.65f + 0.35f * Mathf.Sin(m.Age * 38 + m.Phase));
                        break;
                    case MoteKind.Flame:
                        m.Velocity *= Mathf.Max(0, 1 - 7.5f * delta); m.Position += m.Velocity * delta;
                        view.position = m.Position;
                        view.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(m.Velocity.y, m.Velocity.x) * Mathf.Rad2Deg + Mathf.Sin(m.Age * 30 + m.Phase) * 8);
                        view.localScale = new Vector3(m.Size * (2.4f - 1.5f * t), m.Size * 0.42f * (1 - t * 0.7f), 1);
                        c.a = 1 - t * t;
                        break;
                    case MoteKind.Spark:
                        m.Velocity *= Mathf.Max(0, 1 - 1.1f * delta); m.Position += m.Velocity * delta;
                        m.Lift -= 15f * delta; m.Height += m.Lift * delta;
                        if (m.Height < 0) { m.Height = 0; m.Lift = -m.Lift * 0.35f; m.Velocity *= 0.5f; }
                        Vector2 flight = m.Velocity + Vector2.up * m.Lift;
                        view.position = m.Position + Vector2.up * m.Height;
                        view.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(flight.y, flight.x) * Mathf.Rad2Deg);
                        view.localScale = new Vector3(m.Size * Mathf.Clamp(0.5f + flight.magnitude * 0.12f, 0.5f, 1.6f), m.Size * 0.35f, 1);
                        c.a = 1 - t * t;
                        break;
                    case MoteKind.Smoke:
                        m.Velocity *= Mathf.Max(0, 1 - 0.8f * delta); m.Position += m.Velocity * delta;
                        view.position = m.Position; view.rotation = Quaternion.Euler(0, 0, m.Age * 25);
                        view.localScale = Vector3.one * m.Size * (0.55f + 0.85f * Ease(t));
                        c.a = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t * 1.15f)) * 0.42f;
                        break;
                    case MoteKind.Vapor:
                        m.Velocity *= Mathf.Max(0, 1 - 2.2f * delta); m.Velocity.y += 0.5f * delta; m.Position += m.Velocity * delta;
                        view.position = m.Position; view.rotation = Quaternion.Euler(0, 0, m.Age * m.Phase);
                        view.localScale = new Vector3(m.Size * (0.5f + 1.05f * Ease(t)), m.Size * (0.45f + 0.8f * Ease(t)), 1);
                        c.a = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t * 1.1f + 0.08f)) * 0.75f;
                        break;
                    case MoteKind.Glow:
                        view.position = m.Position; view.rotation = Quaternion.identity;
                        view.localScale = Vector3.one * m.Size * (0.6f + 0.55f * Ease(t));
                        c = Color.Lerp(m.Hot, m.Cool, t); c.a = m.Hot.a * (1 - t) * (1 - t);
                        break;
                    case MoteKind.Pillar:
                        float rise = Ease(Mathf.Clamp01(t / 0.2f)), length = m.Size * rise;
                        view.position = m.Position + Vector2.up * length * 0.5f; view.rotation = Quaternion.Euler(0, 0, 90);
                        view.localScale = new Vector3(Mathf.Max(0.01f, length), m.Width * (1 - t * 0.85f) * (1 + 0.08f * Mathf.Sin(m.Age * 40)), 1);
                        c.a = (1 - t * t) * (PresentationSettings.ReducedFlashes ? 0.5f : 1f);
                        break;
                    default: // dust
                        m.Velocity *= Mathf.Max(0, 1 - 4.5f * delta); m.Position += m.Velocity * delta;
                        view.position = m.Position; view.rotation = Quaternion.identity;
                        float grow = 0.55f + 1.1f * Ease(t);
                        view.localScale = new Vector3(m.Size * grow, m.Size * grow * 0.45f, 1);
                        c.a = (1 - t) * 0.34f;
                        break;
                }
                m.View.color = c; m.View.enabled = true;
            }
            foreach (Scorch b in scorches)
            {
                if (b.Age >= b.Life) continue;
                b.Age += delta;
                if (b.Age >= b.Life) { b.View.enabled = false; continue; }
                float t = b.Age / b.Life, cool = Ease(Mathf.Clamp01(b.Age / 0.35f));
                Color c = Color.Lerp(new Color(1f, 0.5f, 0.18f), new Color(0.07f, 0.045f, 0.06f), cool);
                c.a = Mathf.Clamp01(b.Alpha * (1 - Mathf.Clamp01((t - 0.55f) / 0.45f)) * (cool < 1 ? 1.3f : 1f));
                b.View.color = c; b.View.enabled = true;
                b.View.transform.position = b.Position;
                b.View.transform.localScale = new Vector3(b.Size * Mathf.Lerp(0.7f, 1f, Ease(Mathf.Clamp01(b.Age / 0.12f))), b.Size * 0.55f, 1);
            }
        }

        /// <summary>The end of a boss finale: the body goes up in one big blast with a ground shock.</summary>
        public void BossFinale(Vector2 position, float height)
        {
            float size = Mathf.Clamp(height, 1.2f, 4f);
            Spawn(Kind.Shock, position, size * 1.6f, new Color(1f, 0.9f, 0.7f), true);
            Spawn(Kind.Blast, position, size * 0.95f, new Color(1f, 0.74f, 0.4f), true);
            Spawn(Kind.Pop, position + new Vector2(-0.3f, 0.2f) * size, size * 0.45f, new Color(1f, 0.86f, 0.6f), true);
            Spawn(Kind.Pop, position + new Vector2(0.35f, -0.1f) * size, size * 0.4f, new Color(1f, 0.8f, 0.5f), true);
        }
        private static Color Tint(DamageElement element, Faction faction)
        {
            if (faction == Faction.Enemy) return new Color(0.92f, 0.3f, 0.22f); // hostile blasts keep the ember red
            return element switch
            {
                DamageElement.Ember => new Color(1f, 0.55f, 0.24f),
                DamageElement.Frost => new Color(0.62f, 0.9f, 1f),
                DamageElement.Arcane => new Color(0.82f, 0.62f, 1f),
                _ => new Color(1f, 0.8f, 0.42f)
            };
        }

        private Blast Acquire(bool important)
        {
            Blast blast = pool.Find(candidate => candidate.Age >= candidate.Duration);
            if (blast != null) return blast;
            if (pool.Count >= Capacity)
            {
                if (!important) return null;
                Blast oldest = null;
                foreach (Blast candidate in pool) if (!candidate.Important && (oldest == null || candidate.Age > oldest.Age)) oldest = candidate;
                return oldest;
            }
            blast = new Blast();
            blast.Puffs[0] = Part("Smudge0", puff, 2430); blast.Puffs[1] = Part("Smudge1", puff, 2430); blast.Puffs[2] = Part("Smudge2", puff, 2430);
            blast.Rim = Part("InkRim", ring, 2441); blast.Ring = Part("ShockRing", ring, 2442); blast.Flash = Part("Flash", glow, 2450);
            blast.Core = Part("PaintedCore", null, 2448);
            blast.Bloom = Part("Bloom", white, 2452); if (bloom != null) blast.Bloom.sharedMaterial = bloom;
            for (int i = 0; i < blast.Chips.Length; i++) blast.Chips[i] = Part("Chip" + i, chip, 2460);
            pool.Add(blast);
            return blast;
        }
        private SpriteRenderer Part(string name, Sprite sprite, int order)
        {
            var child = new GameObject("Explosion" + name); child.transform.SetParent(transform, false);
            var view = child.AddComponent<SpriteRenderer>(); view.sprite = sprite; view.sortingOrder = order; view.enabled = false;
            return view;
        }

        private bool Spawn(Kind kind, Vector2 position, float radius, Color color, bool important, bool hostile = false, bool bigBody = false)
        {
            Blast blast = Acquire(important);
            if (blast == null) return false;
            if (blast.Age < blast.Duration) Hide(blast); // stolen from a quieter effect: clear what it was showing
            blast.Kind = kind; blast.Position = position; blast.Radius = radius; blast.Color = color; blast.Important = important; blast.Hostile = hostile; blast.Age = 0;
            blast.Big = bigBody && kind == Kind.Pop;
            blast.Duration = kind == Kind.Shock ? 0.75f : kind == Kind.Smudge ? 0.6f : kind == Kind.Burst ? 0.38f : kind == Kind.Pop ? (blast.Big ? BigPopDuration : PopDuration) : 0.55f;
            int seed = Mathf.Abs(Mathf.RoundToInt(position.x * 97 + position.y * 31)) + pool.IndexOf(blast) * 13;
            int spread = kind == Kind.Pop ? PopChips : blast.Chips.Length;
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                float angle = (i + (seed % 7) * 0.13f) * Mathf.PI * 2f / spread + Mathf.Sin(seed + i) * 0.35f;
                float speed = (kind == Kind.Burst ? 2.4f : kind == Kind.Pop ? 4.6f : 3.4f) * Mathf.Sqrt(radius) * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(seed * 0.37f + i * 1.7f)));
                blast.ChipVelocity[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                blast.ChipSpin[i] = (i % 2 == 0 ? 1 : -1) * (360 + 180 * Mathf.Abs(Mathf.Sin(seed + i)));
                blast.Chips[i].color = i % 3 == 0 ? new Color(0.13f, 0.09f, 0.14f) : i % 3 == 1 ? Color.Lerp(color, Color.white, 0.35f) : new Color(0.93f, 0.86f, 0.72f);
            }
            blast.Spin = (seed % 360) * 1f;
            Draw(blast);
            return true;
        }
        public const float PopDuration = 0.44f, BigPopDuration = 0.6f;
        private const int PopChips = 8;
        /// <summary>An ordinary kill goes up in a small golden explosion: a white-hot snap, the painted blast punching past
        /// full size and settling, a snap ring, hot sparks streaking out and dragging to a stop, then one curl of smoke.
        /// No debris of any kind (no crockery, wood or metal chips). All from the shared pool, gone in under half a second.</summary>
        private void DrawPop(Blast blast)
        {
            float age = blast.Age, r = blast.Radius;
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float flashGain = PresentationSettings.ReducedFlashes ? 0.3f : 1f;
            blast.Rim.enabled = false;
            // the snap of the hit: a white-hot point blowing out in 60 ms
            float flashT = Mathf.Clamp01(age / 0.06f);
            blast.Flash.enabled = flashT < 1f;
            if (blast.Flash.enabled)
            {
                blast.Flash.sprite = glow;
                blast.Flash.transform.position = blast.Position;
                blast.Flash.transform.localScale = Vector3.one * (2f * r * Mathf.Lerp(0.6f, 1.4f, Ease(flashT)));
                blast.Flash.color = new Color(1f, 0.98f, 0.9f, (1 - flashT * flashT) * flashGain);
            }
            // warm light spilling over the crowd (stronger for crits and overkill)
            float bloomT = Mathf.Clamp01(age / (blast.Important ? 0.22f : 0.17f));
            blast.Bloom.enabled = bloom != null && bloomT < 1f && !sparse;
            if (blast.Bloom.enabled)
            {
                blast.Bloom.transform.position = blast.Position;
                blast.Bloom.transform.localScale = Vector3.one * (2.8f * r * Mathf.Lerp(0.7f, 1.1f, Ease(bloomT)));
                blast.Bloom.color = new Color(1f, 0.78f, 0.42f, (1 - bloomT) * (blast.Important ? 0.75f : 0.5f) * flashGain);
            }
            // the fireball: four painted frames in 0.3 s, punching past full size, holding, then burning out
            bool ownBig = blast.Big && bigBlast.Length > 0;
            bool paintedBlast = ownBig || deathBlast.Length > 0;
            Sprite[] frames = ownBig ? bigBlast : deathBlast.Length > 0 ? deathBlast : painted == null ? null : painted.Death;
            float coreT = Mathf.Clamp01(age / (ownBig ? 0.52f : 0.3f));
            blast.Core.enabled = frames != null && frames.Length > 0 && coreT < 1f;
            if (blast.Core.enabled)
            {
                Sprite frame = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(coreT * frames.Length))];
                blast.Core.sprite = frame;
                blast.Core.transform.position = blast.Position;
                float punch = still ? 1f : coreT < 0.25f ? Mathf.Lerp(0.55f, 1.18f, Ease(coreT / 0.25f)) : Mathf.Lerp(1.18f, 1.05f, (coreT - 0.25f) / 0.75f);
                float inkWidth = ownBig ? BigBlastInk : paintedBlast ? 0.93f : Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(frame).width);
                blast.Core.transform.localScale = Vector3.one * (2.3f * r * punch / inkWidth);
                blast.Core.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : blast.Spin % 40f - 20f);
                float alpha = Mathf.Clamp01((1 - coreT) * 2.5f);
                Color tint = paintedBlast ? Color.white : Color.Lerp(Color.white, blast.Color, 0.25f);
                if (PresentationSettings.ReducedFlashes) tint = Color.Lerp(tint, new Color(0.85f, 0.78f, 0.7f), 0.3f);
                blast.Core.color = new Color(tint.r, tint.g, tint.b, alpha);
            }
            // (requirement 23/10/2026: no plain ring snapping out of every death; the burst, sparks and smoke carry it)
            float ringT = Mathf.Clamp01(age / 0.2f);
            float ringAlpha = 0f;
            blast.Ring.enabled = ringAlpha > 0.02f;
            if (blast.Ring.enabled)
            {
                blast.Ring.sprite = ring;
                blast.Ring.transform.position = blast.Position;
                blast.Ring.transform.localScale = Vector3.one * (2f * r * (still ? 1.2f : Mathf.Lerp(0.5f, 1.5f, Ease(ringT))));
                blast.Ring.color = new Color(1f, 0.9f, 0.62f, ringAlpha);
            }
            // hot sparks: white, then gold, then ember, streaking along their flight and dragging to a stop
            int shown = still ? 0 : sparse ? 3 : PopChips;
            float sparkT = Mathf.Clamp01(age / 0.32f);
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                SpriteRenderer view = blast.Chips[i];
                view.enabled = i < shown && sparkT < 1f;
                if (!view.enabled) continue;
                view.sprite = spark;
                Vector2 velocity = blast.ChipVelocity[i];
                float travel = (1 - Mathf.Pow(1 - sparkT, 3f)) * 0.2f;
                view.transform.position = blast.Position + velocity * travel;
                view.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
                float speed = (1 - sparkT) * (1 - sparkT), size = Mathf.Sqrt(r);
                view.transform.localScale = new Vector3(Mathf.Lerp(0.1f, 0.62f, speed) * size, 0.1f * size * (1 - sparkT * 0.5f), 1);
                Color hot = sparkT < 0.3f ? Color.Lerp(new Color(1f, 1f, 0.92f), new Color(1f, 0.86f, 0.36f), sparkT / 0.3f)
                    : Color.Lerp(new Color(1f, 0.86f, 0.36f), new Color(1f, 0.42f, 0.16f), (sparkT - 0.3f) / 0.7f);
                view.color = new Color(hot.r, hot.g, hot.b, 1 - sparkT * sparkT);
            }
            // one curl of smoke once the fire is out
            float puffT = Mathf.Clamp01((age - 0.14f) / (blast.Duration - 0.14f));
            for (int i = 0; i < blast.Puffs.Length; i++)
            {
                SpriteRenderer view = blast.Puffs[i];
                view.enabled = i == 0 && puffT > 0 && puffT < 1;
                if (!view.enabled) continue;
                view.sprite = puff;
                view.transform.position = blast.Position + (still ? Vector2.zero : Vector2.up * puffT * 0.3f * r);
                view.transform.localScale = Vector3.one * (2f * r * (still ? 0.9f : Mathf.Lerp(0.6f, 1.1f, Ease(puffT))));
                view.color = new Color(0.16f, 0.11f, 0.17f, (1 - puffT) * 0.42f);
            }
        }

        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            AdvanceAftermath(delta);
            ActiveCount = 0;
            foreach (Blast blast in pool)
            {
                if (blast.Age >= blast.Duration) continue;
                blast.Age += delta;
                if (blast.Age >= blast.Duration) { Hide(blast); continue; }
                ActiveCount++;
                Draw(blast);
            }
        }

        private void Draw(Blast blast)
        {
            if (blast.Kind == Kind.Pop) { DrawPop(blast); return; }
            float t = Mathf.Clamp01(blast.Age / blast.Duration);
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float flashGain = PresentationSettings.ReducedFlashes ? 0.35f : 1f;
            float r = blast.Radius;
            // flash: a hot core that blooms to the blast edge and is gone within a tenth of a second
            float flashT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.2f : 0.15f));
            bool flash = blast.Kind != Kind.Smudge && flashT < 1f;
            blast.Flash.enabled = flash;
            if (flash)
            {
                blast.Flash.transform.position = blast.Position;
                blast.Flash.transform.localScale = Vector3.one * (2f * r * Mathf.Lerp(0.55f, 1.05f, Ease(flashT)));
                blast.Flash.color = new Color(Mathf.Lerp(1f, blast.Color.r, flashT), Mathf.Lerp(0.97f, blast.Color.g, flashT), Mathf.Lerp(0.9f, blast.Color.b, flashT), (1 - flashT * flashT) * flashGain);
            }
            // bloom: the light of the blast spilling over everything nearby
            float bloomT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.35f : 0.24f));
            // our own blasts keep no wide bloom disc (it read as a film over the brigade); hostile ones still glow
            blast.Bloom.enabled = blast.Kind != Kind.Smudge && bloom != null && bloomT < 1f && (blast.Hostile || blast.Kind == Kind.Shock);
            if (blast.Bloom.enabled)
            {
                blast.Bloom.transform.position = blast.Position;
                blast.Bloom.transform.localScale = Vector3.one * (2.8f * r * Mathf.Lerp(0.7f, 1.1f, Ease(bloomT)));
                blast.Bloom.color = new Color(blast.Color.r, blast.Color.g * 0.92f, blast.Color.b * 0.8f, (1 - bloomT) * 0.75f * flashGain);
            }
            // a painted core: the game's own slam-dust (or death puff for kill bursts) blown up to the blast
            Sprite[] frames = painted == null ? null : blast.Kind == Kind.Burst ? painted.Death : painted.Dust;
            float coreLife = blast.Kind == Kind.Shock ? 0.5f : 0.36f;
            float coreT = Mathf.Clamp01(blast.Age / coreLife);
            blast.Core.enabled = blast.Kind != Kind.Smudge && frames != null && frames.Length > 0 && coreT < 1f;
            if (blast.Core.enabled)
            {
                Sprite frame = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(coreT * frames.Length))];
                blast.Core.sprite = frame;
                blast.Core.transform.position = blast.Position;
                float coreWidth = 2f * r * (blast.Kind == Kind.Burst ? 0.9f : 0.95f) * Mathf.Lerp(0.75f, 1.05f, Ease(coreT));
                blast.Core.transform.localScale = Vector3.one * (coreWidth / Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(frame).width));
                blast.Core.transform.rotation = Quaternion.Euler(0, 0, blast.Position.x * 47f % 360f);
                Color tint = Color.Lerp(Color.white, blast.Color, blast.Hostile ? .8f : .35f);
                blast.Core.color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01((1 - coreT) * 1.6f));
            }
            // shock ring and its inked rim: out to just past the real radius, easing out
            bool ringed = blast.Kind != Kind.Smudge && (blast.Hostile || blast.Kind == Kind.Shock); // a hostile blast shows its reach; ours do not ring
            float ringT = Mathf.Clamp01(blast.Age / (blast.Kind == Kind.Shock ? 0.55f : 0.36f));
            float ringScale = 2f * r * (still ? 1.05f : Mathf.Lerp(0.35f, 1.15f, Ease(ringT)));
            float ringAlpha = ringed ? Mathf.Pow(1 - ringT, 1.4f) : 0;
            blast.Ring.enabled = blast.Rim.enabled = ringAlpha > 0.01f;
            if (blast.Ring.enabled)
            {
                Sprite inkRing = Painted(blast) ? CombatVfxArtwork.Frame("BlastRing", 0) : null;
                blast.Ring.sprite = inkRing != null ? inkRing : ring;
                blast.Ring.transform.position = blast.Rim.transform.position = blast.Position;
                blast.Ring.transform.localScale = Vector3.one * (ringScale / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(blast.Ring.sprite).width));
                blast.Rim.transform.localScale = Vector3.one * ringScale * 1.03f;
                blast.Ring.color = new Color(Mathf.Lerp(blast.Color.r, 1f, 0.3f), Mathf.Lerp(blast.Color.g, 1f, 0.3f), Mathf.Lerp(blast.Color.b, 1f, 0.3f), ringAlpha);
                blast.Rim.color = new Color(0.11f, 0.07f, 0.12f, ringAlpha * 0.7f);
                if (inkRing != null) { blast.Rim.enabled = false; blast.Ring.color = blast.Hostile ? new Color(.95f, .3f, .2f, ringAlpha) : new Color(1, 1, 1, ringAlpha); }
            }
            // chips: paper, ink and coloured flecks thrown outward, slowing and spinning, then gone
            bool chips = blast.Kind != Kind.Smudge && !still && !sparse;
            float chipT = Mathf.Clamp01(blast.Age / (blast.Duration * 0.85f));
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                SpriteRenderer view = blast.Chips[i];
                Sprite inkChip = Painted(blast) ? CombatVfxArtwork.Frame("Chip_" + (i % 6 == 4 ? 3 : i % 6), 0) : null; // never the blue-and-white porcelain shard
                view.sprite = inkChip != null ? inkChip : chip;
                view.enabled = chips && chipT < 1f && (blast.Kind != Kind.Burst || i < 7);
                if (!view.enabled) continue;
                float travel = (1 - Mathf.Pow(1 - chipT, 2.2f)) * blast.Duration * 0.55f;
                Vector2 at = blast.Position + blast.ChipVelocity[i] * travel + Vector2.up * Mathf.Sin(chipT * Mathf.PI) * 0.18f * Mathf.Sqrt(r);
                view.transform.position = at;
                view.transform.rotation = Quaternion.Euler(0, 0, blast.ChipSpin[i] * blast.Age);
                float size = (blast.Kind == Kind.Burst ? 0.18f : 0.26f) * Mathf.Sqrt(r) * (1 - chipT * 0.6f);
                float inkWidth = Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).width);
                view.transform.localScale = Vector3.one * (size * 1.5f / inkWidth);
                Color c = view.color; view.color = new Color(c.r, c.g, c.b, 1 - chipT * chipT);
                if (inkChip != null) view.color = new Color(1, 1, 1, 1 - chipT * chipT);
            }
            // a smudge of ink settles where it happened
            int puffs = blast.Kind == Kind.Burst ? 1 : sparse ? 1 : 3;
            float puffT = Mathf.Clamp01((blast.Age - (blast.Kind == Kind.Smudge ? 0 : 0.05f)) / (blast.Duration * 0.95f));
            for (int i = 0; i < blast.Puffs.Length; i++)
            {
                SpriteRenderer view = blast.Puffs[i];
                Sprite[] inkPuffs = null; // the painted purple smoke is retired (requirement 22/10/2026): the code-drawn smudge settles instead
                Sprite inkPuff = inkPuffs == null || inkPuffs.Length == 0 ? null : inkPuffs[Mathf.Min(inkPuffs.Length - 1, Mathf.FloorToInt(puffT * inkPuffs.Length))];
                view.sprite = inkPuff != null ? inkPuff : puff;
                view.enabled = i < puffs && puffT > 0 && puffT < 1;
                if (!view.enabled) continue;
                float angle = i * 2.1f + blast.Position.x;
                Vector2 offset = i == 0 ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r * 0.35f;
                view.transform.position = blast.Position + offset * (still ? 1 : Ease(puffT));
                view.transform.localScale = Vector3.one * (2f * r * (i == 0 ? 0.7f : 0.45f) * (still ? 1 : Mathf.Lerp(0.6f, 1.1f, Ease(puffT))) / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(view.sprite).width));
                Color smudge = blast.Kind == Kind.Smudge ? blast.Color : new Color(0.14f, 0.1f, 0.17f);
                view.color = new Color(smudge.r, smudge.g, smudge.b, (1 - puffT) * (blast.Kind == Kind.Smudge ? 0.55f : 0.32f));
                if (inkPuff != null) view.color = new Color(1, 1, 1, (1 - puffT) * .6f);
            }
            if (blast.Hostile && blast.Kind != Kind.Smudge) EmberBurst(blast, still, sparse, flashGain);
            ApplyPaintedFlash(blast);
        }
        private static Sprite Once(string key, float progress)
        {
            var frames = CombatVfxArtwork.Frames(key);
            return frames == null || frames.Length == 0 ? null : frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(Mathf.Clamp01(progress) * frames.Length))];
        }
        private static void FitPainted(SpriteRenderer view, Sprite image, float width)
        {
            if (!view.enabled || image == null) return;
            Vector3 centre = view.transform.position; float alpha = view.color.a;
            Rect ink = SpriteInkMetrics.BoundsOf(image); view.sprite = image;
            view.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, ink.width));
            view.transform.position = centre - view.transform.TransformVector(ink.center);
            view.color = new Color(1, 1, 1, alpha);
        }
        /// <summary>A hostile blast as a code-drawn ember burst (requirement 22/10/2026): a white-hot core flash cooling to orange, a soft
        /// orange-red halo, ten embers flying out and fading, a brief heat-ripple ring; no painted dust core.</summary>
        private void EmberBurst(Blast blast, bool still, bool sparse, float flashGain)
        {
            float r = blast.Radius, age = blast.Age;
            Color hot = new Color(1f, .95f, .76f), ember = new Color(1f, .45f, .16f), deep = new Color(.8f, .14f, .1f);
            blast.Core.enabled = false;
            float flashT = Mathf.Clamp01(age / .16f);
            blast.Flash.enabled = flashT < 1;
            if (blast.Flash.enabled)
            {
                blast.Flash.sprite = glow; blast.Flash.transform.position = blast.Position;
                blast.Flash.transform.localScale = Vector3.one * (2f * r * (still ? .8f : Mathf.Lerp(.35f, .9f, Ease(flashT))));
                Color c = Color.Lerp(hot, ember, flashT); blast.Flash.color = new Color(c.r, c.g, c.b, (1 - flashT * flashT) * flashGain);
            }
            float haloT = Mathf.Clamp01(age / .4f);
            if (bloom != null && haloT < 1)
            {
                blast.Bloom.enabled = true; blast.Bloom.transform.position = blast.Position;
                blast.Bloom.transform.localScale = Vector3.one * (2.6f * r * (still ? 1 : Mathf.Lerp(.7f, 1.15f, Ease(haloT))));
                Color c = Color.Lerp(ember, deep, haloT); blast.Bloom.color = new Color(c.r, c.g, c.b, (1 - haloT) * .6f * flashGain);
            }
            // the heat ripple: a faint warm ring passing out to the edge, gone in a third of a second
            float rippleT = Mathf.Clamp01(age / .32f);
            blast.Rim.enabled = false; blast.Ring.enabled = !still && rippleT < 1;
            if (blast.Ring.enabled)
            {
                blast.Ring.sprite = ring; blast.Ring.transform.position = blast.Position;
                blast.Ring.transform.localScale = Vector3.one * (2f * r * Mathf.Lerp(.4f, 1.1f, Ease(rippleT)) / Mathf.Max(.01f, ring.bounds.size.x));
                blast.Ring.color = new Color(1f, .78f, .55f, (1 - rippleT) * (1 - rippleT) * .35f * flashGain);
            }
            // embers: ten sparks thrown out, slowing, cooling from white-gold to red, then gone
            float emberT = Mathf.Clamp01(age / Mathf.Max(.3f, blast.Duration * .8f));
            for (int i = 0; i < blast.Chips.Length; i++)
            {
                SpriteRenderer view = blast.Chips[i];
                view.enabled = !sparse && !still && i < 10 && emberT < 1;
                if (!view.enabled) continue;
                view.sprite = glow;
                float travel = (1 - Mathf.Pow(1 - emberT, 2.4f)) * blast.Duration * .7f;
                view.transform.position = blast.Position + blast.ChipVelocity[i] * travel + Vector2.up * emberT * .2f;
                view.transform.rotation = Quaternion.identity;
                view.transform.localScale = Vector3.one * (.16f * Mathf.Sqrt(r) * (1 - emberT * .5f) / Mathf.Max(.01f, glow.bounds.size.x));
                Color c = Color.Lerp(Color.Lerp(hot, ember, emberT * 1.6f), deep, Mathf.Clamp01(emberT * 2 - 1));
                view.color = new Color(c.r, c.g, c.b, 1 - emberT * emberT);
            }
            // the settling smudge stays faint under the embers
            foreach (var puff in blast.Puffs) if (puff.enabled) { Color c = puff.color; c.a *= .5f; puff.color = c; }
        }
        /// <summary>Hostile blasts and every enemy death are drawn in code (requirement 22/10/2026: no painted fire, rock or smoke on the
        /// hostile side); the painted flash, ring and chips stay only for our own detonations.</summary>
        private static bool Painted(Blast blast) => !blast.Hostile && blast.Kind != Kind.Pop;
        private static void ApplyPaintedFlash(Blast blast)
        {
            if (!Painted(blast)) return;
            float life = blast.Kind == Kind.Pop ? .09f : blast.Kind == Kind.Shock ? .2f : .15f;
            FitPainted(blast.Flash, Once(blast.Hostile ? "HostileBlast" : "BlastFlash", blast.Age / life), blast.Flash.transform.localScale.x);
            // The ordinary-death path is independently pooled; replace its geometry as well as large detonations.
            if (blast.Kind != Kind.Pop) return;
            FitPainted(blast.Ring, CombatVfxArtwork.Frame("BlastRing", 0), blast.Ring.transform.localScale.x);
            for (int i = 0; i < blast.Chips.Length; i++) FitPainted(blast.Chips[i], CombatVfxArtwork.Frame("Chip_" + (i % 6 == 4 ? 3 : i % 6), 0), blast.Chips[i].transform.localScale.x);
        }
        private static float Ease(float t) => 1 - Mathf.Pow(1 - Mathf.Clamp01(t), 3);
        private static void Hide(Blast blast)
        {
            blast.Flash.enabled = blast.Ring.enabled = blast.Rim.enabled = blast.Core.enabled = blast.Bloom.enabled = false;
            foreach (SpriteRenderer view in blast.Chips) view.enabled = false;
            foreach (SpriteRenderer view in blast.Puffs) view.enabled = false;
        }
    }
}
