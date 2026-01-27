using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool MoveBossSignature(UnitState boss)
        {
            boss.BossPoseTime = Elapsed;
            if (boss.BossSkillUntil <= boss.BossSkillStarted || Elapsed < boss.BossSkillStarted) return false;
            if (Elapsed >= boss.BossSkillUntil)
            {
                boss.Position = ClampPosition(boss.AbilityTarget, boss.Definition.Radius);
                boss.BodyLift = 0; boss.BossSkillStarted = boss.BossSkillUntil = -100;
                events.Add(new CombatEvent(CombatEventKind.BossLanded, boss.Position, Faction.Enemy, subjectId: boss.Id, subject: boss));
                return true;
            }
            float progress = Mathf.Clamp01((Elapsed - boss.BossSkillStarted) / (boss.BossSkillUntil - boss.BossSkillStarted));
            boss.Position = Vector2.Lerp(boss.AbilityOrigin, boss.AbilityTarget, Mathf.SmoothStep(0, 1, progress));
            boss.BodyLift = Mathf.Sin(progress * Mathf.PI) * Mathf.Min(1.15f, boss.Definition.BodyHeight * 0.24f);
            return true;
        }

        private void ResolveBossSignature(UnitState boss)
        {
            int pattern = boss.Definition.BossPattern;
            int variation = boss.ActiveBossSignature;
            Vector2 target = boss.AbilityTarget;
            Vector2 heading = boss.TelegraphDirection.sqrMagnitude > 0.001f ? boss.TelegraphDirection : Vector2.down;
            Vector2 across = new Vector2(-heading.y, heading.x);
            bool enraged = boss.Health < boss.Definition.Health * 0.5f;
            if (BossEncounterRules.Leaps(pattern, variation))
            {
                boss.AbilityOrigin = boss.Position;
                boss.BossSkillStarted = Elapsed; boss.BossSkillUntil = Elapsed + 1.1f;
                Danger(ExpandedFieldKind.Shockwave, boss, target, enraged ? 5.4f : 4.5f, 1.1f, 1.6f);
                if (variation == 1)
                    for (int index = -1; index <= 1; index++) Danger(ExpandedFieldKind.FallingProp, boss, target + across * index * 2, 0.65f, 1.65f + Mathf.Abs(index) * 0.22f, 0.35f);
                return;
            }
            int strokes = enraged ? 4 : 3;
            switch (pattern)
            {
                case 0:
                    for (int index = 0; index < strokes; index++)
                        Danger(variation == 0 ? ExpandedFieldKind.FallingProp : ExpandedFieldKind.Tentacle, boss, target + (variation == 0 ? across : heading) * ((index - 1) * 2), variation == 0 ? 0.95f : 0.35f, 0.95f + index * 0.3f, 0.4f);
                    break;
                case 1:
                    for (int index = 0; index < strokes; index++)
                    {
                        Vector2 direction = Quaternion.Euler(0, 0, (index - 1) * 38) * heading;
                        Danger(variation == 0 ? ExpandedFieldKind.Tentacle : ExpandedFieldKind.ShadowPool, boss, variation == 0 ? boss.Position + direction * 7 : target + across * ((index - 1) * 1.8f), variation == 0 ? 0.34f : 0.85f, 0.9f + index * 0.25f, variation == 0 ? 0.45f : 1.8f);
                    }
                    break;
                case 3:
                    for (int index = 0; index < strokes; index++)
                        Danger(variation == 0 ? ExpandedFieldKind.Tentacle : ExpandedFieldKind.ShadowPool, boss, target + across * ((index - 1) * 2), variation == 0 ? 0.4f : 1.05f, 0.9f + index * 0.35f, variation == 0 ? 0.4f : 1.7f);
                    break;
                case 4:
                    for (int index = 0; index < 4; index++)
                    {
                        Vector2 offset = Quaternion.Euler(0, 0, index * 90 + variation * 45) * Vector2.right * 2.4f;
                        Danger(ExpandedFieldKind.FallingProp, boss, target + offset, 0.8f, 1 + index * 0.18f, 0.4f);
                    }
                    if (variation == 1) Danger(ExpandedFieldKind.ShadowPool, boss, target, 1.1f, 1.5f, 1.6f);
                    break;
                case 5:
                    for (int index = 0; index < strokes; index++) Danger(ExpandedFieldKind.FallingProp, boss, target + across * ((index - 1) * 2.1f), 0.85f, 0.9f + index * 0.25f, 0.4f);
                    break;
                case 7:
                    for (int index = 0; index < (variation == 0 ? 2 : strokes); index++)
                    {
                        Vector2 direction = Quaternion.Euler(0, 0, (index - 0.5f) * 46) * heading;
                        Danger(variation == 0 ? ExpandedFieldKind.Tentacle : ExpandedFieldKind.FallingProp, boss, variation == 0 ? boss.Position + direction * 7 : target + across * ((index - 1) * 2), variation == 0 ? 0.38f : 0.8f, 1 + index * 0.28f, 0.45f);
                    }
                    break;
                case 8:
                    for (int index = 0; index < strokes; index++)
                        Danger(variation == 0 ? ExpandedFieldKind.Shockwave : ExpandedFieldKind.FallingProp, boss, variation == 0 ? boss.Position : target + across * ((index - 1) * 1.9f), variation == 0 ? 6 - index * 0.45f : 0.8f, 0.9f + index * 0.65f, variation == 0 ? 1.5f : 0.4f);
                    break;
            }
        }
    }
}
