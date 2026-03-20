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
            return entry == null ? boss.Position : boss.Position + AnimatedOffset(boss, part) * boss.Definition.BodyHeight + Vector2.up * boss.BodyLift;
        }
        public static Vector2 AnimatedOffset(UnitState boss, int part)
        {
            BossAnatomyEntry entry = BossAnatomy.For(boss.Definition);
            if (entry == null) return Vector2.zero;
            BossOrganDefinition organ = entry.Organs[part];
            Vector2 offset = organ.Offset;
            float time = boss.BossPoseTime;
            float phase = time * 1.45f + part * 2.1f + boss.Definition.BossPattern * 0.4f;
            if ((organ.Role & (BossOrganRole.Tempo | BossOrganRole.Pulse)) != 0)
                offset += new Vector2(Mathf.Cos(phase) * 0.11f, Mathf.Sin(phase) * 0.065f);
            else if ((organ.Role & (BossOrganRole.Volley | BossOrganRole.Pool | BossOrganRole.Props)) != 0)
                offset += new Vector2(Mathf.Sin(phase * 0.7f) * 0.035f, Mathf.Sin(phase) * 0.055f + 0.035f);
            if (boss.ActiveBossOrgan == part && !Broken(boss, part))
            {
                float age = time - boss.BossOrganReleaseAt;
                if ((organ.Role & BossOrganRole.Lash) != 0 && age >= 0 && age < 0.75f)
                    offset += boss.TelegraphDirection * Mathf.Sin(age / 0.75f * Mathf.PI) * 0.26f;
            }
            return offset;
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
            bool whole = catalog.WholeKeeper && target.Definition.Id == "OutsideStageKeeper";
            // the whole Keeper counts every blow toward a part, even one with no direction (homing parcels, fields, echoes):
            // otherwise a brigade without side hits could never break it and its body would shrug them off for ever
            if (!catalog.ExpandedContent || !target.Definition.IsBoss || source == Faction.Enemy || direction.sqrMagnitude < 0.001f && !whole) return;
            int part = direction.sqrMagnitude < 0.001f ? 2 : BossAnatomy.For(target.Definition) == null ? direction.x >= 0 ? 0 : 1 : BossPartRules.HitZone(direction);
            // the whole Keeper: a blow at a part already broken lands on one still holding (eye, then hand, then carpet)
            if (whole && BossPartRules.Broken(target, part))
                foreach (int other in new[] { 2, 0, 1 }) if (!BossPartRules.Broken(target, other)) { part = other; break; }
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
            target.ActiveBossSignature = -1; target.ActiveTrademark = false;
            target.BossRecoveryUntil = Mathf.Max(target.BossRecoveryUntil, Elapsed + 0.65f);
            target.NextBossPattern = Mathf.Max(target.NextBossPattern, Elapsed + 0.9f);
            Vector2 outward = part == 0 ? Vector2.left : part == 1 ? Vector2.right : Vector2.down;
            events.Add(new CombatEvent(CombatEventKind.BossPhase, BossPartRules.Position(target, part), Faction.Enemy, value: part + 1, direction: outward, subjectId: target.Id, subject: target, cause: cause));
            if (catalog.WholeKeeper && target.Definition.Id == "OutsideStageKeeper" && BossPartRules.Broken(target, 0) && BossPartRules.Broken(target, 1) && BossPartRules.Broken(target, 2))
            {
                // hand, carpet and eye all broken: it kneels for a while, the way a gate settles on its hinges
                target.KneelUntil = Elapsed + KeeperKneelSeconds; target.TelegraphUntil = 0; target.NextBossPattern = target.KneelUntil + 0.6f;
                events.Add(new CombatEvent(CombatEventKind.BossLanded, target.Position, Faction.Enemy, subjectId: target.Id, subject: target, cause: cause));
            }
        }
        public const float KeeperKneelSeconds = 7f, KeeperBodyGuard = 0.2f, KeeperKneelExposure = 1.6f;
        /// <summary>How much of a blow reaches the whole Keeper's body: a fifth while all three parts hold, more for each part
        /// broken, the full blow once all are broken, and more than that while it kneels.</summary>
        public static float KeeperGuard(UnitState keeper, float elapsed)
        {
            // it is a test, not a siege: after a minute and a half it has seen enough and gives way more every second
            float patience = elapsed > KeeperPatienceSeconds ? 1 + (elapsed - KeeperPatienceSeconds) / 30f : 1;
            if (keeper.KneelUntil > elapsed) return KeeperKneelExposure * patience;
            int broken = (BossPartRules.Broken(keeper, 0) ? 1 : 0) + (BossPartRules.Broken(keeper, 1) ? 1 : 0) + (BossPartRules.Broken(keeper, 2) ? 1 : 0);
            return (KeeperBodyGuard + (1 - KeeperBodyGuard) * broken / 3f) * patience;
        }
        public const float KeeperPatienceSeconds = 90f, KeeperTestSeconds = 180f;
        public bool KeeperWithdrew { get; private set; } // the test ran out: no gift, and no harm
        private float KeeperGuard(UnitState keeper) => KeeperGuard(keeper, Elapsed);
    }
}
