using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum EnemyAffix { None, BlackSmoke, BlueFlame, RedFlame, Blackfire, TwinFlame }
    public enum SpecialEnemyKind { None, SmokeWraith, BlueFlameFiend, RedFlameBerserker, TwinFlameFurnace, AshSwallower, LostLanternSoul, SmokingCoffin, EmberProcession }

    [Serializable]
    public sealed class SpecialEnemyArt
    {
        public string Id;
        public AuthoredMotionSet Normal;
        public AuthoredMotionSet Evolved;
    }

    public sealed class EnemyAffixLibrary : ScriptableObject
    {
        public SpecialEnemyArt[] Species = Array.Empty<SpecialEnemyArt>();
        public Sprite[] Effects = Array.Empty<Sprite>();
        private static EnemyAffixLibrary cached;
        public static EnemyAffixLibrary Current => cached != null ? cached : cached = Resources.Load<EnemyAffixLibrary>("EnemyAffixLibrary");
        public static void ApplyArt(UnitDefinition unit, int wave)
        {
            if (unit.SpecialEnemy == SpecialEnemyKind.None || Current == null) return;
            foreach (SpecialEnemyArt art in Current.Species)
            {
                if (art.Id != unit.Id) continue;
                unit.EvolutionStage = wave >= Mathf.Max(12, unit.UnlockWave + 5) ? 1 : 0;
                unit.AuthoredMotion = unit.EvolutionStage > 0 ? art.Evolved : art.Normal;
                unit.Body = unit.AuthoredMotion.Idle[0];
                unit.AnimationFrames = new[] { unit.Body, unit.AuthoredMotion.Idle[1], unit.AuthoredMotion.Walk[0], unit.AuthoredMotion.Walk[1] };
                if (unit.EvolutionStage > 0) unit.BodyHeight *= 1.1f;
                return;
            }
        }
        public static Color ColorOf(EnemyAffix affix) => affix switch
        {
            EnemyAffix.BlackSmoke => new Color(0.27f, 0.2f, 0.43f),
            EnemyAffix.BlueFlame => new Color(0.22f, 0.82f, 1),
            EnemyAffix.RedFlame => new Color(1, 0.28f, 0.13f),
            EnemyAffix.Blackfire => new Color(0.65f, 0.15f, 0.28f),
            _ => new Color(0.5f, 0.65f, 1)
        };
        public static Sprite Effect(int family, float time, bool reduced = false)
        {
            if (Current == null || Current.Effects.Length != 16) return null;
            return Current.Effects[Mathf.Clamp(family, 0, 3) * 4 + (reduced ? 0 : Mathf.FloorToInt(Mathf.Max(0, time) * 10) % 4)];
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() => cached = null;
    }
}
