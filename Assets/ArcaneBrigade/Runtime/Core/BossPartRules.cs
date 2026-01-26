using UnityEngine;

namespace ArcaneBrigade
{
    public static class BossPartRules
    {
        public static float Damage(UnitState boss, int part) => part == 0 ? boss.LeftPartDamage : part == 1 ? boss.RightPartDamage : boss.CenterPartDamage;
        public static float BrokenAt(UnitState boss, int part) => part == 0 ? boss.LeftPartBrokenAt : part == 1 ? boss.RightPartBrokenAt : boss.CenterPartBrokenAt;
        public static bool Broken(UnitState boss, int part) => BrokenAt(boss, part) >= 0;
        public static float Durability(UnitState boss) => Durability(boss, 0);
        public static float Durability(UnitState boss, int part)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            return boss.Definition.Health * (entry == null ? 0.2f : entry.Organs[part].Durability);
        }
        public static int HitZone(Vector2 direction) => Mathf.Abs(direction.y) > Mathf.Abs(direction.x) * 1.3f ? 2 : direction.x >= 0 ? 0 : 1;
        public static bool Has(UnitState boss, BossOrganRole role, bool broken)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            if (entry == null) return false;
            for (int part = 0; part < entry.Organs.Length; part++)
                if ((entry.Organs[part].Role & role) != 0 && Broken(boss, part) == broken) return true;
            return false;
        }
        public static int BulletCount(UnitState boss)
        {
            int count = CombatSimulation.BossBulletCount(boss.Definition.BossPattern);
            if (BossAnatomy.For(boss.Definition) == null) return Mathf.Max(3, count - (boss.LeftPartBroken ? 4 : 0) - (boss.RightPartBroken ? 2 : 0));
            return Has(boss, BossOrganRole.Volley, true) || boss.LeftPartBroken && boss.RightPartBroken ? Mathf.Max(3, Mathf.CeilToInt(count * 0.4f)) : count;
        }
        public static int NextOrgan(UnitState boss)
        {
            for (int step = 0; step < 3; step++)
            {
                int part = (boss.BossVolley + step) % 3;
                if (!Broken(boss, part)) return part;
            }
            return -1;
        }
        public static Vector2 Position(UnitState boss, int part)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            return entry == null ? boss.Position : boss.Position + entry.Organs[part].Offset * boss.Definition.BodyHeight;
        }
        public static Vector2 VolleyOrigin(UnitState boss)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            if (entry != null)
                for (int part = 0; part < entry.Organs.Length; part++)
                    if ((entry.Organs[part].Role & BossOrganRole.Volley) != 0 && !Broken(boss, part)) return Position(boss, part);
            return boss.Position;
        }
        public static float CycleAdjustment(UnitState boss) => (Has(boss, BossOrganRole.Tempo, true) ? 0.75f : 0) - (Has(boss, BossOrganRole.Rupture, true) ? 0.35f : 0);
        public static float MovementMultiplier(UnitState boss) => Has(boss, BossOrganRole.Locomotion, true) ? 0.6f : 1;
        public static float VolleyAngle(UnitState boss, int index, int count, float time)
        {
            float aim = Mathf.Atan2(boss.TelegraphDirection.y, boss.TelegraphDirection.x);
            int pattern = boss.Definition.BossPattern;
            if (pattern == 5) return aim + (index - (count - 1) * 0.5f) * 0.095f;
            if (pattern == 6) return index * Mathf.PI * 2 / count + Mathf.Sin(time) * 0.3f;
            if (pattern == 7) return aim + (index % 5 - 2) * 0.17f + index / 5 * 0.08f;
            if (pattern == 8) return index * Mathf.PI * 2 / count + boss.BossVolley * 0.22f;
            if (pattern == 9) return aim + Mathf.Sin(index * 1.7f) * 0.8f;
            return CombatSimulation.BossBulletAngle(boss, index, time);
        }
        public static bool Allows(UnitState boss, ExpandedFieldKind kind)
        {
            if (!boss.Definition.IsBoss) return true;
            if (BossAnatomy.For(boss.Definition) != null)
            {
                BossOrganRole role = kind == ExpandedFieldKind.Tentacle ? BossOrganRole.Lash : kind == ExpandedFieldKind.FallingProp ? BossOrganRole.Props : kind == ExpandedFieldKind.ShadowPool ? BossOrganRole.Pool : BossOrganRole.None;
                return role == BossOrganRole.None || !Has(boss, role, true);
            }
            if (boss.LeftPartBroken && kind == ExpandedFieldKind.Tentacle) return false;
            if (boss.RightPartBroken && (kind == ExpandedFieldKind.FallingProp || kind == ExpandedFieldKind.ShadowPool)) return false;
            return true;
        }
        public static string Description => "Hit from either side to break its organ. Left: fewer shots, no lashes. Right: no props or pools, smaller shockwaves. Both broken: core shockwaves.";
        public static string Describe(UnitDefinition boss)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss);
            if (entry == null) return Description;
            string result = "BREAKABLE PARTS\nSide hits target side organs. Vertical hits target the centre.\n";
            foreach (BossOrganDefinition part in entry.Organs) result += "\n" + part.Name + ": " + part.BreakSummary;
            return result;
        }
    }

    public sealed partial class CombatSimulation
    {
        private void DamageBossPart(UnitState target, float damage, Faction source, Vector2 direction, CombatCause cause)
        {
            if (!catalog.ExpandedContent || !target.Definition.IsBoss || source == Faction.Enemy || direction.sqrMagnitude < 0.001f) return;
            int part = BossAnatomy.For(target.Definition) == null ? direction.x >= 0 ? 0 : 1 : BossPartRules.HitZone(direction);
            if (BossPartRules.Broken(target, part)) return;
            if (part == 0) target.LeftPartDamage += damage;
            else if (part == 1) target.RightPartDamage += damage;
            else target.CenterPartDamage += damage;
            if (BossPartRules.Damage(target, part) < BossPartRules.Durability(target, part)) return;
            if (part == 0) target.LeftPartBrokenAt = Elapsed;
            else if (part == 1) target.RightPartBrokenAt = Elapsed;
            else target.CenterPartBrokenAt = Elapsed;
            if (BossPartRules.Has(target, BossOrganRole.Vacuum, true) && target.StolenCoins > 0)
            {
                DropLoot(LootKind.Coin, target.Position, target.StolenCoins); target.StolenCoins = 0;
            }
            target.ExposedUntil = Mathf.Max(target.ExposedUntil, Elapsed + 1.1f);
            target.TelegraphUntil = 0;
            target.BossRecoveryUntil = Mathf.Max(target.BossRecoveryUntil, Elapsed + 0.65f);
            target.NextBossPattern = Mathf.Max(target.NextBossPattern, Elapsed + 0.9f);
            Vector2 outward = part == 0 ? Vector2.left : part == 1 ? Vector2.right : Vector2.down;
            events.Add(new CombatEvent(CombatEventKind.BossPhase, BossPartRules.Position(target, part), Faction.Enemy, value: part + 1, direction: outward, subjectId: target.Id, subject: target, cause: cause));
        }
    }
}
