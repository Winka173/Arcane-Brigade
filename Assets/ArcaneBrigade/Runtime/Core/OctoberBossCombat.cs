using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private sealed class OctoberShot { public UnitState Boss; public Vector2 Position, Direction; public float At, Speed; public HostileShot Motion; public CombatCause Cause; }
        private readonly List<OctoberShot> octoberShots = new List<OctoberShot>();
        private readonly Dictionary<int, float> octoberBossAt = new Dictionary<int, float>();
        private readonly Dictionary<int, int> octoberBossCast = new Dictionary<int, int>();
        private void ClearOctoberBosses() { octoberShots.Clear(); octoberBossAt.Clear(); octoberBossCast.Clear(); }
        private void ProcessOctoberBoss(UnitState boss)
        {
            if (boss.EntranceUntil > Elapsed || boss.Health <= 0) return;
            if (!octoberBossAt.TryGetValue(boss.Id, out float at)) at = 3.5f;
            if (Elapsed < at || octoberShots.Count > 240) return;
            int cast = octoberBossCast.TryGetValue(boss.Id, out int n) ? n : 0;
            octoberBossCast[boss.Id] = cast + 1;
            int identity = boss.Definition.Id == "OutsideStageKeeper" ? 11 : boss.Definition.Id == NightmareRules.CustomerId ? 12 : boss.Definition.BossPattern;
            int pattern = (cast + identity) % 6;
            Vector2 aim = (Champion.Position - boss.Position).normalized;
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            CombatCause cause = NewCause(boss.Id).Child("boss-encore-" + pattern);
            void Shot(Vector2 origin, float degrees, float when, float speed, HostileShot motion = HostileShot.Straight)
            {
                octoberShots.Add(new OctoberShot { Boss = boss, Position = origin, Direction = Quaternion.Euler(0, 0, degrees) * Vector2.right, At = Elapsed + .9f + when, Speed = speed, Motion = motion, Cause = cause });
            }
            Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, boss.Definition.Radius + .5f, .9f, 0);
            switch (pattern)
            {
                case 0:
                    for (int i = 0; i < 36; i++) { Shot(boss.Position, angle + i * 19 + identity * 7, i * .045f, 2.05f, HostileShot.Weave); Shot(boss.Position, angle - i * 19 - identity * 7 + 180, i * .045f, 1.8f); } break;
                case 1:
                    for (int wave = 0; wave < 3; wave++) for (int i = -4; i <= 4; i++) if (i != (wave % 2 == 0 ? 1 : -1)) Shot(boss.Position, angle + i * 12 + wave * 4, wave * .4f, 2.15f, wave == 2 ? HostileShot.Accelerate : HostileShot.Straight); break;
                case 2:
                    for (int wave = 0; wave < 2; wave++) for (int i = 0; i < 24; i++) if (i % 8 >= 2) Shot(boss.Position, angle + i * 15 + wave * 7.5f, wave * .65f, wave == 0 ? 1.7f : 2.2f); break;
                case 3:
                    for (int arm = 0; arm < 4; arm++) for (int i = -3; i <= 3; i++) Shot(boss.Position, arm * 90 + identity * 11 + i * 6, Mathf.Abs(i) * .08f, 1.85f, HostileShot.Weave); break;
                case 4:
                    for (int i = 0; i < 28; i++) if (i % 7 != 0) Shot(boss.Position, angle + i * 360f / 28, i * .018f, 1.65f, HostileShot.Hook); break;
                case 5:
                    for (int i = -6; i <= 6; i++) if (i != 0 && i != 1)
                    {
                        Vector2 origin = ClampPosition(Champion.Position + new Vector2(i * .8f, 4.5f), .3f);
                        Shot(origin, -90 + (i % 2 == 0 ? 9 : -9), Mathf.Abs(i) * .06f, 1.85f, HostileShot.Drift);
                        Danger(ExpandedFieldKind.Shockwave, boss, origin, .3f, .9f, 0);
                    } break;
            }
            octoberBossAt[boss.Id] = Elapsed + (boss.Definition.Id == "OutsideStageKeeper" ? 4.2f : 5.2f) + (boss.Health > boss.Definition.Health * .5f ? 1 : 0);
            events.Add(new CombatEvent(CombatEventKind.Windup, boss.Position, Faction.Enemy, subject: boss, cause: cause));
            if (boss.Definition.Id != "OutsideStageKeeper" && boss.Definition.Id != NightmareRules.CustomerId && cast % 3 == 2)
            {
                boss.OctoberLungeFrom = boss.Position; boss.OctoberLungeTo = ClampPosition(Champion.Position - aim * .8f, boss.Definition.Radius);
                boss.OctoberLungeAt = Elapsed + 1.1f; boss.OctoberLungeUntil = boss.OctoberLungeAt + .55f;
                Danger(ExpandedFieldKind.Shockwave, boss, boss.OctoberLungeTo, 2.2f, 1.7f, .5f);
            }
        }
        private bool MoveOctoberBoss(UnitState unit)
        {
            if (!unit.Definition.IsBoss || unit.OctoberLungeUntil <= Elapsed || unit.OctoberLungeAt <= 0) return false;
            if (Elapsed < unit.OctoberLungeAt) return true;
            float p = Mathf.Clamp01((Elapsed - unit.OctoberLungeAt) / .55f);
            unit.Position = ClampPosition(Vector2.Lerp(unit.OctoberLungeFrom, unit.OctoberLungeTo, p * p * (3 - 2 * p)), unit.Definition.Radius); return true;
        }
        private void ProcessOctoberShots()
        {
            for (int i = octoberShots.Count - 1; i >= 0; i--)
            {
                OctoberShot shot = octoberShots[i];
                if (!shot.Boss.Alive) { octoberShots.RemoveAt(i); continue; }
                if (shot.At > Elapsed) continue;
                octoberShots.RemoveAt(i); if (projectiles.Count >= 1200) continue;
                Vector2 origin = shot.Position + shot.Direction * shot.Boss.Definition.Radius * .7f;
                projectiles.Add(new ProjectileState { Id = ++nextId, Position = origin, PreviousPosition = origin, Velocity = shot.Direction * shot.Speed, LaunchVelocity = shot.Direction * shot.Speed,
                    Damage = shot.Boss.Definition.Weapon.Damage * .7f, Faction = Faction.Enemy, SourceUnitId = shot.Boss.Id, SourceName = shot.Boss.Definition.DisplayName + " encore volley", Cause = shot.Cause,
                    Remaining = 7, Color = new Color(1, .32f, .48f), Radius = .075f, VisualWidth = .2f, Sprite = catalog.Projectile, HostileMotion = shot.Motion, Element = DamageElement.Arcane });
            }
        }
    }
}
