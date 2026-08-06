using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CheckpointCatalog
    {
        [NonSerialized] public BondDefinition[] RuntimeBonds = Array.Empty<BondDefinition>();
        [NonSerialized] public Dictionary<string, int> RuntimeBondCounts = new Dictionary<string, int>();
    }

    public sealed partial class CombatSimulation
    {
        public BondRuntime Bonds { get; private set; }
        private readonly Queue<BondDamageRequest> bondDamage = new Queue<BondDamageRequest>(BondTuning.RequestCapacity);
        private readonly HashSet<int> bondRoots = new HashSet<int>(BondTuning.RequestCapacity);
        private CombatCause BondCause(int sourceId, string rule)
        {
            CombatCause cause = NewCause(sourceId).Child(rule=="execute"?"bond-execute":rule=="eclipse"?"bond-eclipse":"bond-generated");
            bondRoots.Add(cause.RootId); return cause;
        }
        private void StartBonds()
        {
            bondDamage.Clear(); bondRoots.Clear();
            Bonds = new BondRuntime(catalog.RuntimeBonds, catalog.RuntimeBondCounts, catalog.RulesV3BondProgress);
            Bonds.DamageRequested += request => bondDamage.Enqueue(request);
            Bonds.BonusShotRequested += (attacker, damage) =>
            {
                WeaponDefinition weapon = attacker.Definition.Weapon;
                Vector2 direction = attacker.AttackDirection.sqrMagnitude > .0001f ? attacker.AttackDirection.normalized : attacker.Facing;
                projectiles.Add(new ProjectileState
                {
                    Id = ++nextId, Faction = attacker.Faction, Position = attacker.Position, PreviousPosition = attacker.Position,
                    Velocity = direction * weapon.ProjectileSpeed, Damage = damage, Remaining = weapon.ProjectileLifetime,
                    TravelLeft = FriendlyReach.Flight(weapon, 0) * (1 + Bonds.RangeBonusFor(attacker)), SourceUnitId = attacker.Id,
                    Cause = BondCause(attacker.Id, "gear"), BondBonusShot=true, Weapon = weapon, Element = weapon.Element,
                    Color = weapon.ProjectileColor, Radius = weapon.ProjectileRadius, Sprite = weapon.ProjectileSprite,
                    VisualWidth = weapon.ProjectileVisualWidth
                });
            };
            Bonds.ResetWave(Champion, units);
        }
        // Resolve after the triggering attack has finished, preserving the normal death and loot path.
        private void ResolveBondDamage()
        {
            while (bondDamage.Count > 0 && Phase == RunPhase.Combat)
            {
                BondDamageRequest request = bondDamage.Dequeue();
                if (!request.Target.Alive)continue;
                ApplyDamage(request.Target, request.Damage, request.Attacker?.Faction ?? Faction.Companion,
                    cause: BondCause(request.Attacker?.Id ?? 0, request.Rule));
                if(!request.Target.Alive)Bonds.OnGeneratedKill(request.Rule,request.Attacker,request.Target,request.Damage);
            }
        }
        private bool BondGenerated(CombatCause cause) => bondRoots.Contains(cause.RootId) ||
            cause.ItemId != null && cause.ItemId.StartsWith("bond-", StringComparison.Ordinal);
        private float BondHitDamage(UnitState target, float damage, Faction source, bool critical, CombatCause cause)
        {
            if (Bonds == null) return damage;
            UnitState attacker = UnitWithId(cause.SourceUnitId);
            if (source == Faction.Enemy && attacker != null) damage *= Bonds.OutgoingScale(attacker);
            if (target.Faction == Faction.Enemy && source != Faction.Enemy && !BondGenerated(cause))
                damage = Bonds.ModifyHit(attacker, target, damage, critical);
            return damage;
        }
        private void BondAfterHit(UnitState target, float damage, Faction source, bool critical, CombatCause cause)
        {
            if (Bonds != null && target.Faction == Faction.Enemy && source != Faction.Enemy && !BondGenerated(cause))
                Bonds.OnHit(UnitWithId(cause.SourceUnitId), target, damage, critical);
            else if (Bonds != null && target.Faction == Faction.Enemy) Bonds.CheckExecute(target, UnitWithId(cause.SourceUnitId));
        }
    }
}
