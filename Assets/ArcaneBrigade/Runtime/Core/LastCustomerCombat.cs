using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool captureCustomerDefeat;
        public void DefeatCustomerCapture()
        {
            if ((!RunPaths.K7Capture && !MienChoCapture.Requested) || Boss?.Definition.Id != NightmareRules.CustomerId) throw new System.InvalidOperationException("Customer fixture requires isolated capture mode");
            captureCustomerDefeat = true;
        }
        private bool ProcessLastCustomer(UnitState boss)
        {
            if (boss.Definition.Id != NightmareRules.CustomerId) return false;
            if (captureCustomerDefeat)
            {
                captureCustomerDefeat = false;
                ApplyDamage(boss, boss.Health + 1, Faction.Champion, "Customer diagnostic", cause: NewCause(Champion.Id), injectedDamage: true);
                return true;
            }
            boss.BossPoseTime = Elapsed;
            if (boss.TelegraphUntil > 0)
            {
                if (Elapsed < boss.TelegraphUntil) return true;
                boss.TelegraphUntil = 0;
                Vector2 target = boss.AbilityTarget;
                Vector2 across = new Vector2(-boss.TelegraphDirection.y,boss.TelegraphDirection.x);
                int attack = boss.BossVolley % 6;
                bool enraged = boss.Health < boss.Definition.Health * .45f;
                if (BossPartRules.Broken(boss,boss.ActiveBossOrgan))
                    Danger(ExpandedFieldKind.Shockwave,boss,target,2.4f,.9f,1.1f);
                else switch (attack)
                {
                    case 0:
                        Danger(ExpandedFieldKind.FallingProp,boss,target,1.25f,.85f,.45f,0);
                        Danger(ExpandedFieldKind.Shockwave,boss,target,3.4f,1.05f,1.4f,0);
                        break;
                    case 1:
                        for (int index = -1; index <= 1; index++) Danger(ExpandedFieldKind.Tentacle,boss,target + across * index * 2.4f,.35f,.8f + (index + 1) * .35f,.5f,1);
                        break;
                    case 2:
                        for (int index = 0; index < (enraged ? 6 : 4); index++)
                        {
                            float angle = index * 2.39996f;
                            Danger(ExpandedFieldKind.FallingProp,boss,target + new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * (index == 0 ? 0 : 2.5f),.75f,.9f + index * .22f,.4f,2);
                        }
                        break;
                    case 3:
                        Danger(ExpandedFieldKind.Tentacle,boss,target + across * 2,.4f,.8f,.65f,2);
                        Danger(ExpandedFieldKind.Tentacle,boss,target - across * 2,.4f,1.3f,.65f,2);
                        break;
                    case 4:
                        for (int index = -1; index <= 1; index++) Danger(ExpandedFieldKind.FallingProp,boss,target + across * index * 2.2f,.85f,1 + Mathf.Abs(index) * .4f,.4f,0);
                        boss.AbilityOrigin = boss.Position; boss.AbilityTarget = ClampPosition(target,2);
                        boss.BossSkillStarted = Elapsed; boss.BossSkillUntil = Elapsed + .85f;
                        break;
                    default:
                        Danger(ExpandedFieldKind.ShadowPool,boss,target,1.15f,1.2f,2,2);
                        Danger(ExpandedFieldKind.Tentacle,boss,target + across * 3,.4f,.9f,.5f,1);
                        Danger(ExpandedFieldKind.Tentacle,boss,target - across * 3,.4f,1.3f,.5f,1);
                        break;
                }
                boss.BossVolley++; boss.BossOrganReleaseAt = Elapsed;
                boss.AttackStarted = Elapsed - boss.AttackWindup; boss.BossRecoveryUntil = Elapsed + 1.7f;
                boss.NextBossPattern = Elapsed + (enraged ? 3.9f : 4.8f);
                events.Add(new CombatEvent(CombatEventKind.Fire,boss.Position,Faction.Enemy,subject:boss));
            }
            if (Elapsed < boss.NextBossPattern) return true;
            boss.ActiveBossOrgan = boss.BossVolley % 6 / 2;
            boss.TelegraphDirection = (Champion.Position - boss.Position).normalized;
            boss.AbilityTarget = ClampPosition(Champion.Position,1);
            boss.AttackWindup = 1.25f; boss.AttackStarted = Elapsed; boss.TelegraphUntil = Elapsed + 1.25f;
            events.Add(new CombatEvent(CombatEventKind.Windup,boss.Position,Faction.Enemy,subject:boss));
            return true;
        }
    }
}
