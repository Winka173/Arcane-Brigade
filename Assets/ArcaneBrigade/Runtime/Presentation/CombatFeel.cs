using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public readonly struct ImpactAccent
    {
        /// <summary>Always 0 now: nothing holds a frame for a hit (play-test 20/10/2026: players read hit stop as lag).</summary>
        public readonly float Hold;
        public readonly float Camera;
        public ImpactAccent(float hold, float camera) { Hold = 0; Camera = camera; }
        public static ImpactAccent For(CombatEvent value)
        {
            bool boss = value.Subject != null && value.Subject.Definition.IsBoss;
            if (value.Kind == CombatEventKind.BossLanded || value.Kind == CombatEventKind.Kill && boss) return new ImpactAccent(0.045f, 0.09f);
            if (value.Kind == CombatEventKind.Kill) return new ImpactAccent(value.Critical ? 0.024f : 0.012f, value.Critical ? 0.018f : 0);
            if (value.Kind != CombatEventKind.Hit || value.Value <= 0) return default;
            if (value.Faction == Faction.Champion) return new ImpactAccent(0.035f, 0.05f);
            bool heavy = value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Mechanic == CarnivalWeapon.SpringHammer);
            return new ImpactAccent(heavy ? 0.035f : value.Critical ? 0.028f : 0.016f, heavy ? 0.028f : value.Critical ? 0.02f : 0);
        }
    }

    /// <summary>Game feel for the whole screen: trauma-based camera shake that adds up and decays, and one slow-motion beat
    /// (half a second) when the final boss of the journey falls. No hit stop anywhere (play-test 20/10/2026): the simulation
    /// never pauses for a hit or a kill; flashes, shake, impact words and knockback carry the weight instead.</summary>
    public sealed class CombatFeel
    {
        public const float MaxShake = 0.18f;         // world units at full trauma (about 24 px at 1080p; was 0.3: too much shaking, requirement 19/10/2026)
        private const float RoutineTraumaCap = 0.3f;  // kills, crits and heavy hits alone never shake harder than this (was 0.42)
        private float trauma, clock, slow, flash;
        /// <summary>The only time the fight slows: the wave-20 boss falls (seconds of real time, at SlowMotionSpeed).</summary>
        public const float FinalBossSlowMotion = 0.5f, SlowMotionSpeed = 0.35f, AwakenBeat = 0.14f, BossArrivalBeat = 0.3f, ComboBeat = 0.12f;
        /// <summary>0..1 full-screen flash when a boss falls (drawn by the HUD; Reduced Flashes softens it).</summary>
        public float ScreenFlash => PresentationSettings.ReducedFlashes ? flash * 0.25f : flash;
        private Vector2 bias = Vector2.up;
        public float Trauma => trauma;
        /// <summary>Kept for old callers: the screen is never held any more.</summary>
        public bool Frozen => false;
        public bool SlowMotion => slow > 0;
        /// <summary>How fast presentation clocks (effects, numbers, deaths) run: a third during the final boss's slow motion.</summary>
        public float PresentationSpeed => SlowMotion ? SlowMotionSpeed : 1f;
        public Vector2 CameraOffset
        {
            get
            {
                if (PresentationSettings.ReducedMotion || PresentationSettings.ReducedShake || PresentationSettings.ShakeScale <= 0 || trauma <= 0.001f) return Vector2.zero;
                float x = Mathf.PerlinNoise(clock * 22f, 0.37f) * 2 - 1, y = Mathf.PerlinNoise(1.91f, clock * 22f) * 2 - 1;
                Vector2 shake = new Vector2(x, y) * 0.75f + bias * Mathf.Sin(clock * 55f) * 0.35f;
                return Vector2.ClampMagnitude(shake, 1f) * MaxShake * Mathf.Pow(trauma, 1.5f) * PresentationSettings.ShakeScale;
            }
        }
        public void Clear() { trauma = slow = flash = 0; }
        /// <summary>A rules moment with weight (a capstone, a shatter, an enemy blow landing, a boss phase): a shake and an optional
        /// soft flash; never a held frame. Reduced Shake / Reduced Flashes still apply through CameraOffset and ScreenFlash.</summary>
        public void Accent(float shake, float flashAmount) { AddTrauma(shake, shake < RoutineTraumaCap); flash = Mathf.Max(flash, flashAmount); }
        public void Advance(float delta)
        {
            clock += delta; trauma = Mathf.Max(0, trauma - delta * 1.8f);
            slow = Mathf.Max(0, slow - delta); flash = Mathf.Max(0, flash - delta * 3.2f);
        }
        private void AddTrauma(float amount, bool routine) { trauma = routine ? Mathf.Max(trauma, Mathf.Min(RoutineTraumaCap, trauma + amount)) : Mathf.Min(1f, trauma + amount); }
        /// <summary>A big enemy: the kind whose fall deserves a stronger shake (large bodies such as the crab, the toad, the coffin).</summary>
        public static bool BigEnemy(UnitState unit) => unit != null && unit.Definition != null && !unit.Definition.IsBoss && unit.Faction == Faction.Enemy && unit.Definition.Radius >= 0.4f;
        /// <summary>A critical hit that lands as a beat: the hero's own, or one that takes a real bite (6% or more) out of its target.</summary>
        public static bool HeavyCritical(CombatEvent value, int championId) => value.Kind == CombatEventKind.Hit && value.Critical && value.Value > 0 && value.Faction == Faction.Enemy
            && (championId != 0 && value.Cause.SourceUnitId == championId || value.Subject != null && value.Subject.Definition != null && value.Value >= value.Subject.Definition.Health * 0.06f);

        private readonly Dictionary<int, int> swingHits = new Dictionary<int, int>();
        /// <param name="finalBoss">The fight is the journey's last boss wave: its boss's fall plays in slow motion.</param>
        public void Play(IReadOnlyList<CombatEvent> events, Dictionary<int, UnitVisual> views, float time, int championId = 0, bool finalBoss = false)
        {
            int kills = 0;
            swingHits.Clear();
            foreach (CombatEvent value in events)
                if (value.Kind == CombatEventKind.Hit && value.Faction == Faction.Enemy && value.Value > 0 && value.Weapon != null && value.Weapon.Kind == WeaponKind.Melee && value.Cause.RootId != 0)
                    swingHits[value.Cause.RootId] = swingHits.TryGetValue(value.Cause.RootId, out int n) ? n + 1 : 1;
            int widest = 0; foreach (int n in swingHits.Values) widest = Mathf.Max(widest, n);
            if (widest >= 3) AddTrauma(.03f * Mathf.Min(5, widest) / 5f, true);
            foreach (CombatEvent value in events)
            {
                if (value.Kind == CombatEventKind.Knockback && value.Value >= .5f && value.Subject != null && value.Subject.Alive && !value.Subject.Definition.IsBoss
                    && value.Subject.Definition.BodyHeight < .95f && value.Subject.Definition.KnockbackResistance < .1f && views.TryGetValue(value.SubjectId, out UnitVisual rolled))
                    rolled.Tumble(time, value.Direction);
                bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
                Vector2 direction = value.Direction.sqrMagnitude > 0 ? value.Direction.normalized : Vector2.up;
                switch (value.Kind)
                {
                    case CombatEventKind.Kill when boss: AddTrauma(0.9f, false); if (finalBoss && !PresentationSettings.ReducedMotion) slow = FinalBossSlowMotion; flash = 1f; bias = direction; break;
                    case CombatEventKind.Kill when value.Faction == Faction.Enemy:
                        kills++; AddTrauma(value.Critical || value.Value > 0.6f ? 0.09f : 0.03f, true);
                        if (BigEnemy(value.Subject)) { AddTrauma(0.14f, true); bias = direction; }
                        break;
                    case CombatEventKind.BodyImpact: AddTrauma(0.06f, true); break;
                    // a combo of four or five friends lands as a beat: a short catch and a jolt
                    case CombatEventKind.ComboNamed when Combos.Of((ComboId)Mathf.RoundToInt(value.Value)).Members.Length >= 4: AddTrauma(0.22f, false); if (!PresentationSettings.ReducedMotion) slow = Mathf.Max(slow, ComboBeat); break; // a flung body bowls into another
                    case CombatEventKind.WallSplat: AddTrauma(0.09f, true); break;
                    // a boss arriving lands as a beat: the night holds its breath for a moment as it touches down
                    case CombatEventKind.BossLanded: AddTrauma(0.55f, false); bias = Vector2.down; if (!PresentationSettings.ReducedMotion) slow = Mathf.Max(slow, BossArrivalBeat); break;
                    case CombatEventKind.Detonation when value.Cause.ItemId == "ink-bloater": AddTrauma(0.08f, true); break; // chains rattle, they do not freeze
                    case CombatEventKind.Flare: AddTrauma(0.45f, false); flash = Mathf.Max(flash, 0.35f); break;
                    case CombatEventKind.Detonation when value.Cause.ItemId != null && value.Cause.ItemId.StartsWith("hero-identity-"): AddTrauma(0.07f, true); break; // the hero's own blasts rattle, they do not shake the room
                    // friends' bombs and bursts rattle the screen a little and never hold it: a brigade of bombers would
                    // otherwise shake and stutter the whole fight (18/10/2026); the hero's own blasts still land as a beat
                    case CombatEventKind.Detonation when value.Faction == Faction.Companion: AddTrauma(Mathf.Clamp(value.Value * 0.07f, 0.04f, 0.12f), true); break;
                    case CombatEventKind.Detonation: AddTrauma(Mathf.Clamp(value.Value * 0.16f, 0.14f, 0.34f), false); break;
                    // the moment a hero transforms is the beat of the fight
                    // requirement 23/10/2026: the transformation lands as a beat - the action catches for an instant and the screen jolts
                    case CombatEventKind.HeroIdentityCast when value.Value >= 1: AddTrauma(0.42f, false); bias = Vector2.up; if (!PresentationSettings.ReducedMotion) slow = Mathf.Max(slow, AwakenBeat); break;
                    // a royal blow, a whale's spout or a gale: big single moments of the new relics and friends
                    case CombatEventKind.Proc when value.Cause.ItemId == "curio-royal" || value.Cause.ItemId == "curio-quilt": AddTrauma(0.16f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Faction == Faction.Champion:
                        // being hurt shakes hard, but a swarm of small bites cannot keep the screen at full tilt
                        trauma = Mathf.Max(trauma, Mathf.Min(0.62f, trauma + 0.24f)); bias = direction;
                        break;
                    case CombatEventKind.Hit when value.Value > 0 && boss: AddTrauma(value.Critical ? 0.08f : 0.04f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam): AddTrauma(0.1f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Critical: AddTrauma(0.05f, true); break;
                }
                if (HeavyCritical(value, championId)) AddTrauma(0.06f, true);
                // a critical from far away (RangedFeel.LongShot): the target tips back along the shot (no held frame)
                if (value.Kind == CombatEventKind.Hit && value.Critical && RangedFeel.LongShot(value))
                {
                    if (views.TryGetValue(value.SubjectId, out UnitVisual struck)) struck.TipBack(time, value.Direction);
                    ImpactWords.Pop("CRIT!", value.Position + Vector2.up * .6f, ImpactWords.Cream, 1, .8f, -12);
                }
            }
            if (kills >= 6) AddTrauma(0.12f, true);  // a big multi-kill lands as one shake
        }
    }

    public static class EffectDensity
    {
        public static float CrowdPressure { get; private set; }
        public static void SetSceneLoad(int projectiles, int units) { CrowdPressure = Mathf.Clamp01(Mathf.Max(0, projectiles - 40) / 160f + Mathf.Max(0, units - 35) / 160f); }
        public static int Limit(int full) => PresentationSettings.ReducedEffects ? Mathf.Max(4, full / 3) : Mathf.Max(4, Mathf.RoundToInt(full * Mathf.Lerp(1, 0.6f, CrowdPressure)));
        public static bool CosmeticProjectile(int identity) => PresentationSettings.ReducedEffects ? identity % 3 == 0 : CrowdPressure < 0.5f || identity % 2 == 0;
        public static float AlliedAlpha => (PresentationSettings.ReducedEffects || PresentationSettings.HighReadability ? 0.42f : 0.8f) * Mathf.Lerp(1, 0.82f, CrowdPressure);
        /// <summary>Area effects that matter (blasts, fields that deal damage) never fade below this, however busy the fight.</summary>
        public static float AreaAlpha => Mathf.Max(PresentationSettings.HighReadability ? 0.55f : 0.7f, AlliedAlpha);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { CrowdPressure = 0; }
    }
}
