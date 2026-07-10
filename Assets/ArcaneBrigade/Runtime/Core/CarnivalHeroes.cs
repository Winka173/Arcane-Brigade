using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private void ProcessCarnivalHero(float delta, HeroMechanic mechanic)
        {
            if (mechanic == HeroMechanic.RootedOracle || mechanic == HeroMechanic.SoloMaw)
            {
                if (Elapsed < heroReadyAt) return;
                bool threat = false;
                foreach (UnitState enemy in StableUnits)
                    threat |= enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - Champion.Position).sqrMagnitude <= 6.25f;
                if (!threat) return;
                CombatCause pulse = NewCause(Champion.Id).Child(mechanic == HeroMechanic.RootedOracle ? "root-ward" : "solo-whirl");
                Vector2 ward = ThreatPoint(2.5f);
                foreach (UnitState enemy in StableUnits)
                {
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy || (enemy.Position - ward).sqrMagnitude > 1.69f) continue;
                    ApplyWeaponDamage(enemy, Champion.Definition.Weapon.Damage * (mechanic == HeroMechanic.RootedOracle ? 1 : 0.8f), Faction.Champion, mechanic == HeroMechanic.RootedOracle ? DamageElement.Frost : DamageElement.Physical, "Personal ward", cause: pulse);
                    if (mechanic == HeroMechanic.RootedOracle && enemy.Alive && !enemy.Definition.IsBoss)
                    {
                        Vector2 direction = (enemy.Position - Champion.Position).normalized;
                        enemy.Position = ClampPosition(enemy.Position + direction * 0.35f * (1 - enemy.Definition.KnockbackResistance), enemy.Definition.Radius);
                        spatial.Move(enemy);
                    }
                }
                heroReadyAt = Elapsed + 2; HeroActivations++; Champion.LastSupport = Elapsed;
                events.Add(new CombatEvent(CombatEventKind.SignaturePulse, ward, Faction.Champion, 1.3f, subject: Champion, element: mechanic == HeroMechanic.RootedOracle ? DamageElement.Frost : DamageElement.Physical, cause: pulse));
                return;
            }
            float moved = Vector2.Distance(Champion.Position, Champion.PreviousPosition);
            float required = mechanic == HeroMechanic.MirrorGuard ? 2 : mechanic == HeroMechanic.TideCollector ? 8 : mechanic == HeroMechanic.GlassGambler ? 6 : 10;
            if (mechanic == HeroMechanic.ClockRunner) heroCharge += moved;
            else if (mechanic == HeroMechanic.MirrorGuard) heroCharge = moved > 0.001f ? 0 : heroCharge + delta;
            else
            {
                int count = events.Count;
                for (int index = 0; index < count; index++)
                    if (events[index].Kind == CombatEventKind.Pickup && events[index].Loot == LootKind.Coin)
                        heroCharge += Mathf.Max(0, events[index].Value);
            }
            heroCharge = Mathf.Min(required, heroCharge);
            if (heroCharge < required || Elapsed < heroReadyAt) return;
            if (mechanic == HeroMechanic.MirrorGuard)
            {
                int reflected = 0;
                foreach (ProjectileState projectile in projectiles)
                {
                    if (projectile.Faction != Faction.Enemy || (projectile.Position - Champion.Position).sqrMagnitude > 9 || reflected >= 3) continue;
                    projectile.Faction = Faction.Champion; projectile.HostileMotion = HostileShot.Straight;
                    projectile.Velocity = -projectile.Velocity; projectile.SourceUnitId = Champion.Id;
                    projectile.Cause = NewCause(Champion.Id).Child("mirror-guard"); projectile.Color = new Color(0.4f, 1, 0.8f);
                    projectile.Victims.Clear(); reflected++;
                }
                if (reflected == 0) return;
            }
            else if (mechanic == HeroMechanic.TreasureTax)
            {
                Gold = RunSession.BoundedCounter((long)Gold + 1);
                Heal(1, NewCause(Champion.Id).Child("treasure-tax"));
            }
            else
            {
                CombatCause cause = NewCause(Champion.Id).Child("hero-" + mechanic);
                if (mechanic == HeroMechanic.TideCollector) Heal(3, cause);
                Vector2 burst = ThreatPoint(3);
                foreach (UnitState enemy in StableUnits)
                {
                    if (!enemy.Alive || enemy.Faction != Faction.Enemy || (enemy.Position - burst).sqrMagnitude > 1.96f) continue;
                    if (mechanic == HeroMechanic.GlassGambler)
                        ApplyWeaponDamage(enemy, Champion.Definition.Weapon.Damage * 0.7f, Faction.Champion, DamageElement.Arcane, "Lucky burst", cause: cause);
                    else if (mechanic == HeroMechanic.ClockRunner)
                        enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + (enemy.Definition.IsBoss ? 0.5f : 1.4f));
                    else
                    {
                        Vector2 direction = (enemy.Position - Champion.Position).normalized;
                        enemy.Position = ClampPosition(enemy.Position + direction * 0.5f * (1 - enemy.Definition.KnockbackResistance), enemy.Definition.Radius);
                        spatial.Move(enemy);
                    }
                }
            }
            heroCharge = 0; heroReadyAt = Elapsed + 4; HeroActivations++; Champion.LastSupport = Elapsed;
            events.Add(new CombatEvent(CombatEventKind.SignaturePulse, ThreatPoint(3), Faction.Champion, 1.4f, subject: Champion, element: DamageElement.Arcane, cause: NewCause(Champion.Id)));
        }
    }
}
