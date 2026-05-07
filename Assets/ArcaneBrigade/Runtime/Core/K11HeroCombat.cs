using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private float heroCharge;
        private float heroReadyAt;
        public int HeroActivations { get; private set; }
        public float HeroCharge => heroCharge;
        public float HeroCooldownRemaining => Mathf.Max(0, heroReadyAt - Elapsed);
        private void ResetHeroMechanic() { heroCharge = heroReadyAt = 0; HeroActivations = 0; }
        private void ProcessHeroMechanic(float deltaTime)
        {
            if (HeroIdentityEnabled) return;
            if (!catalog.EnableHeroMechanics || !Champion.Alive || Phase != RunPhase.Combat) return;
            HeroMechanic mechanic = Champion.Definition.HeroMechanic;
            if (mechanic >= HeroMechanic.RootedOracle) { ProcessCarnivalHero(deltaTime, mechanic); return; }
            if (mechanic == HeroMechanic.None) return;
            float moved = Vector2.Distance(Champion.Position, Champion.PreviousPosition);
            if (mechanic == HeroMechanic.LanternStride) heroCharge = Mathf.Min(12, heroCharge + moved);
            else if (mechanic == HeroMechanic.DreamBreath) heroCharge = moved > 0.001f ? 0 : Mathf.Min(2, heroCharge + deltaTime);
            else
            {
                int originalEvents = events.Count;
                for (int index = 0; index < originalEvents; index++)
                {
                    CombatEvent pickup = events[index];
                    if (pickup.Kind == CombatEventKind.Pickup && pickup.Loot == LootKind.Coin) heroCharge = Mathf.Min(6, heroCharge + Mathf.Max(0, pickup.Value));
                }
            }
            float required = mechanic == HeroMechanic.LanternStride ? 12 : mechanic == HeroMechanic.ParcelExpress ? 6 : 2;
            if (heroCharge < required || Elapsed < heroReadyAt) return;
            UnitState target = mechanic == HeroMechanic.ParcelExpress ? FindTarget(Champion, Champion.Definition.Weapon.Range) : null;
            if (mechanic == HeroMechanic.ParcelExpress && target == null) return;
            if (mechanic != HeroMechanic.ParcelExpress)
            {
                float reach = mechanic == HeroMechanic.LanternStride ? 2.4f : 3;
                bool threat = false;
                foreach (UnitState enemy in units) threat |= enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - Champion.Position).sqrMagnitude <= reach * reach;
                if (mechanic == HeroMechanic.DreamBreath)
                    foreach (ProjectileState projectile in projectiles) threat |= projectile.Faction == Faction.Enemy && (projectile.Position - Champion.Position).sqrMagnitude <= 9;
                if (!threat) return;
            }
            heroCharge = 0; heroReadyAt = Elapsed + (mechanic == HeroMechanic.DreamBreath ? 7 : 4); HeroActivations++;
            Champion.LastSupport = Elapsed;
            CombatCause cause = NewCause(Champion.Id).Child("hero-" + mechanic);
            Contribution(cause).Activations++;
            if (mechanic == HeroMechanic.ParcelExpress)
            {
                Vector2 previousDirection = Champion.AttackDirection;
                Champion.AttackDirection = (target.Position - Champion.Position).normalized;
                EmitProjectile(Champion, Champion.Definition.Weapon.Damage * 1f, false, cause); // owner 23/10/2026: 70% -> 100%
                events.Add(new CombatEvent(CombatEventKind.Fire, Champion.Position, Faction.Champion, direction: Champion.AttackDirection, weapon: Champion.Definition.Weapon, subject: Champion, cause: cause));
                Champion.AttackDirection = previousDirection;
            }
            else
            {
                // lands on the nearest threat (radius 1.3 round it), not as a ring round the hero
                float radius = 1.3f; Vector2 centre = ThreatPoint(mechanic == HeroMechanic.LanternStride ? 2.4f : 3);
                foreach (UnitState enemy in units)
                    if (enemy.Alive && enemy.Faction == Faction.Enemy && InBody(enemy, centre, radius))
                    {
                        if (mechanic == HeroMechanic.LanternStride) ApplyWeaponDamage(enemy, Champion.Definition.Weapon.Damage * 0.65f, Faction.Champion, DamageElement.Ember, Champion.Definition.DisplayName, (enemy.Position - Champion.Position).normalized, Champion.Definition.Weapon, cause: cause);
                        else enemy.ChillUntil = Mathf.Max(enemy.ChillUntil, Elapsed + (enemy.Definition.IsBoss ? 0.6f : 1.5f));
                    }
                if (mechanic == HeroMechanic.DreamBreath)
                {
                    int removed = 0;
                    for (int index = projectiles.Count - 1; index >= 0 && removed < 3; index--)
                        if (projectiles[index].Faction == Faction.Enemy && (projectiles[index].Position - Champion.Position).sqrMagnitude <= 9) { projectiles.RemoveAt(index); removed++; }
                }
                events.Add(new CombatEvent(CombatEventKind.SignaturePulse, centre, Faction.Champion, radius, subject: Champion, element: mechanic == HeroMechanic.LanternStride ? DamageElement.Ember : DamageElement.Frost, cause: cause));
            }
            events.Add(new CombatEvent(CombatEventKind.Support, Champion.Position, Faction.Champion, subject: Champion, cause: cause));
        }
    }
}
