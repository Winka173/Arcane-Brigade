using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The battle side of the hero kits (18/10/2026): conditional damage, attack speed and crit for the hero and its
    /// friends, damage the hero takes and dodges, enemy slows, hostile shot speed, coins and chests. Everything reads the hero's
    /// kit (HeroKits); a hero without a kit is untouched.</summary>
    public sealed partial class CombatSimulation
    {
        private HeroKit kit;
        private HeroKit Kit => HeroIdentityEnabled ? kit ??= MetaProgression.Habit(HeroKits.For(IdentityId), catalog.MetaSecondHabit) : null;
        private float KitValue(KitPassive kind) => Kit?.Value(kind) ?? 0;
        private bool KitHas(KitPassive kind) => Kit != null && Kit.Has(kind);
        private float kitHurtUntil = -100, kitDodgeUntil = -100, kitStillArmed = -1, kitCracks, kitHealAt, kitLeadHealAt, kitLidAt, kitRootAt, kitThornAt;
        private int kitKills, kitInjuryGold, kitChestsPaid, kitSpeciesPaid, kitHitsForHeal, kitRecorded;
        private readonly HashSet<string> kitSpecies = new HashSet<string>();

        private void ResetKit()
        {
            kit = null; kitHurtUntil = kitDodgeUntil = -100; kitStillArmed = -1; kitCracks = 0; kitHealAt = kitLeadHealAt = kitLidAt = kitRootAt = kitThornAt = 0;
            kitKills = kitInjuryGold = kitChestsPaid = kitSpeciesPaid = kitHitsForHeal = kitRecorded = 0; kitSpecies.Clear();
            kitProbedCoins = kitFrozenGold = 0; kitBallAt = 0;
        }

        private bool IsLead(UnitState u) => u != null && u.Faction == Faction.Companion && (identityLead == u || identityLead == null && u.Definition.Id == catalog.HeroLeadUnit);
        private bool Marked(UnitState u) { foreach (var m in identityMarks) if (m.Target == u) return true; return false; }
        private bool Recorded(UnitState u) => u != null && identitySpecimens.Exists(d => d.Id == u.Definition.Id);
        /// <summary>The attacker stands behind the target (against the way it faces).</summary>
        private static bool Behind(UnitState attacker, UnitState target)
        {
            if (attacker == null || target == null) return false;
            Vector2 toAttacker = attacker.Position - target.Position; if (toAttacker.sqrMagnitude < .0001f) return false;
            Vector2 facing = target.Facing.sqrMagnitude > .0001f ? target.Facing.normalized : Vector2.right;
            return Vector2.Dot(toAttacker.normalized, facing) <= -.25f;
        }
        private float kitRootGripAt; private int kitInjuries;
        private int CompanionsNear(Vector2 at, float radius) { int n = 0; foreach (var u in units) if (u.Alive && u.Faction == Faction.Companion && (u.Position - at).sqrMagnitude <= radius * radius) n++; return n; }
        private int PetalsGrown() { int g = 0; for (int i = 0; i < 4 && i < catalog.HeroPetals.Length; i++) if (catalog.HeroPetals[i] > 0) g++; return g; }

        /// <summary>Damage dealt by the hero and its friends, scaled by the kit's conditions.</summary>
        private float KitDamageDealt(UnitState target, float damage, Faction source, CombatCause cause)
        {
            HeroKit k = Kit; if (k == null || target.Faction != Faction.Enemy || source == Faction.Enemy) return damage;
            UnitState attacker = UnitWithId(cause.SourceUnitId);
            float scale = 1;
            // anything of ours
            if (k.Has(KitPassive.EnemyVulnerableNearHero) && (target.Position - Champion.Position).sqrMagnitude <= 16) scale += k.Value(KitPassive.EnemyVulnerableNearHero);
            if (target.KitCrackedUntil > Elapsed) scale += k.Value(KitPassive.EnemyCrackOnHit);
            if (source == Faction.Champion && cause.SourceUnitId == Champion.Id)
            {
                if (Marked(target) && k.Has(KitPassive.Stamps)) scale += .1f;
                if (Recorded(target)) scale += k.Value(KitPassive.HeroDamageVsRecorded);
                scale += kitCracks * k.Value(KitPassive.CrackDamage);
                if (k.Has(KitPassive.DamageWithAllies) && CompanionCount >= 3) scale += k.Value(KitPassive.DamageWithAllies);
                if (cause.Depth == 0 && kitStillArmed > 0) { scale += k.Value(KitPassive.StillNextAttack); kitStillArmed = -1; }
                if (cause.Depth == 0 && k.Has(KitPassive.RearDamage) && (Behind(Champion, target) || HeroAwakening && k.Action == KitAction.Eclipse)) scale += k.Value(KitPassive.RearDamage);
                if (k.Has(KitPassive.DiceFaceBuff) && identityFace == 1) scale += .1f;
                if (k.Has(KitPassive.FarShotDamage) && (target.Position - Champion.Position).sqrMagnitude > 16) scale += k.Value(KitPassive.FarShotDamage);
            }
            else if (attacker != null && attacker.Faction == Faction.Companion)
            {
                if (Marked(target)) scale += k.Value(KitPassive.TeamDamageVsMarked) + k.Value(KitPassive.TeamDamageVsPinned);
                if (target == identityBounty) scale += k.Value(KitPassive.TeamDamageVsBounty);
                if (target.ChillUntil > Elapsed || target.KitSlowUntil > Elapsed) scale += k.Value(KitPassive.TeamDamageVsSlowed);
                if (target.Health < target.Definition.Health * .3f) scale += k.Value(KitPassive.TeamDamageVsLowHealth);
                if (Behind(attacker, target)) scale += k.Value(KitPassive.TeamDamageVsRear);
                if (Recorded(target)) scale += k.Value(KitPassive.TeamDamageVsRecorded);
                if (target.KitShockedUntil > Elapsed) scale += k.Value(KitPassive.TeamDamageVsShocked);
                if (target.KitWetUntil > Elapsed) scale += k.Value(KitPassive.TeamDamageVsWet);
                if (target.ProbedUntil > Elapsed) scale += k.Value(KitPassive.TeamDamageVsProbed);
                if ((target.Position - Champion.Position).sqrMagnitude <= 2.2f * 2.2f) scale += k.Value(KitPassive.TeamDamageNearHero);
                if (Champion.Health >= Champion.Definition.Health * .5f) scale += k.Value(KitPassive.TeamDamageWhileHeroHealthy);
                scale += Mathf.Min(.1f, Mathf.Floor(catalog.HeroWallet / 50f) * k.Value(KitPassive.TeamDamagePerGold));
                scale += Mathf.Min(.15f, catalog.HeroFood * k.Value(KitPassive.TeamDamagePerFood));
                scale += PetalsGrown() * k.Value(KitPassive.TeamDamagePerPetal);
                if (kitHurtUntil > Elapsed && (attacker.Position - Champion.Position).sqrMagnitude <= 16) scale += k.Value(KitPassive.TeamDamageOnHeroHurt);
                Vector2 heading = identityHeading.sqrMagnitude > .01f ? identityHeading : Champion.Facing;
                if (Vector2.Dot(attacker.Position - Champion.Position, heading) < 0) scale += k.Value(KitPassive.TeamDamageBehindHero);
                if (IsLead(attacker)) scale += k.Value(KitPassive.TeamLeadDamage);
                if (!target.KitStruck) { target.KitStruck = true; scale += k.Value(KitPassive.TeamFirstHitBonus); }
                if (HeroAwakening && k.Action == KitAction.Parade) scale += .5f; // Knighting: every friend fights one star higher
            }
            if (scale > 1)
            {
                // the hero's own conditional bonus is the hero's; a friend's is a team passive (measured apart)
                if (source == Faction.Champion && cause.SourceUnitId == Champion.Id) HeroIdentityBonusDamage += damage * (scale - 1);
                else HeroKitTeamBonusDamage += damage * (scale - 1);
            }
            return damage * scale;
        }

        /// <summary>The hero's dodge chance from the kit (ButtonCavalier's poise).</summary>
        private float KitDodge => Kit != null && Kit.Has(KitPassive.PoiseDodge) && IdentityId == "ButtonCavalier" ? Mathf.Min(3, identityCharge) * KitValue(KitPassive.PoiseDodge) : 0;

        /// <summary>How much of a blow the hero takes: armour from the kit, the transformation's guard, golden or royal immunity.</summary>
        private float KitDamageTaken(CombatCause cause)
        {
            HeroKit k = Kit; if (k == null) return 1;
            float scale = 1;
            if (k.Has(KitPassive.ArmorNearAllies) && CompanionsNear(Champion.Position, 3f) >= 2) scale *= 1 - k.Value(KitPassive.ArmorNearAllies);
            scale *= 1 - k.Value(KitPassive.HeroArmor);
            if (HeroAwakening)
            {
                scale *= k.HeroDamageTaken;
                if (k.Action == KitAction.Kintsugi && Elapsed - awakenStarted < 3) scale = 0;
            }
            return scale;
        }

        /// <summary>Attack cooldown multiplier for the hero or a friend (below 1 is faster).</summary>
        private float KitCycle(UnitState unit)
        {
            HeroKit k = Kit; if (k == null || unit.Faction == Faction.Enemy) return 1;
            float bonus = 0;
            if (unit == Champion)
            {
                if (HeroAwakening) bonus += k.HeroAttackSpeed;
                if (Elapsed < kitHurtUntil) bonus += k.Value(KitPassive.AttackSpeedWhenHurt);
                if (k.Has(KitPassive.DiceFaceBuff) && identityFace == 2) bonus += .1f;
            }
            else
            {
                bool near = (unit.Position - Champion.Position).sqrMagnitude <= 16;
                if (HeroAwakening && k.TeamAttackSpeed > 0 && (k.Action != KitAction.Lighthouse || (unit.Position - Champion.Position).sqrMagnitude <= 25)) bonus += k.TeamAttackSpeed;
                if (HeroAwakening && k.Action == KitAction.TimeGarden && (unit.Position - awakenPlot).sqrMagnitude <= GardenReach * GardenReach) bonus += .4f; // friends beside the bonsai
                if (near) bonus += k.Value(KitPassive.TeamAttackSpeedNearHero);
                if (Elapsed < 5) bonus += k.Value(KitPassive.TeamAttackSpeedWaveStart);
                bonus += Mathf.Min(.1f, kitKills * k.Value(KitPassive.TeamAttackSpeedPerKill));
                if (identityBank > 0) bonus += k.Value(KitPassive.TeamAttackSpeedWhileDebt);
                if (Elapsed < kitHurtUntil && near) bonus += k.Value(KitPassive.TeamAttackSpeedOnHeroHurt);
                bonus += Mathf.Min(.1f, Mathf.Floor(catalog.HeroWallet / 50f) * k.Value(KitPassive.TeamAttackSpeedPerGold));
                bonus += Mathf.Min(.2f, catalog.HeroRelics * k.Value(KitPassive.TeamAttackSpeedPerRelic));
                if (PetalsGrown() == 4) bonus += k.Value(KitPassive.TeamAttackSpeedFullGarden);
                if (k.Has(KitPassive.LeadStarAttackSpeed) && identityLead != null && identityLead.Definition.CombatTier >= 2) bonus += k.Value(KitPassive.LeadStarAttackSpeed);
                bonus += Mathf.Min(.15f, catalog.HeroBondLevels * k.Value(KitPassive.BondLevelAttackSpeed));
            }
            return 1f / (1f + Mathf.Max(0, bonus));
        }

        /// <summary>Lantern Weaver: a friend's shot passing through a pinned enemy pierces one more (once per shot).</summary>
        private bool KitPinnedPierce(ProjectileState projectile, UnitState hit)
        {
            HeroKit k = Kit;
            if (k == null || !k.Has(KitPassive.PinnedPierce) || projectile.PinnedPierced || projectile.Faction == Faction.Enemy || !Marked(hit)) return false;
            if (UnitWithId(projectile.SourceUnitId)?.Faction != Faction.Companion) return false;
            projectile.PinnedPierced = true; return true;
        }
        /// <summary>Bell Diver: a friend's shot that strikes a probed enemy bounces once more (once per shot).</summary>
        private bool KitProbedBounce(ProjectileState projectile, UnitState hit)
        {
            HeroKit k = Kit;
            if (k == null || !k.Has(KitPassive.ProbedBounce) || projectile.ProbedBounced || projectile.Faction == Faction.Enemy || hit.ProbedUntil <= Elapsed) return false;
            if (UnitWithId(projectile.SourceUnitId)?.Faction != Faction.Companion) return false;
            projectile.ProbedBounced = true; return true;
        }
        /// <summary>Shots a friend just fired (projectiles from `first` on): Kite Admiral's pierce while the gauge is full and the
        /// kite fleet's split shots for the friends a kite flies over.</summary>
        private void KitEmitted(UnitState unit, int first)
        {
            HeroKit k = Kit;
            if (k == null || unit.Faction != Faction.Companion || first >= projectiles.Count || unit.Definition.Weapon.Kind != WeaponKind.Projectile) return;
            bool pierce = k.Has(KitPassive.RangedPierceWhileFull) && (HeroAwakening || HeroAwakenPrimed);
            bool fleet = HeroAwakening && k.Action == KitAction.KiteFleet && awakenFleet.Contains(unit);
            int count = projectiles.Count;
            for (int i = first; i < count; i++)
            {
                ProjectileState p = projectiles[i];
                if (p.Faction != Faction.Companion) continue;
                if (pierce) p.ExtraPierce += Mathf.RoundToInt(k.Value(KitPassive.RangedPierceWhileFull));
                if (!fleet || p.IsFragment) continue;
                // the kite splits the shot: two shots (tight, long voyage) or three with lighter outer ones (wide formation)
                if (IdentityApproach == 1) { KitTwin(p, 16, .6f); KitTwin(p, -16, .6f); }
                else { KitTwin(p, 8, 1); p.Velocity = Quaternion.Euler(0, 0, -4) * p.Velocity; p.LaunchVelocity = Quaternion.Euler(0, 0, -4) * p.LaunchVelocity; }
            }
        }
        private void KitTwin(ProjectileState a, float degrees, float share)
        {
            if (projectiles.Count >= 4096) return;
            var turn = Quaternion.Euler(0, 0, degrees);
            projectiles.Add(new ProjectileState { Id = ++nextId, Faction = a.Faction, Position = a.Position, PreviousPosition = a.PreviousPosition, Velocity = turn * a.Velocity,
                LaunchVelocity = turn * a.LaunchVelocity, Damage = a.Damage * share, Remaining = a.Remaining, TravelLeft = a.TravelLeft, Color = a.Color, Element = a.Element, SourceName = a.SourceName,
                LaunchSegment = a.LaunchSegment, Sprite = a.Sprite, VisualWidth = a.VisualWidth, Radius = a.Radius, Critical = a.Critical, SourceUnitId = a.SourceUnitId,
                Cause = a.Cause.Child("kite-split"), Weapon = a.Weapon, SplitCount = a.SplitCount, SplitOnImpact = a.SplitOnImpact, Pattern = a.Pattern, ExtraPierce = a.ExtraPierce,
                Arsenal = a.Arsenal, ArsenalStage = a.ArsenalStage, ArsenalTimer = a.ArsenalTimer, ArsenalHome = a.ArsenalHome, ArsenalTick = a.ArsenalTick,
                OrbitTargetId = a.OrbitTargetId, OrbitCenter = a.OrbitCenter, RicochetsRemaining = a.RicochetsRemaining, FinalNote = a.FinalNote, IsFragment = true });
        }
        /// <summary>Copper Cauldron: friends' collision damage grows with the blows the hero took this wave.</summary>
        private float KitHurtCollision(UnitState source)
            => source.Faction == Faction.Companion && Kit != null ? Mathf.Min(10, kitInjuries * Kit.Value(KitPassive.HurtCollision)) : 0;

        /// <summary>Extra crit chance for the hero or a friend against a target.</summary>
        private float KitCrit(UnitState unit, UnitState target)
        {
            HeroKit k = Kit; if (k == null || unit.Faction == Faction.Enemy) return 0;
            if (unit == Champion) return (HeroAwakening ? k.HeroCrit : 0) + (k.Has(KitPassive.DiceFaceBuff) && identityFace == 3 ? .1f : 0);
            float c = 0;
            if (Elapsed < kitDodgeUntil) c += k.Value(KitPassive.TeamCritOnHeroDodge);
            if (Recorded(target)) c += k.Value(KitPassive.TeamCritVsRecorded);
            if (k.Has(KitPassive.TeamRearCrit) && unit.Definition.Weapon.Kind == WeaponKind.Melee && Behind(unit, target)) c += 1;
            return c;
        }

        /// <summary>How fast an enemy moves under the kit's slows (and the transformations that slow or stop the field).</summary>
        private float KitEnemySpeed(UnitState enemy)
        {
            HeroKit k = Kit; if (k == null || enemy.Faction != Faction.Enemy) return 1;
            float slow = 0;
            if (k.Has(KitPassive.EnemySlowNearHero) && (enemy.Position - Champion.Position).sqrMagnitude <= 1.8f * 1.8f) slow = Mathf.Max(slow, k.Value(KitPassive.EnemySlowNearHero));
            if (enemy.KitSlowUntil > Elapsed) slow = Mathf.Max(slow, enemy.KitSlow);
            if (k.Has(KitPassive.Pins) && Marked(enemy)) slow = Mathf.Max(slow, .1f);
            if (k.Has(KitPassive.EnemySlowWithDebt) && enemy == identityDebtor && identityBank > 0) slow = Mathf.Max(slow, k.Value(KitPassive.EnemySlowWithDebt));
            if (HeroAwakening)
            {
                if (k.Action == KitAction.Rink) slow = Mathf.Max(slow, .4f);
                if (k.Action == KitAction.Tapestry && Marked(enemy)) slow = Mathf.Max(slow, .4f);
                if (k.Action == KitAction.Foreclosure && Elapsed - awakenStarted < 1.5f) slow = 1;
            }
            if (enemy.Definition.IsBoss) slow = Mathf.Min(slow, .3f);
            return 1 - Mathf.Clamp01(slow);
        }
        private void KitSlowEnemy(UnitState enemy, float amount, float seconds)
        {
            if (enemy == null || !enemy.Alive || amount <= 0) return;
            if (enemy.KitSlowUntil <= Elapsed || amount >= enemy.KitSlow) enemy.KitSlow = amount;
            enemy.KitSlowUntil = Mathf.Max(enemy.KitSlowUntil, Elapsed + seconds);
        }

        private float KitHostileShotScale => 1 - KitValue(KitPassive.EnemyShotSlow);
        /// <summary>BULL MARKET: coins count double while it runs.</summary>
        private int KitCoinScale => HeroAwakening && Kit.Action == KitAction.BullMarket ? 2 : 1;
        /// <summary>Extra gold in the first chest of each wave.</summary>
        private int KitChestBonus()
        {
            if (kitChestsPaid > 0 || !KitHas(KitPassive.ChestBonusGold)) return 0;
            kitChestsPaid++; return Mathf.RoundToInt(KitValue(KitPassive.ChestBonusGold));
        }
        /// <summary>The hero's own move speed under its kit.</summary>
        private float KitHeroSpeed()
        {
            HeroKit k = Kit; if (k == null) return 1;
            float bonus = HeroAwakening ? k.HeroMoveSpeed : 0;
            if (k.Has(KitPassive.SprintSpeed) && identityRun >= 2) bonus += k.Value(KitPassive.SprintSpeed);
            if (k.Has(KitPassive.DiceFaceBuff) && identityFace == 4) bonus += .1f;
            return 1 + bonus;
        }
        private float identityRun;
        private void KitDropCoin(Vector2 at, int value = 1) { if (value > 0) DropLoot(LootKind.Coin, at, value); }
    }
}
