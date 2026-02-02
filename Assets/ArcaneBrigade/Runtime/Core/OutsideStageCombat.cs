using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private float outsideSummonAt;
        private int outsideSummonCycle;
        private void ResetOutsideSummons(){outsideSummonAt=6;outsideSummonCycle=0;}
        private void SummonOutsideGuard(UnitState boss)
        {
            if(Elapsed<outsideSummonAt)return;
            outsideSummonAt=Elapsed+(boss.Health<boss.Definition.Health*.35f?8:11);
            int alive=0;foreach(var unit in units)if(unit.Alive && unit.Faction==Faction.Enemy && unit!=boss)alive++;
            int amount=Mathf.Min(12-alive,boss.Health<boss.Definition.Health*.5f?4:3);
            for(int i=0;i<amount;i++)
            {
                float side=(outsideSummonCycle+i)%2==0?-1:1;
                Vector2 position=ClampPosition(new Vector2(side*(catalog.ArenaSize.x*.5f-1),-3+(outsideSummonCycle+i)%4*2),.6f);
                if((position-Champion.Position).sqrMagnitude<16)position=ClampPosition(new Vector2(-position.x,position.y),.6f);
                var guard=Spawn((outsideSummonCycle+i)%2==0?catalog.MeleeEnemy:catalog.RangedEnemy,Faction.Enemy,position);
                guard.EntranceUntil=Elapsed+1.2f;guard.NextAttack=guard.NextContact=Elapsed+1.6f;guard.SwarmMinion=true;
                if(guard.Variant!=null){guard.Variant.Summoned=true;guard.Variant.LeaderId=0;}
            }
            outsideSummonCycle++;
        }
        private bool ProcessOutsideStage(UnitState boss)
        {
            if (boss.Definition.Id != "OutsideStageKeeper") return false;
            SummonOutsideGuard(boss);
            boss.BossPoseTime = Elapsed;
            if (boss.TelegraphUntil > 0)
            {
                if (Elapsed < boss.TelegraphUntil) return true;
                boss.TelegraphUntil = 0;
                int pattern = boss.BossVolley % 6;
                int part = pattern / 2;
                Vector2 target = boss.AbilityTarget;
                Vector2 across = new Vector2(-boss.TelegraphDirection.y, boss.TelegraphDirection.x);
                if (BossPartRules.Broken(boss, part)) Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 3.2f, 0.65f, 1.3f);
                else switch (pattern)
                {
                    case 0:
                        Danger(ExpandedFieldKind.FallingProp, boss, target, 1.05f, 0.75f, 0.4f, 0);
                        Danger(ExpandedFieldKind.Shockwave, boss, target, 3.1f, 0.85f, 1.4f, 0);
                        break;
                    case 1:
                        for (int index = -1; index <= 1; index++) Danger(ExpandedFieldKind.Tentacle, boss, target + across * index * 1.7f, 0.32f, 0.35f + (index + 1) * 0.3f, 0.4f, 0);
                        break;
                    case 2:
                        Danger(ExpandedFieldKind.Tentacle, boss, target, 0.6f, 0.5f, 0.5f, 1);
                        foreach (PickupState coin in pickups) if (!coin.Attracting) coin.Position = Vector2.Lerp(coin.Position, boss.Position, 0.25f);
                        break;
                    case 3:
                        for (int index = -1; index <= 1; index += 2) Danger(ExpandedFieldKind.Tentacle, boss, target + across * index * 2.2f, 0.45f, index < 0 ? 0.4f : 0.95f, 0.4f, 1);
                        break;
                    case 4:
                        for (int index = 0; index < 3; index++) Danger(ExpandedFieldKind.ShadowPool, boss, target + across * (index - 1) * 2.2f, 0.8f, 0.7f + index * 0.2f, 1.5f, 2);
                        break;
                    case 5:
                        for (int index = 0; index < 3; index++) Danger(ExpandedFieldKind.FallingProp, boss, target + across * (index - 1) * 2, 0.65f, 0.85f + index * 0.3f, 0.4f, 2);
                        break;
                }
                boss.BossVolley++; boss.BossOrganReleaseAt = Elapsed;
                boss.AttackStarted = Elapsed - boss.AttackWindup;
                boss.BossRecoveryUntil = Elapsed + 1.2f;
                boss.NextBossPattern = Elapsed + (boss.Health < boss.Definition.Health * 0.35f ? 2.8f : 3.5f);
                events.Add(new CombatEvent(CombatEventKind.Fire, boss.Position, Faction.Enemy, subject: boss));
                return true;
            }
            if (Elapsed < boss.NextBossPattern) return true;
            int phase = boss.Health < boss.Definition.Health * 0.35f ? 2 : boss.Health < boss.Definition.Health * 0.7f ? 1 : 0;
            if (boss.BossVolley % 6 > phase * 2 + 1) boss.BossVolley += 6 - boss.BossVolley % 6;
            boss.ActiveBossOrgan = boss.BossVolley % 6 / 2;
            boss.ActiveBossSignature = -1;
            boss.AttackStarted = Elapsed; boss.AttackWindup = 1.35f;
            boss.TelegraphUntil = Elapsed + boss.AttackWindup;
            boss.TelegraphDirection = (Champion.Position - boss.Position).normalized;
            boss.AbilityTarget = ClampPosition(Champion.Position, 1);
            events.Add(new CombatEvent(CombatEventKind.Windup, BossPartRules.Position(boss, boss.ActiveBossOrgan), Faction.Enemy, subject: boss));
            return true;
        }
    }
}
