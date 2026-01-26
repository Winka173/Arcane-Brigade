using UnityEngine;

namespace ArcaneBrigade
{
    public static class BossPartRules
    {
        public static float Durability(UnitState boss) => boss.Definition.Health * 0.2f;
        public static int BulletCount(UnitState boss) => Mathf.Max(3, CombatSimulation.BossBulletCount(boss.Definition.BossPattern) - (boss.LeftPartBroken ? 4 : 0) - (boss.RightPartBroken ? 2 : 0));
        public static bool Allows(UnitState boss, ExpandedFieldKind kind)
        {
            if (!boss.Definition.IsBoss) return true;
            if (boss.LeftPartBroken && kind == ExpandedFieldKind.Tentacle) return false;
            if (boss.RightPartBroken && (kind == ExpandedFieldKind.FallingProp || kind == ExpandedFieldKind.ShadowPool)) return false;
            return true;
        }
        public static string Description => "Hit from either side to break its organ. Left: fewer shots, no lashes. Right: no props or pools, smaller shockwaves. Both broken: core shockwaves.";
    }

    public sealed partial class CombatSimulation
    {
        private void DamageBossPart(UnitState target, float damage, Faction source, Vector2 direction, CombatCause cause)
        {
            if (!catalog.ExpandedContent || !target.Definition.IsBoss || source == Faction.Enemy || direction.sqrMagnitude < 0.001f) return;
            bool left = direction.x >= 0;
            if (left ? target.LeftPartBroken : target.RightPartBroken) return;
            if (left) target.LeftPartDamage += damage;
            else target.RightPartDamage += damage;
            if ((left ? target.LeftPartDamage : target.RightPartDamage) < BossPartRules.Durability(target)) return;
            if (left) target.LeftPartBrokenAt = Elapsed;
            else target.RightPartBrokenAt = Elapsed;
            target.ExposedUntil = Mathf.Max(target.ExposedUntil, Elapsed + 1.1f);
            target.TelegraphUntil = 0;
            target.BossRecoveryUntil = Mathf.Max(target.BossRecoveryUntil, Elapsed + 0.65f);
            target.NextBossPattern = Mathf.Max(target.NextBossPattern, Elapsed + 0.9f);
            events.Add(new CombatEvent(CombatEventKind.BossPhase, target.Position, Faction.Enemy, value: left ? 1 : 2, direction: left ? Vector2.left : Vector2.right, subjectId: target.Id, subject: target, cause: cause));
        }
    }
}
