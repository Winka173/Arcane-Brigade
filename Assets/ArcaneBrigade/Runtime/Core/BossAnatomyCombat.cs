using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private void ProcessBossVacuum(UnitState boss)
        {
            float delta = Mathf.Clamp(Elapsed - boss.LastBossVacuumTime, 0, 0.1f); boss.LastBossVacuumTime = Elapsed;
            if (!catalog.ExpandedContent || !BossPartRules.Has(boss, BossOrganRole.Vacuum, false) || boss.TelegraphUntil <= Elapsed || boss.ActiveBossOrgan != 2 || boss.StolenCoins >= 120) return;
            Vector2 origin = BossPartRules.Position(boss, 2);
            for (int index = pickups.Count - 1; index >= 0; index--)
            {
                PickupState pickup = pickups[index];
                if (pickup.Kind != LootKind.Coin || pickup.Attracting || pickup.Value <= 0 || pickup.Value > 120 - boss.StolenCoins || (pickup.Position - origin).sqrMagnitude > 25) continue;
                pickup.Position = Vector2.MoveTowards(pickup.Position, origin, delta * 2.1f);
                if ((pickup.Position - origin).sqrMagnitude > 0.3f) continue;
                boss.StolenCoins += pickup.Value; pickups.RemoveAt(index);
            }
        }
        private bool ResolveAnatomyAttack(UnitState boss)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            int part = boss.ActiveBossOrgan;
            bool core = boss.LeftPartBroken && boss.RightPartBroken;
            if (core) Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 4.5f, 0.9f, 1.4f);
            if (part < 0 || BossPartRules.Broken(boss, part)) return false;
            BossOrganRole role = entry.Organs[part].Role;
            Vector2 target = ClampPosition(Champion.Position, 1);
            Vector2 origin = BossPartRules.Position(boss, part);
            Vector2 direction = (target - origin).normalized;
            int pattern = boss.Definition.BossPattern;
            if ((role & BossOrganRole.Lash) != 0)
            {
                int lashes = pattern == 6 ? 3 : pattern == 9 ? 2 : 1;
                for (int index = 0; index < lashes; index++)
                {
                    Vector2 heading = Quaternion.Euler(0, 0, (index - (lashes - 1) * 0.5f) * 28) * direction;
                    Danger(ExpandedFieldKind.Tentacle, boss, origin + heading * (pattern == 9 ? 6 : 4.5f), pattern == 5 ? 0.52f : 0.3f, 0.8f + index * 0.18f, 0.45f, part);
                }
            }
            if ((role & BossOrganRole.Props) != 0)
            {
                int count = pattern == 9 ? 4 : pattern == 4 ? 3 : 2;
                for (int index = 0; index < count; index++)
                {
                    Vector2 offset = pattern == 9 ? (Vector2)(Quaternion.Euler(0, 0, index * 90 + boss.BossVolley * 17) * Vector2.right) * 1.8f : Vector2.right * ((index - (count - 1) * 0.5f) * 1.7f);
                    Danger(ExpandedFieldKind.FallingProp, boss, target + offset, 0.7f, 0.95f + index * 0.2f, 0.4f, part);
                }
            }
            if ((role & BossOrganRole.Pool) != 0)
            {
                int count = pattern == 1 ? 3 : pattern == 6 ? 2 : 1;
                for (int index = 0; index < count; index++)
                    Danger(ExpandedFieldKind.ShadowPool, boss, target + Vector2.right * ((index - (count - 1) * 0.5f) * 1.6f), pattern == 3 ? 1.25f : 0.85f, 1 + index * 0.25f, 1.5f, part);
            }
            if ((role & BossOrganRole.Pulse) != 0)
            {
                int pulses = pattern == 8 ? 3 : 2;
                for (int index = 0; index < pulses; index++) Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 5.5f - index * 0.4f, 0.8f + index * 0.7f, 1.6f, part);
            }
            if ((role & BossOrganRole.Tempo) != 0)
            {
                Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 5, 0.9f, 1.4f, part);
                if (pattern == 0) Danger(ExpandedFieldKind.FallingProp, boss, target, 0.8f, 1.2f, 0.4f, part);
            }
            if ((role & BossOrganRole.Vacuum) != 0) Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 5, 1, 1.5f, part);
            return (role & BossOrganRole.Volley) == 0 && !core;
        }
    }
}
