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

    public sealed class CombatFeel
    {
        private float age = 1;
        private float amplitude;
        private float cooldown;
        private Vector2 direction;
        public Vector2 CameraOffset => PresentationSettings.ReducedMotion || PresentationSettings.ReducedShake || age >= 0.16f ? Vector2.zero : direction * amplitude * Mathf.Sin(age * 65) * Mathf.Pow(1 - age / 0.16f, 2);
        public void Clear() { age = 1; amplitude = cooldown = 0; }
        public void Advance(float delta) { age += delta; cooldown = Mathf.Max(0, cooldown - delta); }
        public void Play(IReadOnlyList<CombatEvent> events, Dictionary<int, UnitVisual> views, float time)
        {
            float strongest = 0; Vector2 push = Vector2.right;
            foreach (CombatEvent value in events)
            {
                ImpactAccent accent = ImpactAccent.For(value);
                if (accent.Hold > 0 && views.TryGetValue(value.SubjectId, out UnitVisual visual)) visual.HoldImpact(time, accent.Hold);
                if (accent.Hold > 0.02f && value.Faction != Faction.Champion && value.Weapon != null && views.TryGetValue(value.Cause.SourceUnitId, out UnitVisual attacker)) attacker.HoldImpact(time, accent.Hold * 0.55f);
                if (accent.Camera > strongest) { strongest = accent.Camera; push = value.Direction.sqrMagnitude > 0 ? value.Direction.normalized : Vector2.up; }
            }
            if (cooldown > 0 || strongest <= 0) return;
            amplitude = strongest; direction = push; age = 0; cooldown = 0.25f;
        }
    }

    public static class EffectDensity
    {
        public static float CrowdPressure { get; private set; }
        public static void SetSceneLoad(int projectiles, int units) { CrowdPressure = Mathf.Clamp01(Mathf.Max(0, projectiles - 40) / 160f + Mathf.Max(0, units - 35) / 160f); }
        public static int Limit(int full) => PresentationSettings.ReducedEffects ? Mathf.Max(4, full / 3) : Mathf.Max(4, Mathf.RoundToInt(full * Mathf.Lerp(1, 0.6f, CrowdPressure)));
        public static bool CosmeticProjectile(int identity) => PresentationSettings.ReducedEffects ? identity % 3 == 0 : CrowdPressure < 0.5f || identity % 2 == 0;
        public static float AlliedAlpha => (PresentationSettings.ReducedEffects || PresentationSettings.HighReadability ? 0.42f : 0.72f) * Mathf.Lerp(1, 0.55f, CrowdPressure);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { CrowdPressure = 0; }
    }
}
