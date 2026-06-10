using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How our ranged weapons feel (19/10/2026, requirements R1-R8): one place for the rules the presentation shares.
    /// Heft (how heavy a shot is: a gumball flick to a cannon), which shots are pointy enough to stay stuck in an enemy,
    /// which weapons are guns that eject a casing, which are slow enough to charge, each weapon's own muzzle flourish,
    /// a long-range critical, and the climbing pitch of a piercing shot. Presentation only: the simulation never reads it.</summary>
    public static class RangedFeel
    {
        public enum Flourish { None, Twang, Pop, Steam, Spark, Glint, Bubbles, Notes, Paper, Smoke, Snow, Petal, Swish }

        /// <summary>A critical from further away than this (world units) is a long shot.</summary>
        public const float LongShotDistance = 5f;
        /// <summary>The tiny hold a long-range critical earns (shares CombatFeel's punctuation gap, so it never stutters).</summary>
        public const float LongShotHold = 0.03f;
        /// <summary>Pointy shots kept in one enemy (fewer with Reduced Effects).</summary>
        public const int StuckPerEnemy = 5, ReducedStuckPerEnemy = 2;
        /// <summary>Slow weapons charge: a cooldown of at least this, or a wind-up of at least ChargeWindup.</summary>
        public const float ChargeCooldown = 1.1f, ChargeWindup = 0.25f;

        private static readonly string[] HeavyWords = { "Cannon", "Mortar", "Blunderbuss", "Launcher", "Firework", "Firecracker", "Bazooka", "Brick" };
        private static readonly string[] LightWords = { "Cork", "Popgun", "Straw", "Squirt", "Bubble", "Kazoo", "Gumball", "Dart", "Needle" };
        private static readonly string[] GunWords = { "Pistol", "Rifle", "Cannon", "Gatling", "Blunderbuss", "Popgun", "Blaster", "Launcher", "Mortar", "Gun", "Musket" };

        private sealed class Info
        {
            public string Id; public WeaponMotion Motion; public WeaponKind Kind; public Sprite Shot; public bool ShotResolved;
            public bool Pointy, Gun; public Flourish Look; public float Heft;
        }
        private static readonly Dictionary<WeaponDefinition, Info> cache = new Dictionary<WeaponDefinition, Info>();
        private static readonly List<string> words = new List<string>(8);

        /// <summary>The words of a weapon's name ("KunaiFan" -> Kunai, Fan).</summary>
        private static bool HasWord(WeaponDefinition w, string[] wanted)
        {
            words.Clear();
            Split(w.Id, words);
            foreach (string word in words)
                foreach (string want in wanted)
                    if (string.Equals(word, want, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        private static void Split(string name, List<string> into)
        {
            if (string.IsNullOrEmpty(name)) return;
            int start = 0;
            for (int i = 1; i <= name.Length; i++)
            {
                bool cut = i == name.Length || char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]) || !char.IsLetter(name[i]);
                if (!cut) continue;
                int from = start; while (from < i && !char.IsLetter(name[from])) from++;
                if (i > from) into.Add(name.Substring(from, i - from));
                start = i;
            }
        }
        private static string PaintedKey(WeaponDefinition w)
        {
            if (w == null) return null;
            var probe = new ProjectileState { Weapon = w, Faction = Faction.Companion, Sprite = w.ProjectileSprite, Element = w.Element };
            string key = CombatVfxArtwork.ProjectileKey(probe, null);
            return key == "AllySpark" ? null : key;
        }

        private static Info For(WeaponDefinition w)
        {
            if (cache.TryGetValue(w, out Info info) && info.Id == w.Id && info.Motion == w.Motion && info.Kind == w.Kind) return info;
            if (cache.Count > 512) cache.Clear();
            info = new Info { Id = w.Id, Motion = w.Motion, Kind = w.Kind };
            bool ranged = w.Kind == WeaponKind.Projectile;
            info.Pointy = ranged && ImpactRules.Pointed(w); // the simulation's own rule (it also pins bodies to the edge)
            info.Gun = ranged && w.Motion == WeaponMotion.Recoil && HasWord(w, GunWords);
            info.Heft = ComputeHeft(w);
            info.Look = ComputeFlourish(w, info);
            cache[w] = info;
            return info;
        }

        // ------------------------------------------------------------------ R1 heft and recoil
        /// <summary>How heavy one of this weapon's shots is, 0 (a gumball flick) to 1 (a cannon or a firework): its punch
        /// (damage per volley), its bulk (shot size), how slow it is, and a nudge from what it is called. 0 for melee.</summary>
        public static float Heft(WeaponDefinition w) => w == null || w.Kind != WeaponKind.Projectile ? 0 : For(w).Heft;
        private static float ComputeHeft(WeaponDefinition w)
        {
            if (w.Kind != WeaponKind.Projectile) return 0;
            float punch = Mathf.InverseLerp(4f, 36f, w.Damage * Mathf.Sqrt(Mathf.Max(1, w.ProjectileCount)));
            float bulk = Mathf.Max(Mathf.InverseLerp(.05f, .2f, w.ProjectileRadius), Mathf.InverseLerp(.22f, .8f, w.ProjectileVisualWidth));
            float slow = Mathf.InverseLerp(.3f, 1.6f, w.Cooldown);
            float named = HasWord(w, HeavyWords) ? .25f : HasWord(w, LightWords) ? -.15f : 0;
            return Mathf.Clamp01(.4f * punch + .2f * bulk + .4f * slow + named);
        }
        /// <summary>How far the weapon kicks back, as a multiple of its authored RecoilDistance.</summary>
        public static float RecoilScale(WeaponDefinition w) => Mathf.Lerp(.6f, 2.4f, Heft(w));
        /// <summary>How far the barrel climbs (degrees) as the shot leaves.</summary>
        public static float TiltDegrees(WeaponDefinition w) => Mathf.Lerp(4f, 22f, Heft(w));
        /// <summary>The holder's own shove backwards (world units) at this moment of the attack: sharp on release, settling.</summary>
        public static float BodyKick(WeaponDefinition w, float age, float windup)
        {
            if (w == null || w.Kind != WeaponKind.Projectile || age < windup) return 0;
            float after = age - windup, settle = Mathf.Max(.12f, w.Recovery);
            if (after >= settle) return 0;
            return Heft(w) * .07f * Mathf.Clamp01(after / .03f) * (1 - Mathf.SmoothStep(0, 1, after / settle));
        }

        // ------------------------------------------------------------------ R3 guns, R4 charge, R5 pointy shots
        /// <summary>A gun: a Recoil weapon named as one (pistol, rifle, cannon, gatling, blaster, launcher, mortar...), not a thrown object.</summary>
        public static bool IsGunLike(WeaponDefinition w) => w != null && w.Kind == WeaponKind.Projectile && For(w).Gun;
        /// <summary>Arrows, darts, kunai, quills, needles, pins and harpoons (ImpactRules.Pointed, the simulation's rule):
        /// these stay stuck in the enemy they hit.</summary>
        public static bool IsPointy(WeaponDefinition w) => w != null && w.Kind == WeaponKind.Projectile && For(w).Pointy;
        /// <summary>Slow ranged weapons show a charge through their wind-up.</summary>
        public static bool IsSlow(WeaponDefinition w) => w != null && w.Kind == WeaponKind.Projectile && (w.Cooldown >= ChargeCooldown || w.Windup >= ChargeWindup);
        /// <summary>How much further a slow bow draws its string (1 for a quick one).</summary>
        public static float BowDraw(WeaponDefinition w) => IsSlow(w) ? Mathf.Lerp(1.15f, 1.55f, Mathf.InverseLerp(ChargeCooldown, 1.8f, w.Cooldown)) : 1f;
        /// <summary>The shot's own picture (its painted Shot_ art, its painted shared shot, or its projectile sprite).</summary>
        public static Sprite ShotSprite(WeaponDefinition w)
        {
            if (w == null) return null;
            Info info = For(w);
            if (info.ShotResolved) return info.Shot;
            Sprite shot = ArsenalArtwork.Frame("Shot_" + w.Id, 0);
            if (shot == null) { string key = PaintedKey(w); if (key != null) shot = CombatVfxArtwork.Frame(key, 0); }
            if (shot == null) shot = w.ProjectileSprite;
            info.Shot = shot; info.ShotResolved = Application.isPlaying; // edit-time fixtures may load art later
            return shot;
        }
        /// <summary>The little thing a gun throws out of its breech: a cork, a gumball, a brass casing, an ink drop.</summary>
        public static (InkToken token, InkPalette palette, float squash) Casing(WeaponDefinition w)
        {
            string id = w != null ? w.Id ?? "" : "";
            if (id.Contains("Cork")) return (InkToken.Patch, ArsenalInk.Leather, .8f);
            if (id.Contains("Gumball")) return (InkToken.Button, ArsenalInk.Candy, 1f);
            if (id.Contains("Confetti")) return (InkToken.Page, ArsenalInk.Candy, .9f);
            if (id.Contains("Ink")) return (InkToken.Drop, ArsenalInk.Midnight, 1f);
            if (id.Contains("Mailbox")) return (InkToken.Envelope, ArsenalInk.Cream, .9f);
            if (id.Contains("Scoop")) return (InkToken.Drop, ArsenalInk.Candy, 1f);
            if (id.Contains("Saucer")) return (InkToken.Bolt, ArsenalInk.Cyan, .8f);
            if (id.Contains("Cuckoo")) return (InkToken.Petal, ArsenalInk.Cream, .7f);
            return (InkToken.Button, ArsenalInk.Gold, .5f); // a brass shell, squashed long
        }

        // ------------------------------------------------------------------ R2 muzzle flourishes
        /// <summary>Arsenal weapons whose attack already has its own ink moment in ArsenalVisual (no second flourish).</summary>
        public static bool HasArsenalFireMoment(ArsenalWeapon w) => w == ArsenalWeapon.CorkRifle || w == ArsenalWeapon.ConfettiBlunderbuss || w == ArsenalWeapon.FanfareHorn
            || w == ArsenalWeapon.SquirtTrio || w == ArsenalWeapon.GlitchToybox || w == ArsenalWeapon.WateringCan;
        /// <summary>Each ranged weapon's own small flourish at the muzzle (None for melee and for Arsenal weapons that have a moment).</summary>
        public static Flourish FlourishFor(WeaponDefinition w) => w == null || w.Kind != WeaponKind.Projectile ? Flourish.None : For(w).Look;
        private static Flourish ComputeFlourish(WeaponDefinition w, Info info)
        {
            if (w.Kind != WeaponKind.Projectile || HasArsenalFireMoment(w.Arsenal)) return Flourish.None;
            string id = w.Id ?? "";
            bool Has(params string[] keys) { foreach (string k in keys) if (id.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) return true; return false; }
            if (w.Motion == WeaponMotion.Bow || w.Family == ExpandedWeapon.TetherBow) return Flourish.Twang;
            if (w.Family == ExpandedWeapon.Tea || Has("Straw", "Teapot", "Kettle", "Fizz", "Extinguisher")) return Flourish.Steam;
            if (Has("Bubble")) return Flourish.Bubbles;
            if (w.Motion == WeaponMotion.Music || w.Family == ExpandedWeapon.MusicBox || Has("Harp", "Kazoo", "Horn", "Music", "Singer")) return Flourish.Notes;
            if (Has("Mailbox", "Paper", "Kite", "Plane")) return Flourish.Paper;
            if (Has("Rose", "Bloom", "Leaf", "Petal", "Acorn")) return Flourish.Petal;
            if (Has("Cork", "Popgun", "Popper", "Gumball", "Plunger", "Cherry")) return Flourish.Pop;
            if (w.Element == DamageElement.Frost && (w.Motion == WeaponMotion.Cast || Has("Snow", "Globe", "Frost"))) return Flourish.Snow;
            if (info.Pointy) return Flourish.Glint;
            if (w.Motion == WeaponMotion.Cast || w.Motion == WeaponMotion.Bell || w.Motion == WeaponMotion.Camera) return Flourish.Spark;
            if (info.Gun) return Flourish.Smoke;
            return Flourish.Swish; // a thrown object leaves the hand with a swish
        }
        /// <summary>Our side's flourish colours by element: never ember-red (an ember weapon sparks gold).</summary>
        public static InkPalette ElementPalette(DamageElement element) => element switch
        {
            DamageElement.Ember => ArsenalInk.Gold, DamageElement.Frost => ArsenalInk.Glass, DamageElement.Arcane => ArsenalInk.Violet, _ => ArsenalInk.Cream
        };

        // ------------------------------------------------------------------ R8 long-range crits
        /// <summary>The simulation the arena is showing (set by RangedFeelVisual) so a shooter can be found when it is not drawn.</summary>
        public static CombatSimulation Simulation { get; set; }
        private static bool LongShotCandidate(CombatEvent v) => v.Kind == CombatEventKind.Hit && v.Critical && v.Value > 0 && v.Faction == Faction.Enemy
            && v.Weapon != null && v.Weapon.Kind == WeaponKind.Projectile && v.Cause.SourceUnitId != 0;
        /// <summary>A critical hit on an enemy by one of our shots fired from far away (> LongShotDistance).</summary>
        public static bool LongShot(CombatEvent v, Vector2 shooter) => LongShotCandidate(v) && (shooter - v.Position).sqrMagnitude > LongShotDistance * LongShotDistance;
        public static bool LongShot(CombatEvent v) => LongShotCandidate(v) && TryShooter(v.Cause.SourceUnitId, out Vector2 at) && LongShot(v, at);
        public static bool TryShooter(int unitId, out Vector2 at)
        {
            if (UnitVisual.TryDrawn(unitId, out at)) return true;
            UnitState unit = Simulation != null ? Simulation.UnitForPresentation(unitId) : null;
            at = unit != null ? unit.Position : default;
            return unit != null;
        }
        /// <summary>The struck target's tip-back over its 0.3 s: 0..1, sharp then easing home.</summary>
        public static float TipBack(float age) => age < 0 || age >= .3f ? 0 : Mathf.Clamp01(age / .04f) * (1 - Mathf.SmoothStep(0, 1, age / .3f));

        // ------------------------------------------------------------------ R6 pierce pitch
        private struct Chain { public int Step; public float At; }
        private static readonly Dictionary<int, Chain> pierceAudio = new Dictionary<int, Chain>();
        /// <summary>Each successive pierce of one shot sounds a whole tone higher (capped at about an octave).</summary>
        public static float PiercePitch(CombatEvent v, float now)
        {
            int root = v.Cause.RootId;
            if (root == 0) return 1f;
            int step = pierceAudio.TryGetValue(root, out Chain c) && now - c.At < 1.2f ? c.Step + 1 : 0;
            if (pierceAudio.Count > 256) pierceAudio.Clear();
            pierceAudio[root] = new Chain { Step = step, At = now };
            return PitchForStep(step);
        }
        public static float PitchForStep(int step) => Mathf.Min(1.9f, Mathf.Pow(2f, Mathf.Max(0, step) * 2f / 12f));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() { cache.Clear(); pierceAudio.Clear(); Simulation = null; }
    }
}
