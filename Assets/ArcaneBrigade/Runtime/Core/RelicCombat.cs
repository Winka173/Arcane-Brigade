using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private float relicPulseAt;
        private float relicSecondAt;
        private float relicEchoAt;
        private int relicProcessedEvents;
        private bool relicMoved;
        private bool relicStanding;
        private int relicLastKills;
        private float relicDamageLoss;
        private bool lotusActive;
        private readonly List<UnitState> phantomWeapons = new List<UnitState>();
        public int RelicCount(RelicMechanic mechanic) => catalog.RelicStacks != null && (int)mechanic < catalog.RelicStacks.Length ? catalog.RelicStacks[(int)mechanic] : 0;

        private void ResetRelics()
        {
            relicQuiltUsed = false; relicQuiltUntil = -1; relicCrownHits = 0; relicApronAt = relicForwardAt = relicCrownAt = 0;
            ResetNewRelics();
            relicPulseAt = 3; relicSecondAt = 1; relicEchoAt = 0; relicMoved = false;
            relicLastKills = relicProcessedEvents = 0; relicDamageLoss = 0; phantomWeapons.Clear();
            lotusActive = RelicCount(RelicMechanic.FreshLotus) > 0;
            if (lotusActive) AdjustLotus(1);
            if (RelicCount(RelicMechanic.SoloOverdrive) > 0) Champion.Definition.Weapon.Cooldown = Mathf.Max(.1f,Champion.Definition.Weapon.Cooldown / 6);
            if (RelicCount(RelicMechanic.SixWeapons) == 0) return;
            for (int index = 0; index < 6; index++)
            {
                UnitDefinition template = catalog.RelicWeapons.Length == 6 ? catalog.RelicWeapons[index] : Champion.Definition;
                phantomWeapons.Add(new UnitState { Id = Champion.Id, Faction = Faction.Champion, Definition = template, Alive = true, NextAttack = index * .13f });
            }
        }

        private bool RelicBlocksAttack(UnitState unit)
        {
            if (unit.Faction == Faction.Enemy) return false;
            if (RelicCount(RelicMechanic.SixWeapons) > 0) return true;
            if (unit.Faction == Faction.Champion && RelicCount(RelicMechanic.CollisionOnly) > 0) return true;
            return unit.Faction == Faction.Companion && (RelicCount(RelicMechanic.SoloOverdrive) > 0 || RelicCount(RelicMechanic.SoloOverture) > 0); // Solo Overture: friends rest for good
        }

        private void AdjustLotus(int direction)
        {
            int count = RelicCount(RelicMechanic.FreshLotus);
            foreach (UnitState ally in StableUnits)
                if (ally.Faction != Faction.Enemy)
                {
                    ally.Definition.Weapon.ProjectilePierce += direction * 3 * count;
                    ally.Definition.CriticalChance += direction * .3f * count;
                }
        }

        private float RelicAttackDamage(UnitState unit, float damage)
        {
            if (unit.Faction == Faction.Enemy) return damage;
            float flat = !relicMoved ? 16 * RelicCount(RelicMechanic.StillPower) : 0;
            float hoard = RelicCount(RelicMechanic.HoardPower);
            float purse = 1 + hoard * RunSession.HoardBonus(catalog.RelicWallet + (long)Gold, catalog.ChestGoldScale);
            float initial = 1 + hoard * RunSession.HoardBonus(catalog.RelicWallet, catalog.ChestGoldScale);
            float result = Mathf.Max(0,damage + flat) * Mathf.Max(.05f,1 - relicDamageLoss) * purse / initial;
            result *= RelicAttackScale(unit);
            return RelicCount(RelicMechanic.StatFloor) > 0 ? Mathf.Max(damage,result) : result;
        }

        /// <summary>Stats gained (or lost) for good during a wave, for the presentation to show over the hero the way the shop
        /// does (requirement 24/10/2026); the arena reads and empties it each frame.</summary>
        public readonly List<KeyValuePair<ItemEffect, float>> StatGainsToShow = new List<KeyValuePair<ItemEffect, float>>();
        private void RelicGain(ItemEffect effect, float value, ModifierScope? scope = null)
        {
            if (value == 0) return;
            StatGainsToShow.Add(new KeyValuePair<ItemEffect, float>(effect, value));
            catalog.RelicGain?.Invoke(new StatModifier { Effect = effect, Value = value, Scope = scope ?? RunModifierRules.DefaultScope(effect) });
            if (effect == ItemEffect.Health) { Champion.Definition.Health += value; Champion.Health = Mathf.Min(Champion.Definition.Health,Champion.Health + Mathf.Max(0,value)); }
            if (effect == ItemEffect.Armor) catalog.Armor += value;
            if (effect == ItemEffect.Heal) catalog.PassiveHeal += value;
            if (effect == ItemEffect.Luck) catalog.Luck += value;
            if (effect == ItemEffect.Crit) foreach (UnitState ally in StableUnits) if (ally.Faction != Faction.Enemy) ally.Definition.CriticalChance += value;
            if (effect == ItemEffect.Damage)
                foreach (UnitState ally in StableUnits) if (ally.Faction != Faction.Enemy) ally.Definition.Weapon.Damage *= Mathf.Max(.05f,1 + value);
            events.Add(new CombatEvent(CombatEventKind.Proc,Champion.Position,Faction.Champion,value,cause:NewCause(Champion.Id).Child("curio-growth")));
        }

        private void RelicHit(UnitState target, Faction source, CombatCause cause)
        {
            if (target == Champion)
            {
                RelicApron(cause); RelicSundae();
                if (lotusActive) { AdjustLotus(-1); lotusActive = false; }
                relicDamageLoss += .05f * RelicCount(RelicMechanic.FragilePower);
                if (RelicCount(RelicMechanic.FragilePower) > 0) { catalog.RelicGain?.Invoke(new StatModifier { Effect = ItemEffect.Damage, Scope = ModifierScope.Team, Value = -.05f * RelicCount(RelicMechanic.FragilePower) }); StatGainsToShow.Add(new KeyValuePair<ItemEffect, float>(ItemEffect.Damage, -.05f * RelicCount(RelicMechanic.FragilePower))); }
                if (RelicCount(RelicMechanic.PanicMagnet) > 0 && combatRandom.NextDouble() < .05) foreach (PickupState pickup in pickups) pickup.Attracting = true;
                if (RelicCount(RelicMechanic.RetortVolley) > 0 && combatRandom.NextDouble() < .3) RelicVolley(8,12 * RelicCount(RelicMechanic.RetortVolley));
                if (RelicCount(RelicMechanic.HurtGrowth) > 0)
                {
                    ItemEffect[] effects = { ItemEffect.Health, ItemEffect.Heal, ItemEffect.Armor, ItemEffect.Damage, ItemEffect.Luck, ItemEffect.Crit };
                    int choice = combatRandom.Next(effects.Length);
                    float[] values = { 4,.08f,.015f,.02f,.02f,.01f };
                    RelicGain(effects[choice],values[choice] * RelicCount(RelicMechanic.HurtGrowth));
                }
            }
            else if (target.Faction == Faction.Enemy && source == Faction.Champion && RelicCount(RelicMechanic.IncomePoison) > 0 && cause.Depth == 0)
            {
                if (target.RelicPoisonUntil <= Elapsed) target.RelicPoisonAt = Elapsed + 1;
                target.RelicPoisonUntil = Elapsed + 4;
            }
        }

        private void RelicPickup(PickupState pickup)
        {
            if (pickup.Kind != LootKind.Coin) return;
            if (RelicCount(RelicMechanic.CoinMending) > 0 && combatRandom.NextDouble() < .15) Heal(2 * RelicCount(RelicMechanic.CoinMending),NewCause(Champion.Id).Child("curio-coin-heal"));
            if (RelicCount(RelicMechanic.CoinLightning) > 0) RelicStrike(Mathf.Max(4,catalog.Luck * 50) * RelicCount(RelicMechanic.CoinLightning),1,"curio-coin-strike");
        }

        private void RelicChest()
        {
            if (RelicCount(RelicMechanic.ChestOnlyRecovery) > 0) Heal(30 * RelicCount(RelicMechanic.ChestOnlyRecovery),NewCause(Champion.Id).Child("curio-chest-heal"));
            if (RelicCount(RelicMechanic.ChestLuck) > 0) RelicGain(ItemEffect.Luck,.06f * RelicCount(RelicMechanic.ChestLuck));
            if (RelicCount(RelicMechanic.ChestLightning) > 0) RelicStrike(Mathf.Max(8,catalog.TreasureDetection * 100) * RelicCount(RelicMechanic.ChestLightning),combatRandom.Next(1,4),"curio-chest-strike");
        }

        private void RelicStrike(float damage, int count, string id)
        {
            var enemies = new List<UnitState>();
            foreach (UnitState enemy in StableUnits) if (enemy.Alive && enemy.Faction == Faction.Enemy && enemy.EntranceUntil <= 0) enemies.Add(enemy);
            for (int index = 0; index < count && enemies.Count > 0; index++)
            {
                int choice = combatRandom.Next(enemies.Count); UnitState enemy = enemies[choice]; enemies.RemoveAt(choice);
                CombatCause cause = NewCause(Champion.Id).Child(id);
                ApplyDamage(enemy,damage,Faction.Champion,id,cause:cause);
                events.Add(new CombatEvent(CombatEventKind.SignaturePulse,enemy.Position,Faction.Champion,damage,cause:cause));
            }
        }

        private void RelicVolley(int count, float damage)
        {
            WeaponDefinition weapon = Champion.Definition.Weapon;
            for (int index = 0; index < count; index++)
            {
                float angle = index * Mathf.PI * 2 / count; Vector2 direction = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                projectiles.Add(new ProjectileState { Id = ++nextId,Faction = Faction.Champion,Position = Champion.Position,PreviousPosition = Champion.Position,Velocity = direction * 5,Damage = damage,Remaining = 2,Color = new Color(.55f,.9f,1),Sprite = catalog.Projectile,VisualWidth = .35f,Radius = .12f,SourceUnitId = Champion.Id,Weapon = weapon,Cause = NewCause(Champion.Id).Child("curio-retort") });
            }
            events.Add(new CombatEvent(CombatEventKind.Fire,Champion.Position,Faction.Champion,weapon:weapon,cause:NewCause(Champion.Id).Child("curio-retort")));
        }

        private void StepRelics(float deltaTime, Vector2 movement)
        {
            relicProcessedEvents = events.Count;
            relicStanding = movement.sqrMagnitude < .001f;
            if (!relicStanding) relicMoved = true;
            if (RelicCount(RelicMechanic.StillRecovery) > 0 && relicStanding) Heal(3 * RelicCount(RelicMechanic.StillRecovery) * deltaTime,NewCause(Champion.Id).Child("curio-still-heal"));
            if (Kills / 10 > relicLastKills && RelicCount(RelicMechanic.SoloOverdrive) > 0) Heal((Kills / 10 - relicLastKills) * 3,NewCause(Champion.Id).Child("curio-overdrive-heal"));
            relicLastKills = Kills / 10;
            foreach (UnitState enemy in StableUnits)
                if (enemy.Alive && enemy.Faction == Faction.Enemy && enemy.RelicPoisonUntil > Elapsed && enemy.RelicPoisonAt <= Elapsed)
                { enemy.RelicPoisonAt = Elapsed + 1; ApplyDamage(enemy,Mathf.Max(1,catalog.RelicIncome) * RelicLateFee,Faction.Champion,"curio-poison",cause:NewCause(Champion.Id).Child("curio-poison")); }
            if (Elapsed >= relicSecondAt)
            {
                relicSecondAt = Elapsed + 1;
                if (RelicCount(RelicMechanic.BleedingAlarm) > 0) ApplyDamage(Champion,RelicCount(RelicMechanic.BleedingAlarm),Faction.Enemy,"Bleeding alarm",cause:NewCause(Champion.Id).Child("curio-cost"),injectedDamage:true);
            }
            if (Elapsed >= relicPulseAt)
            {
                relicPulseAt = Elapsed + 3;
                int gamble = RelicCount(RelicMechanic.GambleRecovery);
                if (gamble > 0 && combatRandom.NextDouble() < .5) Heal(20 * gamble,NewCause(Champion.Id).Child("curio-gamble"));
                if (gamble > 0 && combatRandom.NextDouble() < .5) ApplyDamage(Champion,8 * gamble,Faction.Enemy,"Gamble sandwich",cause:NewCause(Champion.Id).Child("curio-cost"),injectedDamage:true);
                foreach (UnitState enemy in StableUnits)
                    if (enemy.Alive && enemy.Faction == Faction.Enemy && enemy.EntranceUntil <= 0)
                    {
                        float damage = RelicCount(RelicMechanic.SorrowPulse) * Champion.Definition.Health * .2f + RelicCount(RelicMechanic.VoidPulse) * enemy.Health * .15f;
                        if (damage > 0) ApplyDamage(enemy,damage,Faction.Champion,"Curio pulse",cause:NewCause(Champion.Id).Child("curio-pulse"));
                    }
            }
            RelicSeals(); RelicBorrowedCrown(); StepNewRelics();
            foreach (UnitState phantom in phantomWeapons)
            {
                if (Elapsed < phantom.NextAttack) continue;
                WeaponDefinition weapon = phantom.Definition.Weapon;
                UnitState target = FindTarget(Champion,Mathf.Max(5,weapon.Range));
                if (target == null) continue;
                phantom.Position = Champion.Position; phantom.AttackDirection = (target.Position - Champion.Position).normalized;
                phantom.NextAttack = Elapsed + Mathf.Max(.2f,weapon.Cooldown);
                if (weapon.Kind == WeaponKind.Projectile) EmitProjectile(phantom,RelicAttackDamage(phantom,weapon.Damage),false,NewCause(Champion.Id).Child("curio-sixfold"));
                else
                    foreach (UnitState enemy in StableUnits)
                        if (CanHit(Faction.Champion,enemy) && InBody(enemy, Champion.Position, weapon.Range))
                            ApplyWeaponDamage(enemy,RelicAttackDamage(phantom,weapon.Damage),Faction.Champion,weapon.Element,"Phantom weapon",(enemy.Position - Champion.Position).normalized,weapon,false,NewCause(Champion.Id).Child("curio-sixfold"));
                events.Add(new CombatEvent(CombatEventKind.Fire,Champion.Position,Faction.Champion,direction:phantom.AttackDirection,weapon:weapon,cause:NewCause(Champion.Id).Child("curio-sixfold")));
            }
        }

        private void ProcessRelicEchoes()
        {
            int end = events.Count;
            for (int index = relicProcessedEvents; index < end; index++)
            {
                CombatEvent action = events[index];
                RelicItemEvent(action); RelicNewItemEvent(action);
                if (action.Kind != CombatEventKind.Fire || action.Faction != Faction.Champion || action.Cause.Depth != 0 || Elapsed < relicEchoAt || RelicCount(RelicMechanic.PrismEcho) == 0) continue;
                relicEchoAt = Elapsed + .15f;
                RelicStrike(Mathf.Max(2,catalog.RelicRanged * 2) * RelicCount(RelicMechanic.PrismEcho),1,"curio-prism");
            }
        }
    }
}
