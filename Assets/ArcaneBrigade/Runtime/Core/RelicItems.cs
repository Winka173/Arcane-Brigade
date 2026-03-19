using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Relic mechanics of content expansion step 3 (Docs/Content_Expansion_v5.md §5): the Mythics that only added stats
    /// and the four redesigned old Prismatics.</summary>
    public sealed partial class CombatSimulation
    {
        private bool relicQuiltUsed;
        private float relicQuiltUntil = -1, relicApronAt, relicForwardAt, relicCrownAt;
        private int relicCrownHits;

        /// <summary>Tarnished Crown: every 8th hit of the hero is a royal blow (triple damage).</summary>
        private float RelicItemDamage(UnitState target, float damage, Faction source, CombatCause cause)
        {
            if (target.Faction != Faction.Enemy || source == Faction.Enemy) return damage;
            int stamps = RelicCount(RelicMechanic.PostageStamp), bells = RelicCount(RelicMechanic.DivingBellLantern);
            if (stamps + bells > 0 && RelicMarked(target)) damage *= 1 + .1f * stamps + .15f * bells;
            if (source != Faction.Champion || cause.Depth != 0 || cause.SourceUnitId != Champion.Id || RelicCount(RelicMechanic.RoyalBlow) == 0) return damage;
            if (++relicCrownHits % 8 != 0) return damage;
            events.Add(new CombatEvent(CombatEventKind.Proc, target.Position, Faction.Champion, damage * 2, cause: cause.Child("curio-royal")));
            return damage * 3;
        }

        /// <summary>Starlight Quilt: once per wave a defeating blow leaves 1 HP and 1.5s of safety.</summary>
        private float RelicQuilt(float damage)
        {
            if (relicQuiltUsed || damage < Champion.Health || RelicCount(RelicMechanic.QuiltGuard) == 0) return damage;
            relicQuiltUsed = true; relicQuiltUntil = Elapsed + 1.5f;
            events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 1, subject: Champion, cause: NewCause(Champion.Id).Child("curio-quilt")));
            return Mathf.Max(0, Champion.Health - 1);
        }

        /// <summary>Mending Moth: after 3s without a hit the hero's regeneration is tripled.</summary>
        private float RelicMendingScale => RelicCount(RelicMechanic.QuietMending) > 0 && Elapsed - Champion.LastHit >= 3 ? 3 : 1;

        /// <summary>Iron Apron: an enemy striking the hero up close is shoved back.</summary>
        private void RelicApron(CombatCause cause)
        {
            if (RelicCount(RelicMechanic.ApronShove) == 0 || Elapsed < relicApronAt) return;
            UnitState attacker = UnitWithId(cause.SourceUnitId);
            if (attacker == null || !attacker.Alive || attacker.Faction != Faction.Enemy || attacker.Definition.IsBoss) return;
            Vector2 away = attacker.Position - Champion.Position;
            if (away.sqrMagnitude > 6.25f) return;
            relicApronAt = Elapsed + 1;
            away = away.sqrMagnitude > .0001f ? away.normalized : Vector2.right;
            attacker.Position = ClampPosition(attacker.Position + away * 1.5f, attacker.Definition.Radius); spatial.Move(attacker);
            events.Add(new CombatEvent(CombatEventKind.Knockback, attacker.Position, Faction.Champion, direction: away, cause: NewCause(Champion.Id).Child("curio-apron")));
        }

        /// <summary>Shield Blower: an intercepted hostile shot is blown back as a friendly one (true keeps it flying).</summary>
        private bool RelicBlowBack(ProjectileState projectile, UnitState companion)
        {
            if (projectile.Faction != Faction.Enemy || RelicCount(RelicMechanic.BlowerReturn) == 0) return false;
            projectile.Faction = Faction.Companion; projectile.Velocity = -projectile.Velocity; projectile.Damage *= 2;
            projectile.SourceUnitId = companion.Id; projectile.Cause = NewCause(companion.Id).Child("curio-blower");
            projectile.Victims.Clear(); projectile.Victims.Add(companion.Id); projectile.Remaining = Mathf.Max(projectile.Remaining, 1.2f);
            return true;
        }

        /// <summary>Impossible Prism: friends' split shots break into ember, frost and arcane.</summary>
        private void RelicPrismFragment(ProjectileState fragment, int branch)
        {
            if (fragment.Faction == Faction.Enemy || RelicCount(RelicMechanic.ImpossiblePrism) == 0) return;
            switch (branch % 3)
            {
                case 0: fragment.Element = DamageElement.Ember; fragment.Color = new Color(1f, .5f, .25f); break;
                case 1: fragment.Element = DamageElement.Frost; fragment.Color = new Color(.45f, .85f, 1f); break;
                default: fragment.Element = DamageElement.Arcane; fragment.Color = new Color(.8f, .45f, 1f); break;
            }
        }

        /// <summary>Crimson Seal: a collision stamps a seal that bursts 1s later.</summary>
        private void RelicSeal(UnitState enemy, float collision)
        {
            if (!enemy.Alive || enemy.SealAt > Elapsed || RelicCount(RelicMechanic.SealBurst) == 0 || collision <= 0) return;
            enemy.SealAt = Elapsed + 1; enemy.SealDamage = collision * 3;
        }
        private void RelicSeals()
        {
            if (RelicCount(RelicMechanic.SealBurst) == 0) return;
            foreach (UnitState sealed_ in units.ToArray())
            {
                if (sealed_.SealAt < 0 || sealed_.SealAt > Elapsed) continue;
                float damage = sealed_.SealDamage; Vector2 at = sealed_.Position; sealed_.SealAt = -1;
                CombatCause cause = NewCause(Champion.Id).Child("curio-seal");
                events.Add(new CombatEvent(CombatEventKind.Detonation, at, Faction.Champion, damage, cause: cause));
                foreach (UnitState enemy in units.ToArray())
                    if (enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - at).sqrMagnitude <= 1.2f * 1.2f)
                        ApplyDamage(enemy, damage, Faction.Champion, "Crimson seal", enemy.Position - at, cause: cause);
            }
        }

        /// <summary>Borrowed Crown: the highest-star companion also fires the hero's weapon, at the hero weapon's own rhythm.</summary>
        private void RelicBorrowedCrown()
        {
            if (RelicCount(RelicMechanic.BorrowedCrown) == 0 || Elapsed < relicCrownAt) return;
            WeaponDefinition weapon = Champion.Definition.Weapon;
            UnitState bearer = null;
            foreach (UnitState u in units)
                if (u.Alive && u.Faction == Faction.Companion && (bearer == null || u.Definition.CombatTier > bearer.Definition.CombatTier)) bearer = u;
            if (bearer == null) return;
            UnitState target = FindTarget(bearer, Mathf.Max(4, weapon.Range));
            if (target == null) return;
            relicCrownAt = Elapsed + Mathf.Max(.25f, weapon.Cooldown);
            CombatCause cause = NewCause(bearer.Id).Child("curio-crown");
            Vector2 aim = (target.Position - bearer.Position).sqrMagnitude > .0001f ? (target.Position - bearer.Position).normalized : Vector2.right;
            if (weapon.Kind == WeaponKind.Projectile) { Vector2 kept = bearer.AttackDirection; bearer.AttackDirection = aim; EmitProjectile(bearer, weapon.Damage, false, cause, weapon); bearer.AttackDirection = kept; }
            else
                foreach (UnitState enemy in units.ToArray())
                    if (CanHit(Faction.Companion, enemy) && (enemy.Position - bearer.Position).magnitude <= weapon.Range + enemy.Definition.Radius)
                        ApplyWeaponDamage(enemy, weapon.Damage, Faction.Companion, weapon.Element, "Borrowed crown", (enemy.Position - bearer.Position).normalized, weapon, false, cause);
            events.Add(new CombatEvent(CombatEventKind.Fire, bearer.Position, Faction.Companion, direction: aim, weapon: weapon, subject: bearer, cause: cause));
        }

        /// <summary>Event-driven item mechanics: Echo Postage, Crimson Spring and Living Thread.</summary>
        private void RelicItemEvent(CombatEvent e)
        {
            if (e.Cause.Depth != 0) return;
            UnitState performer = UnitWithId(e.Cause.SourceUnitId);
            if (performer == null || performer.Faction != Faction.Companion) return;
            if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy && Elapsed >= relicForwardAt && RelicCount(RelicMechanic.EchoForward) > 0)
            {
                UnitState next = NearestEnemy(e.Position, 4);
                if (next != null)
                {
                    relicForwardAt = Elapsed + .2f; CombatCause cause = e.Cause.Child("curio-echo");
                    ApplyDamage(next, performer.Definition.Weapon.Damage * .5f, Faction.Companion, "Echo postage", next.Position - e.Position, cause: cause);
                    events.Add(new CombatEvent(CombatEventKind.SignaturePulse, next.Position, Faction.Companion, 0, cause: cause));
                }
            }
            if (e.Kind == CombatEventKind.Fire && e.Faction == Faction.Companion && RelicCount(RelicMechanic.SpringDouble) > 0 && Elapsed >= performer.SpringAt)
            {
                performer.SpringAt = Elapsed + 4; performer.NextAttack = Mathf.Min(performer.NextAttack, Elapsed + .12f);
            }
            if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && e.Subject.Alive && RelicCount(RelicMechanic.LivingThread) > 0
                && performer.Definition.Weapon.Kind == WeaponKind.Melee)
            {
                UnitState partner = null; float best = 9;
                foreach (UnitState u in units)
                    if (u != performer && u.Alive && u.Faction == Faction.Companion && u.Definition.Weapon.Kind == WeaponKind.Melee && u.ThreadAt <= Elapsed)
                    { float d = (u.Position - performer.Position).sqrMagnitude; if (d < best) { best = d; partner = u; } }
                if (partner == null) return;
                partner.ThreadAt = Elapsed + .4f; CombatCause cause = NewCause(partner.Id).Child("curio-thread");
                ApplyDamage(e.Subject, partner.Definition.Weapon.Damage * .5f, Faction.Companion, "Living thread", e.Subject.Position - partner.Position, partner.Definition.Weapon, cause: cause);
                events.Add(new CombatEvent(CombatEventKind.SignaturePulse, e.Subject.Position, Faction.Companion, 0, cause: cause));
            }
        }
        private UnitState NearestEnemy(Vector2 at, float range)
        {
            UnitState best = null; float bestDistance = range * range;
            foreach (UnitState u in units)
                if (u.Alive && u.Faction == Faction.Enemy && u.EntranceUntil <= Elapsed) { float d = (u.Position - at).sqrMagnitude; if (d < bestDistance) { bestDistance = d; best = u; } }
            return best;
        }
    }
}
