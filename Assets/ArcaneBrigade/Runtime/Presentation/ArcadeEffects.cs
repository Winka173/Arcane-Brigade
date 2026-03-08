using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The small combat cues: a weapon's wind-up gathering, its muzzle and swing, a signature mark, burst or pulse,
    /// a proc, a heal, a pickup, a chest. Each is a short painted moment in several stages, not a lone outline: a flash,
    /// the painted ring / sparkle / crescent / ink puff of PaintedAnimationLibrary and CombatVFX, and motes themed by the
    /// weapon that made it (leaves, notes, bubbles, tea drops, stars, magnet sparks, film frames, embers...) that gather,
    /// burst, rise or trail. Large areas lie on the floor under the bodies; small cues sit over them.</summary>
    public sealed class ArcadeEffects : MonoBehaviour
    {
        private enum Shape { Ring, Slash, Star, Puff, Glyph }
        private enum Drift { Out, Rise }
        private const int Motes = 8;
        private sealed class Burst
        {
            public Transform Transform;
            public SpriteRenderer Core, Glow, Ring;
            public readonly SpriteRenderer[] Motes = new SpriteRenderer[ArcadeEffects.Motes];
            public readonly Vector2[] Heading = new Vector2[ArcadeEffects.Motes];
            public readonly float[] Spin = new float[ArcadeEffects.Motes];
            public Shape Shape;
            public Drift Drift;
            public Sprite Mote;
            public float Age = 10f, Duration, Radius, Angle;
            public Color Color;
            public bool Floor;
        }
        private readonly List<Burst> bursts = new List<Burst>();
        private PaintedAnimationLibrary library;
        private static Sprite softGlow, fallbackRing, ripple;
        private string theme;
        private Drift motion;
        private int seed;
        public int ActiveCount { get; private set; }

        private void Awake()
        {
            library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            softGlow ??= Make("ArcadeSoftGlow", 96, d => Mathf.Pow(Mathf.Clamp01(1 - d), 2.2f));
            fallbackRing ??= Make("ArcadeRing", 160, d => Mathf.Clamp01(1 - Mathf.Abs(d - 0.86f) / 0.12f));
            ripple ??= Make("ArcadeRipple", 256, d => Mathf.Pow(Mathf.Clamp01(1 - Mathf.Abs(d - 0.93f) / 0.06f), 1.6f) + 0.18f * Mathf.Clamp01(1 - Mathf.Abs(d - 0.8f) / 0.12f));
        }
        private static Sprite Make(string name, int size, System.Func<float, float> alpha)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size]; float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half, dy = (y - half) / half;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha(Mathf.Sqrt(dx * dx + dy * dy))) * 255));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
        }

        public void Clear()
        {
            foreach (Burst burst in bursts) { burst.Age = 10f; Hide(burst); }
            ActiveCount = 0;
        }

        /// <summary>The painted mote a weapon's cues are made of (its own projectile art), a soft spark otherwise.</summary>
        public static string MoteKey(WeaponDefinition weapon) => weapon?.Id switch
        {
            "CrescentBow" => "ArrowBolt", "RuneStaff" or "DreamOrbStaff" or "MoonseedOrb" or "PlasmaTrident" => "RuneOrb", "BellFork" => "BellChime", "AcornSling" => "AcornPellet",
            "BubbleWand" or "UmbrellaLance" => "Bubble", "ButtonBow" => "ButtonDart", "InkPistol" => "InkBullet", "LanternStaff" or "DreamIron" or "FireExtinguisher" => "LanternEmber",
            "LeafCrossbow" or "MoonFeatherFan" or "PaperFan" => "LeafBolt", "MailboxLauncher" or "DreamStamp" or "StampHammer" => "Letter", "MusicBox" or "RibbonHarp" or "StarKazoo" => "MusicNote",
            "ShellCannon" => "ShellShot", "SnowGlobeOrb" => "GlobeShard", "TeapotPopgun" or "TeabagFlail" => "TeaDrop", "TinFish" or "CrescentBoomerang" or "SatelliteDisc" => "TinFish",
            "DartCompass" or "ClockCrescent" or "ClockworkYoYo" or "AlarmMortar" => "CompassDart", "ScrapMagnet" => "MagnetBolt", "DreamProjector" or "SleepCamera" => "FilmFrame",
            "NeedleSinger" or "CloudScissors" or "TapeWhip" or "TwinSabres" or "RibbonWheel" => "Thread", _ => "AllySpark"
        };

        public static Color ElementColor(DamageElement element)
        {
            return element == DamageElement.Ember ? new Color(1f, 0.34f, 0.12f) : element == DamageElement.Frost ? new Color(0.15f, 0.92f, 1f) : element == DamageElement.Arcane ? new Color(0.85f, 0.25f, 1f) : new Color(1f, 0.82f, 0.28f);
        }

        public void Play(CombatEvent value)
        {
            WeaponDefinition weapon = value.Weapon; theme = MoteKey(weapon); motion = value.Kind == CombatEventKind.Heal || value.Kind == CombatEventKind.ChestOpened || value.Kind == CombatEventKind.Support ? Drift.Rise : Drift.Out;
            Color color = ElementColor(value.Element);
            float angle = Mathf.Atan2(value.Direction.y, value.Direction.x) * Mathf.Rad2Deg;
            if (value.Kind == CombatEventKind.Dodge)
            {
                Spawn(Shape.Puff, value.Position, 0, 0.32f, 0.25f, new Color(0.3f, 1, 0.85f));
            }
            else if (value.Kind == CombatEventKind.Support)
            {
                Spawn(Shape.Glyph, value.Position + Vector2.up * 0.25f, 0, 0.22f, 0.36f, color);
            }
            else if (value.Kind == CombatEventKind.SignatureMark)
            {
                Spawn(Shape.Star, value.Position, 45, 0.12f + value.Value * 0.055f, 0.24f, new Color(1, 0.28f, 0.38f));
            }
            else if (value.Kind == CombatEventKind.SignatureBurst)
            {
                Spawn(Shape.Star, value.Position, 45, 0.72f, 0.28f, new Color(1, 0.25f, 0.34f));
                Spawn(Shape.Glyph, value.Position, 0, 0.55f, 0.35f, new Color(1, 0.86f, 0.56f));
            }
            else if (value.Kind == CombatEventKind.SignaturePulse)
            {
                // the muzzle bloom of a thrown volley (bell chimes, umbrella drops): a small flourish at the wielder's front
                bool umbrella = weapon != null && weapon.Signature == SignatureWeapon.Umbrella;
                Spawn(umbrella ? Shape.Slash : Shape.Star, value.Position, angle, weapon != null ? Mathf.Min(0.6f, value.Value) : value.Value, 0.26f, umbrella ? new Color(0.35f, 0.95f, 0.88f) : new Color(1, 0.82f, 0.35f));
            }
            else if (value.Kind == CombatEventKind.Fire && weapon != null && value.Faction != Faction.Enemy) { /* our weapons' swings and muzzles are drawn in their own ink (ArsenalVisual, WeaponInkStyle) */ }
            else if (value.Kind == CombatEventKind.Fire && weapon != null)
            {
                if (weapon.Family == ExpandedWeapon.Camera)
                {
                    Spawn(Shape.Glyph, value.Position + value.Direction * 0.8f, angle, 0.55f, 0.18f, new Color(0.5f, 1, 0.86f));
                    Spawn(Shape.Star, value.Position + value.Direction * 0.6f, angle, 0.25f, 0.08f, Color.white);
                }
                else if (weapon.Family == ExpandedWeapon.Scissors)
                {
                    Spawn(Shape.Slash, value.Position, angle - 18, weapon.Range, 0.22f, new Color(0.3f, 0.95f, 0.85f));
                    Spawn(Shape.Slash, value.Position, angle + 18, weapon.Range * 0.9f, 0.28f, new Color(1, 0.92f, 0.74f));
                }
                else if (weapon.Family == ExpandedWeapon.Iron)
                    Spawn(Shape.Puff, value.Position + value.Direction, angle, 0.4f, 0.32f, new Color(1, 0.65f, 0.3f, 0.6f));
                else if (weapon.Family == ExpandedWeapon.Hammer)
                {
                    Spawn(Shape.Star, value.Position + value.Direction * 1.1f, angle, 0.48f, 0.2f, new Color(1, 0.68f, 0.25f));
                    Spawn(Shape.Ring, value.Position + value.Direction * 1.1f, angle, 0.8f, 0.25f, new Color(1, 0.9f, 0.65f));
                }
                else if (weapon.Kind == WeaponKind.Melee)
                {
                    float radius = Mathf.Min(weapon.Range, 2.5f);
                    if (weapon.Signature == SignatureWeapon.Stamp)
                        Spawn(Shape.Star, value.Position + value.Direction * weapon.Range * 0.65f, 45, 0.22f, 0.13f, new Color(1, 0.35f, 0.38f));
                    else
                    {
                        Spawn(Shape.Slash, value.Position, angle, radius, 0.18f, color);
                        Spawn(Shape.Slash, value.Position, angle + 8f, radius * 0.9f, 0.1f, new Color(1f, 1f, 0.88f));
                    }
                }
                else
                {
                    Vector2 offset = weapon.HasMuzzleOffset ? weapon.ProjectileMuzzleOffset : Vector2.right * weapon.MuzzleDistance;
                    Vector2 muzzle = value.Position + value.Direction * (weapon.MountDistance - weapon.RecoilDistance) + (Vector2)(Quaternion.Euler(0, 0, angle) * offset);
                    Spawn(weapon.Motion == WeaponMotion.Bow ? Shape.Star : Shape.Glyph, muzzle, angle, weapon.Motion == WeaponMotion.Bow ? 0.25f : 0.42f, 0.22f, color);
                }
            }
            else if (value.Kind == CombatEventKind.Windup && weapon != null && (weapon.Motion == WeaponMotion.Cast || weapon.Motion == WeaponMotion.Bell))
                Spawn(Shape.Glyph, value.Position + value.Direction * 0.7f, angle, 0.26f, weapon.Windup, color);
            else if (value.Kind == CombatEventKind.Hit && weapon != null && value.Faction == Faction.Enemy) { /* an enemy struck by our weapon flashes in that weapon's own palette and token */ }
            else if (value.Kind == CombatEventKind.Hit)
            {
                Spawn(Shape.Star, value.Position, angle + 15f, value.Critical ? 0.58f : 0.32f, 0.12f, new Color(1f, 1f, 0.9f));
            }
            else if (value.Kind == CombatEventKind.Kill)
            {
                // ordinary enemy kills already burst in ExplosionFeedback.Pop; only other deaths get the soft puff
                if (value.Faction != Faction.Enemy || value.Subject != null && value.Subject.Definition.IsBoss)
                    Spawn(Shape.Puff, value.Position, 0, 0.6f, 0.32f, new Color(0.17f, 0.13f, 0.26f));
            }
            else if (value.Kind == CombatEventKind.Pickup)
                Spawn(Shape.Star, value.Position, 0, 0.23f, 0.25f, value.Loot == LootKind.Egg ? new Color(0.45f, 1f, 0.86f) : value.Loot == LootKind.Heart ? new Color(1f, 0.4f, 0.58f) : new Color(1f, 0.88f, 0.45f));
            else if (value.Kind == CombatEventKind.ChestOpened)
            {
                Spawn(Shape.Ring, value.Position, 0, 0.85f, 0.4f, new Color(1f, 0.79f, 0.3f));
                Spawn(Shape.Star, value.Position, 0, 0.55f, 0.22f, new Color(1f, 0.96f, 0.75f));
            }
            else if (value.Kind == CombatEventKind.Proc)
                Spawn(Shape.Ring, value.Position, 0, value.Cause.ItemId == "chest-thunder" ? 3f : 0.4f, 0.35f, new Color(0.55f, 0.9f, 1f));
            else if (value.Kind == CombatEventKind.Heal)
                Spawn(Shape.Ring, value.Position, 0, 0.35f, 0.28f, new Color(0.35f, 0.92f, 0.75f));
        }

        private void Spawn(Shape shape, Vector2 position, float angle, float radius, float duration, Color color)
        {
            Burst burst = bursts.Find(candidate => candidate.Age >= candidate.Duration);
            if (burst == null)
            {
                if (bursts.Count >= EffectDensity.Limit(96)) return;
                var child = new GameObject("ArcadeCue"); child.transform.SetParent(transform, false);
                burst = new Burst { Transform = child.transform };
                burst.Glow = Part(child.transform, "Glow"); burst.Ring = Part(child.transform, "Ring"); burst.Core = Part(child.transform, "Core");
                for (int i = 0; i < Motes; i++) burst.Motes[i] = Part(child.transform, "Mote" + i);
                bursts.Add(burst);
            }
            // a wide pulse is a soft sound-wave: it needs time to roll out
            if (shape == Shape.Ring && radius > 1.2f) duration = Mathf.Max(duration, 0.7f);
            burst.Shape = shape; burst.Drift = motion; burst.Age = 0; burst.Duration = Mathf.Max(0.08f, duration);
            burst.Radius = radius; burst.Color = color; burst.Angle = angle;
            burst.Mote = CombatVfxArtwork.Frame(theme ?? "AllySpark", 0) ?? CombatVfxArtwork.Frame("AllySpark", 0) ?? softGlow;
            burst.Transform.position = position; burst.Transform.rotation = Quaternion.identity;
            // a pulse or field wider than a body lies on the floor under the brigade; small cues sit over it
            burst.Floor = shape == Shape.Ring && radius > 1.2f || shape == Shape.Puff;
            int order = burst.Floor ? 66 : 2100;
            burst.Glow.sortingOrder = order; burst.Ring.sortingOrder = order + 1; burst.Core.sortingOrder = order + 2;
            seed = unchecked(seed * 1103515245 + 12345);
            for (int i = 0; i < Motes; i++)
            {
                float a = (i + (seed >> 8 & 7) * 0.11f) * Mathf.PI * 2f / Motes + Mathf.Sin(seed + i) * 0.3f;
                burst.Heading[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                burst.Spin[i] = (i % 2 == 0 ? 1 : -1) * (120 + 90 * Mathf.Abs(Mathf.Sin(seed * 0.3f + i)));
                burst.Motes[i].sortingOrder = order + 3;
            }
            Draw(burst);
        }
        private static SpriteRenderer Part(Transform parent, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var view = go.AddComponent<SpriteRenderer>(); view.enabled = false; return view;
        }

        public void Advance(float delta)
        {
            ActiveCount = 0;
            foreach (Burst burst in bursts)
            {
                if (burst.Age >= burst.Duration) continue;
                burst.Age += delta;
                if (burst.Age >= burst.Duration) { Hide(burst); continue; }
                ActiveCount++;
                Draw(burst);
            }
        }
        private static void Hide(Burst burst)
        {
            burst.Core.enabled = burst.Glow.enabled = burst.Ring.enabled = false;
            foreach (SpriteRenderer mote in burst.Motes) mote.enabled = false;
        }
        private static float Ease(float t) => 1 - Mathf.Pow(1 - Mathf.Clamp01(t), 3);
        private static void Fit(SpriteRenderer view, Sprite sprite, Vector2 at, float width, float angle, Color color)
        {
            view.enabled = sprite != null && color.a > 0.004f; if (!view.enabled) return;
            view.sprite = sprite; view.color = color;
            float ink = Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(sprite).width);
            view.transform.position = at; view.transform.rotation = Quaternion.Euler(0, 0, angle);
            view.transform.localScale = Vector3.one * (width / ink);
        }
        private static Sprite Frame(Sprite[] frames, float t) => frames == null || frames.Length == 0 ? null : frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(Mathf.Clamp01(t) * frames.Length))];

        private void Draw(Burst burst)
        {
            float t = Mathf.Clamp01(burst.Age / burst.Duration);
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float flash = PresentationSettings.ReducedFlashes ? 0.4f : 1f;
            Vector2 centre = burst.Transform.position;
            Color tint = burst.Color, light = Color.Lerp(burst.Color, Color.white, 0.3f);
            float r = burst.Radius, fade = Mathf.Pow(1 - t, 1.3f);
            int motes = sparse ? 3 : burst.Shape == Shape.Star ? 4 : burst.Shape == Shape.Slash ? 3 : Motes;
            float moteSize = Mathf.Clamp(0.1f + r * 0.08f, 0.1f, 0.24f);
            Burst b = burst;
            switch (b.Shape)
            {
                case Shape.Ring when b.Floor:
                {
                    // wide pulses (the bell's sound, a field's beat): three thin, faint ripples rolling out one after another,
                    // over the floor and under the bodies, so a big area reads without covering anyone
                    SpriteRenderer[] waves = { b.Ring, b.Glow, b.Core };
                    for (int k = 0; k < waves.Length; k++)
                    {
                        float p = still ? .7f : Mathf.Clamp01((t - k * .2f) / .8f);
                        Fit(waves[k], p <= 0 || p >= 1 ? null : ripple, centre, 2 * r * (.25f + .75f * Ease(p)), 0, new Color(light.r, light.g, light.b, .3f * Mathf.Pow(1 - p, 1.2f) * (k == 0 ? 1 : .7f)));
                    }
                    for (int i = 0; i < b.Motes.Length; i++)
                    {
                        SpriteRenderer mote = b.Motes[i]; if (i >= Mathf.Min(motes, 3) || still) { mote.enabled = false; continue; }
                        Vector2 at = centre + b.Heading[i * 2 % Motes] * r * (.3f + .7f * Ease(t)) + Vector2.up * t * .3f;
                        Fit(mote, b.Mote, at, moteSize * .9f, b.Spin[i] * b.Age * .3f, new Color(1, 1, 1, .55f * Mathf.Sin(t * Mathf.PI)));
                    }
                    break;
                }
                case Shape.Ring:
                {
                    // a flash at the heart, the painted ring opening outward, motes thrown off its rim (or rising, for a heal)
                    Sprite ring = CombatVfxArtwork.Frame("BlastRing", 0) ?? fallbackRing;
                    float open = still ? 1 : 0.35f + 0.65f * Ease(t);
                    Fit(b.Ring, ring, centre, 2 * r * open, still ? 0 : t * 30, new Color(light.r, light.g, light.b, fade * (b.Floor ? 0.6f : 0.85f)));
                    Fit(b.Glow, softGlow, centre, 2 * r * (0.5f + 0.6f * t), 0, new Color(tint.r, tint.g, tint.b, Mathf.Pow(1 - t, 2) * 0.45f * flash));
                    b.Core.enabled = false;
                    for (int i = 0; i < b.Motes.Length; i++)
                    {
                        SpriteRenderer mote = b.Motes[i]; if (i >= motes || still) { mote.enabled = false; continue; }
                        Vector2 at = b.Drift == Drift.Rise ? centre + b.Heading[i] * r * 0.6f * (0.5f + 0.5f * Ease(t)) + Vector2.up * t * (0.5f + r * 0.4f)
                                                           : centre + b.Heading[i] * r * open * (1 + 0.25f * t);
                        Fit(mote, b.Mote, at, moteSize * (1 - 0.4f * t), b.Spin[i] * b.Age, new Color(1, 1, 1, fade));
                    }
                    break;
                }
                case Shape.Star:
                {
                    // the painted sparkle in four frames, opening and turning, with a few motes flicked away
                    Sprite[] frames = library != null ? library.Magic : null;
                    Fit(b.Core, Frame(frames, t), centre, 2.6f * r * (still ? 1 : 0.7f + 0.5f * Ease(t)), b.Angle + (still ? 0 : t * 25), new Color(light.r, light.g, light.b, Mathf.Clamp01((1 - t) * 1.8f) * flash));
                    Fit(b.Glow, softGlow, centre, 2.2f * r, 0, new Color(tint.r, tint.g, tint.b, Mathf.Pow(1 - t, 2) * 0.4f * flash));
                    b.Ring.enabled = false;
                    for (int i = 0; i < b.Motes.Length; i++)
                    {
                        SpriteRenderer mote = b.Motes[i]; if (i >= motes || still) { mote.enabled = false; continue; }
                        Fit(mote, b.Mote, centre + b.Heading[i] * r * (0.6f + 1.4f * Ease(t)), moteSize * 0.8f * (1 - 0.5f * t), b.Spin[i] * b.Age, new Color(1, 1, 1, fade));
                    }
                    break;
                }
                case Shape.Glyph:
                {
                    // a gathering: motes spiral in from all round, the glow swells, the painted ring tightens, then a spark
                    float gather = Ease(t), spiral = still ? 0 : (1 - t) * 160f;
                    Sprite ring = CombatVfxArtwork.Frame("BlastRing", 0) ?? fallbackRing;
                    Fit(b.Ring, ring, centre, 2 * r * (still ? 1 : 1.6f - 0.9f * gather), still ? 0 : -t * 60, new Color(light.r, light.g, light.b, Mathf.Sin(t * Mathf.PI) * 0.7f));
                    Fit(b.Glow, softGlow, centre, 2 * r * (0.4f + 1.1f * gather), 0, new Color(tint.r, tint.g, tint.b, (0.2f + 0.5f * gather) * flash * Mathf.Clamp01((1 - t) * 6)));
                    Sprite[] frames = library != null ? library.Magic : null;
                    Fit(b.Core, t > 0.8f ? Frame(frames, (t - 0.8f) / 0.2f) : null, centre, 2.2f * r, b.Angle, new Color(1, 1, 1, flash));
                    for (int i = 0; i < b.Motes.Length; i++)
                    {
                        SpriteRenderer mote = b.Motes[i]; if (i >= motes) { mote.enabled = false; continue; }
                        Vector2 heading = Quaternion.Euler(0, 0, spiral) * b.Heading[i];
                        Fit(mote, b.Mote, centre + heading * r * 2.2f * (still ? 0.6f : 1 - gather), moteSize * (0.6f + 0.4f * gather), b.Spin[i] * b.Age, new Color(1, 1, 1, Mathf.Clamp01(t * 4) * Mathf.Clamp01((1 - t) * 5)));
                    }
                    break;
                }
                case Shape.Slash:
                {
                    // the painted crescent in four frames along the swing, motes trailing off its tip
                    Sprite[] frames = library != null ? library.Slash : null;
                    Vector2 facing = new Vector2(Mathf.Cos(b.Angle * Mathf.Deg2Rad), Mathf.Sin(b.Angle * Mathf.Deg2Rad));
                    Fit(b.Core, Frame(frames, t), centre + facing * r * 0.45f, 1.7f * r, b.Angle, new Color(light.r, light.g, light.b, Mathf.Clamp01((1 - t) * 2.2f)));
                    b.Glow.enabled = b.Ring.enabled = false;
                    for (int i = 0; i < b.Motes.Length; i++)
                    {
                        SpriteRenderer mote = b.Motes[i]; if (i >= motes || still) { mote.enabled = false; continue; }
                        float sweep = (b.Angle - 60 + 120 * (t * 0.7f + i * 0.15f)) * Mathf.Deg2Rad;
                        Fit(mote, b.Mote, centre + new Vector2(Mathf.Cos(sweep), Mathf.Sin(sweep)) * r * (0.85f + 0.2f * t), moteSize * 0.8f * (1 - t), b.Spin[i] * b.Age, new Color(1, 1, 1, fade));
                    }
                    break;
                }
                default:
                {
                    // a painted ink puff (three frames), drifting up and thinning
                    Sprite[] frames = CombatVfxArtwork.Frames("InkPuff");
                    if (frames == null || frames.Length == 0) frames = library != null ? library.Dust : null;
                    Fit(b.Core, Frame(frames, t), centre + (still ? Vector2.zero : Vector2.up * t * r * 0.5f), 2.2f * r * (still ? 1 : 0.7f + 0.5f * Ease(t)), 0, new Color(tint.r, tint.g, tint.b, (1 - t) * 0.6f * Mathf.Max(0.35f, tint.a)));
                    b.Glow.enabled = b.Ring.enabled = false;
                    foreach (SpriteRenderer mote in b.Motes) mote.enabled = false;
                    break;
                }
            }
        }

        private void OnDestroy() { foreach (Burst burst in bursts) if (burst.Transform != null) Destroy(burst.Transform.gameObject); }
    }
}
