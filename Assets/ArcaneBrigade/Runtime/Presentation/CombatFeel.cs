using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public readonly struct ImpactAccent
    {
        public readonly float Hold;
        public readonly float Camera;
        public ImpactAccent(float hold, float camera) { Hold = hold; Camera = camera; }
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

    /// <summary>Game feel for the whole screen: trauma-based camera shake that adds up and decays, a short global hit-stop
    /// (the simulation simply does not step, so gameplay and Time.timeScale stay untouched), and a slow-motion beat when a boss falls.</summary>
    public sealed class CombatFeel
    {
        public const float MaxShake = 0.3f;          // world units at full trauma (about 40 px at 1080p)
        private const float RoutineTraumaCap = 0.42f; // kills, crits and heavy hits alone never shake harder than this
        private float trauma, clock, freeze, freezeCooldown, slow, flash, punctuation;
        /// <summary>How long a critical hit or a big kill holds the whole screen (18/10/2026), and how soon another may.</summary>
        public const float CriticalHold = 0.035f, BigKillHold = 0.055f, PunctuationGap = 0.9f;
        /// <summary>0..1 full-screen flash when a boss falls (drawn by the HUD; Reduced Flashes softens it).</summary>
        public float ScreenFlash => PresentationSettings.ReducedFlashes ? flash * 0.25f : flash;
        private Vector2 bias = Vector2.up;
        public float Trauma => trauma;
        /// <summary>True while the screen is held on an impact; the arena does not step the simulation.</summary>
        public bool Frozen => freeze > 0;
        public bool SlowMotion => slow > 0;
        /// <summary>How fast presentation clocks (effects, numbers, deaths) run: nearly still during a hold, a third in slow motion.</summary>
        public float PresentationSpeed => Frozen ? 0.15f : SlowMotion ? 0.35f : 1f;
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
        public void Clear() { trauma = freeze = freezeCooldown = slow = flash = punctuation = 0; }
        public void Advance(float delta)
        {
            clock += delta; trauma = Mathf.Max(0, trauma - delta * 1.8f);
            freeze = Mathf.Max(0, freeze - delta); freezeCooldown = Mathf.Max(0, freezeCooldown - delta); punctuation = Mathf.Max(0, punctuation - delta); slow = Mathf.Max(0, slow - delta); flash = Mathf.Max(0, flash - delta * 3.2f);
        }
        private void AddTrauma(float amount, bool routine) { trauma = routine ? Mathf.Max(trauma, Mathf.Min(RoutineTraumaCap, trauma + amount)) : Mathf.Min(1f, trauma + amount); }
        private void Hold(float seconds)
        {
            if (PresentationSettings.ReducedMotion || freezeCooldown > 0 || seconds <= 0) return;
            freeze = Mathf.Min(0.12f, Mathf.Max(freeze, seconds)); freezeCooldown = freeze + 0.5f;
        }
        /// <summary>A big enemy: the kind whose fall deserves a held beat (large bodies such as the crab, the toad, the coffin).</summary>
        public static bool BigEnemy(UnitState unit) => unit != null && unit.Definition != null && !unit.Definition.IsBoss && unit.Faction == Faction.Enemy && unit.Definition.Radius >= 0.4f;
        /// <summary>A critical hit that earns a held beat: the hero's own, or one that takes a real bite (6% or more) out of its target.</summary>
        public static bool HeavyCritical(CombatEvent value, int championId) => value.Kind == CombatEventKind.Hit && value.Critical && value.Value > 0 && value.Faction == Faction.Enemy
            && (championId != 0 && value.Cause.SourceUnitId == championId || value.Subject != null && value.Subject.Definition != null && value.Value >= value.Subject.Definition.Health * 0.06f);

        public void Play(IReadOnlyList<CombatEvent> events, Dictionary<int, UnitVisual> views, float time, int championId = 0)
        {
            float hold = 0, punctuate = 0; int kills = 0;
            foreach (CombatEvent value in events)
            {
                ImpactAccent accent = ImpactAccent.For(value);
                if (accent.Hold > 0 && views.TryGetValue(value.SubjectId, out UnitVisual visual)) visual.HoldImpact(time, accent.Hold);
                if (accent.Hold > 0.02f && value.Faction != Faction.Champion && value.Weapon != null && views.TryGetValue(value.Cause.SourceUnitId, out UnitVisual attacker)) attacker.HoldImpact(time, accent.Hold * 0.55f);
                bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
                Vector2 direction = value.Direction.sqrMagnitude > 0 ? value.Direction.normalized : Vector2.up;
                switch (value.Kind)
                {
                    case CombatEventKind.Kill when boss: AddTrauma(0.9f, false); hold = Mathf.Max(hold, 0.12f); if (!PresentationSettings.ReducedMotion) slow = 0.9f; flash = 1f; bias = direction; break;
                    case CombatEventKind.Kill when value.Faction == Faction.Enemy:
                        kills++; AddTrauma(value.Critical || value.Value > 0.6f ? 0.09f : 0.03f, true);
                        if (BigEnemy(value.Subject)) { punctuate = Mathf.Max(punctuate, BigKillHold); AddTrauma(0.14f, true); bias = direction; }
                        break;
                    case CombatEventKind.BossLanded: AddTrauma(0.55f, false); hold = Mathf.Max(hold, 0.06f); bias = Vector2.down; break;
                    case CombatEventKind.Detonation when value.Cause.ItemId == "ink-bloater": AddTrauma(0.08f, true); break; // chains rattle, they do not freeze
                    case CombatEventKind.Flare: AddTrauma(0.45f, false); hold = Mathf.Max(hold, 0.08f); flash = Mathf.Max(flash, 0.35f); break;
                    case CombatEventKind.Detonation when value.Cause.ItemId != null && value.Cause.ItemId.StartsWith("hero-identity-"): AddTrauma(0.07f, true); break; // the hero's own blasts rattle, they do not shake the room
                    // friends' bombs and bursts rattle the screen a little and never hold it: a brigade of bombers would
                    // otherwise shake and stutter the whole fight (18/10/2026); the hero's own blasts still land as a beat
                    case CombatEventKind.Detonation when value.Faction == Faction.Companion: AddTrauma(Mathf.Clamp(value.Value * 0.07f, 0.04f, 0.12f), true); break;
                    case CombatEventKind.Detonation: AddTrauma(Mathf.Clamp(value.Value * 0.16f, 0.14f, 0.34f), false); hold = Mathf.Max(hold, 0.045f); break;
                    // the moment a hero transforms is the beat of the fight
                    case CombatEventKind.HeroIdentityCast when value.Value >= 1: AddTrauma(0.32f, false); hold = Mathf.Max(hold, 0.07f); bias = Vector2.up; break;
                    // a royal blow, a whale's spout or a gale: big single moments of the new relics and friends
                    case CombatEventKind.Proc when value.Cause.ItemId == "curio-royal" || value.Cause.ItemId == "curio-quilt": AddTrauma(0.16f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Faction == Faction.Champion:
                        // being hurt shakes hard, but a swarm of small bites cannot keep the screen at full tilt; only real blows hold
                        trauma = Mathf.Max(trauma, Mathf.Min(0.62f, trauma + 0.24f)); bias = direction;
                        if (value.Subject != null && value.Subject.Definition != null && value.Value >= value.Subject.Definition.Health * 0.08f) hold = Mathf.Max(hold, 0.05f);
                        break;
                    case CombatEventKind.Hit when value.Value > 0 && boss: AddTrauma(value.Critical ? 0.08f : 0.04f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam): AddTrauma(0.1f, true); break;
                    case CombatEventKind.Hit when value.Value > 0 && value.Critical: AddTrauma(0.05f, true); break;
                }
                if (HeavyCritical(value, championId)) punctuate = Mathf.Max(punctuate, CriticalHold);
            }
            if (kills >= 6) { AddTrauma(0.12f, true); hold = Mathf.Max(hold, 0.05f); }  // a big multi-kill lands as one beat
            // crits and big kills hold the whole screen for a blink, but never more often than once every PunctuationGap
            // seconds, so a brigade full of crits punctuates the fight instead of stuttering it
            if (punctuate > 0 && punctuation <= 0 && !PresentationSettings.ReducedMotion) { hold = Mathf.Max(hold, punctuate); punctuation = PunctuationGap; }
            Hold(hold);
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
