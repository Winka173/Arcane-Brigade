using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class RelicLease
    {
        public RelicMechanic Mechanic;
        public int Wave;
        public int Count;
    }

    public sealed partial class RunSession
    {
        public int MechanicStacks(RelicMechanic mechanic)
        {
            int count = 0;
            foreach (ItemStack stack in State.Items)
            {
                if (SecretItemSuspended(stack.Id)) continue;
                ItemDefinition item = catalog.Item(stack.Id);
                if (item.Mechanic == mechanic) count += item.EffectiveStacks(stack.Count);
            }
            return count;
        }

        private float RelicBonus(ItemEffect effect, ModifierScope target, float value)
        {
            foreach (StatModifier gain in State.RelicGains ?? new List<StatModifier>())
                if (gain.Effect == effect && RunModifierRules.Applies(gain.Scope, target)) value += gain.Value;
            if (effect == ItemEffect.Damage && target != ModifierScope.Run) value += MechanicStacks(RelicMechanic.HoardPower) * (State.Gold / 5) * .01f;
            if (effect == ItemEffect.Health) value += MechanicStacks(RelicMechanic.LuckToHealth) * Mathf.Max(0, Bonus(ItemEffect.Luck)) * 100;
            if (effect == ItemEffect.CollisionDamage && target == ModifierScope.Champion)
                value += MechanicStacks(RelicMechanic.MeleeToCollision) * Mathf.Max(0, Bonus(ItemEffect.MeleeDamage, target)) * .5f + MechanicStacks(RelicMechanic.CollisionOnly) * Mathf.Max(0, Bonus(ItemEffect.Armor)) * 10;
            if (effect == ItemEffect.Armor) value += MechanicStacks(RelicMechanic.SlowToArmor) * Mathf.Max(0, -Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion));
            if (effect == ItemEffect.Income) value += MechanicStacks(RelicMechanic.DamageToIncome) * Mathf.Floor(Mathf.Max(0, Bonus(ItemEffect.Damage, ModifierScope.Champion)) / .04f);
            if (effect == ItemEffect.Discount && State.RelicDiscountWave == State.Wave) value += .3f;
            foreach (RelicLease lease in State.RelicLeases ?? new List<RelicLease>())
            {
                if (lease.Wave != State.Wave) continue;
                if (effect == ItemEffect.EnemyCount && lease.Mechanic == RelicMechanic.CrowdedTape) value += .1f * lease.Count;
                if (effect == ItemEffect.Armor && lease.Mechanic == RelicMechanic.ExposedRecovery) value -= .8f * lease.Count;
                if (effect == ItemEffect.TreasureDetection && lease.Mechanic == RelicMechanic.TreasureLease) value += lease.Count;
            }
            foreach (StatModifier floor in State.RelicFloors ?? new List<StatModifier>())
                if (floor.Effect == effect && floor.Scope == target) value = Mathf.Max(value, floor.Value);
            return value;
        }

        private void GainRelic(ItemEffect effect, float value, ModifierScope? scope = null)
        {
            if (value == 0) return;
            ModifierScope target = scope ?? RunModifierRules.DefaultScope(effect);
            StatModifier existing = State.RelicGains.Find(modifier => modifier.Effect == effect && modifier.Scope == target);
            if (existing == null) State.RelicGains.Add(new StatModifier { Effect = effect, Value = value, Scope = target });
            else existing.Value = Mathf.Clamp(existing.Value + value, -100000, 100000);
        }

        private void BeginRelicWave()
        {
            if (State.Phase != SessionPhase.Combat || State.RelicWave == State.Wave) return;
            State.RelicWave = State.Wave;
            State.RelicLeases.RemoveAll(lease => lease.Wave < State.Wave);
            int growth = MechanicStacks(RelicMechanic.WaveGrowth);
            if (growth > 0) { GainRelic(ItemEffect.MeleeDamage,4 * growth); GainRelic(ItemEffect.RangedDamage,4 * growth); GainRelic(ItemEffect.Damage,.1f * growth); }
            int interest = MechanicStacks(RelicMechanic.CompoundInterest);
            if (interest > 0) State.Gold = BoundedCounter((long)Math.Min(int.MaxValue, State.Gold * Math.Pow(1.2, interest)));
            int chaos = MechanicStacks(RelicMechanic.ChaosIncome);
            if (chaos > 0) State.Gold = BoundedCounter((long)State.Gold + new System.Random(unchecked(State.Seed ^ State.Wave * 7919)).Next(25,76) * chaos);
        }

        private void BuyRelic(ItemDefinition item, int purchasedToken)
        {
            if (item.Mechanic == RelicMechanic.SixWeapons && State.RelicWeaponIds.Count == 0)
                for (int index = 0; index < 6; index++) State.RelicWeaponIds.Add(catalog.Weapons[Next(ref State.WeaponRandom,catalog.Weapons.Length)].Id);
            if (item.Category == RelicCategory.Drink)
            {
                GainRelic(ItemEffect.Health, 4 * MechanicStacks(RelicMechanic.DrinkVitality));
                GainRelic(ItemEffect.AttackSpeed, .06f * MechanicStacks(RelicMechanic.DrinkTempo));
            }
            if (item.Category == RelicCategory.Food)
            {
                int growth = MechanicStacks(RelicMechanic.FoodGrowth);
                GainRelic(ItemEffect.Health, 10 * growth); GainRelic(ItemEffect.Heal, .5f * growth); GainRelic(ItemEffect.BodySize, .05f * growth);
            }
            if (item.Mechanic == RelicMechanic.CrowdedTape || item.Mechanic == RelicMechanic.ExposedRecovery || item.Mechanic == RelicMechanic.TreasureLease || item.Mechanic == RelicMechanic.SoloOverdrive)
                State.RelicLeases.Add(new RelicLease { Mechanic = item.Mechanic, Wave = State.Wave + 1, Count = 1 });
            if (item.Mechanic == RelicMechanic.EggGift) State.Eggs = BoundedCounter((long)State.Eggs + 3);
            if (item.Mechanic == RelicMechanic.TurnDiscount)
            {
                State.RelicDiscountWave = State.Wave;
                foreach (ShopOffer offer in State.Offers) if (offer.Token != purchasedToken && !offer.Sold && offer.Kind == OfferKind.Item && offer.Currency == ShopCurrency.Gold) offer.Price = Mathf.Max(1, Mathf.CeilToInt(offer.Price * .7f));
            }
            if (item.Mechanic == RelicMechanic.CompanionGift && catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw)
                for (int index = 0; index < 2 && State.NewRecruitsThisWave < 3 && State.Roster.Count < 20; index++)
                {
                    var available = new List<UnitDefinition>();
                    foreach (UnitDefinition ally in catalog.Companions)
                        if (!ally.SpecialRecruit && ally.UnlockWave <= State.Wave && (catalog.HeroRevision == 0 || K11HeroRules.Eligible(catalog.Unit(State.ChampionId),ally)) && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(ally.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == ally.Id && entry.WeaponId == weapon.Id))) available.Add(ally);
                    if (available.Count == 0) break;
                    UnitDefinition chosen = available[RecruitChoice(available)];
                    var weapons = new List<WeaponDefinition>();
                    foreach (WeaponDefinition weapon in catalog.Weapons) if (RecruitWeaponEligible(chosen.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == chosen.Id && entry.WeaponId == weapon.Id)) weapons.Add(weapon);
                    var recruit = new RosterEntry { UnitId = chosen.Id, WeaponId = BalancedWeapon(weapons, false).Id };
                    State.Roster.Add(recruit); RememberVariant(recruit); State.NewRecruitsThisWave++; MergeTriples(chosen.Id);
                }
            if (item.Mechanic == RelicMechanic.StatFloor)
                foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                    foreach (ModifierScope scope in Enum.GetValues(typeof(ModifierScope)))
                    {
                        float value = Bonus(effect, scope);
                        StatModifier floor = State.RelicFloors.Find(candidate => candidate.Effect == effect && candidate.Scope == scope);
                        if (floor == null) State.RelicFloors.Add(new StatModifier { Effect = effect, Scope = scope, Value = value }); else floor.Value = Mathf.Max(floor.Value,value);
                    }
        }

        private void ConfigureRelics()
        {
            CombatCatalog.RelicStacks = new int[Enum.GetValues(typeof(RelicMechanic)).Length];
            foreach (RelicMechanic mechanic in Enum.GetValues(typeof(RelicMechanic))) CombatCatalog.RelicStacks[(int)mechanic] = MechanicStacks(mechanic);
            CombatCatalog.RelicStacks[(int)RelicMechanic.SoloOverdrive] = 0;
            foreach (RelicLease lease in State.RelicLeases) if (lease.Wave == State.Wave && lease.Mechanic == RelicMechanic.SoloOverdrive) CombatCatalog.RelicStacks[(int)RelicMechanic.SoloOverdrive] += lease.Count;
            CombatCatalog.RelicIncome = Mathf.Max(0,catalog.Income + Bonus(ItemEffect.Income));
            CombatCatalog.RelicMelee = Bonus(ItemEffect.MeleeDamage, ModifierScope.Champion);
            CombatCatalog.RelicRanged = Bonus(ItemEffect.RangedDamage, ModifierScope.Champion);
            CombatCatalog.RelicWallet = State.Gold;
            if (MechanicStacks(RelicMechanic.SixWeapons) > 0)
            {
                var random = new System.Random(unchecked(State.Seed ^ 6151));
                CombatCatalog.RelicWeapons = new UnitDefinition[6];
                for (int index = 0; index < 6; index++)
                {
                    WeaponDefinition weapon = State.RelicWeaponIds.Count == 6 ? catalog.Weapon(State.RelicWeaponIds[index]) : catalog.Weapons[random.Next(catalog.Weapons.Length)];
                    UnitDefinition vessel = BuildUnit(catalog.Unit(State.ChampionId),weapon,1,false);
                    WeaponDefinition shared = BuildUnit(catalog.Unit(State.ChampionId),weapon,1,true).Weapon;
                    vessel.Weapon.Damage = Mathf.Max(vessel.Weapon.Damage,shared.Damage);
                    vessel.Weapon.ProjectileCount = Mathf.Max(vessel.Weapon.ProjectileCount,shared.ProjectileCount);
                    vessel.Weapon.ProjectilePierce = Mathf.Max(vessel.Weapon.ProjectilePierce,shared.ProjectilePierce);
                    vessel.Weapon.ProjectileSplit = Mathf.Max(vessel.Weapon.ProjectileSplit,shared.ProjectileSplit);
                    vessel.Weapon.ProjectileBounceChance = Mathf.Max(vessel.Weapon.ProjectileBounceChance,shared.ProjectileBounceChance);
                    vessel.Weapon.Range = Mathf.Max(vessel.Weapon.Range,shared.Range);
                    vessel.Weapon.ProjectileLifetime = Mathf.Max(vessel.Weapon.ProjectileLifetime,shared.ProjectileLifetime);
                    vessel.Weapon.Cooldown = Mathf.Min(vessel.Weapon.Cooldown,shared.Cooldown);
                    CombatCatalog.RelicWeapons[index] = vessel;
                }
            }
            CombatCatalog.RelicGain = modifier => GainRelic(modifier.Effect,modifier.Value,modifier.Scope);
            CombatCatalog.FreeFollowers = MechanicStacks(RelicMechanic.FreeFollowers) > 0;
            if (CombatCatalog.FreeFollowers) CombatCatalog.CompanionRoamOuterRadius = catalog.WorldSize.magnitude * .5f;
            CombatCatalog.StaticFollowers = MechanicStacks(RelicMechanic.StaticFollowers) > 0;
            if (MechanicStacks(RelicMechanic.CloseFollowers) > 0) { CombatCatalog.CompanionLeash *= .65f; CombatCatalog.CompanionRoamOuterRadius *= .65f; }
            if (CombatCatalog.StaticFollowers)
                foreach (UnitDefinition ally in CombatCatalog.StartingCompanions) { ally.Speed = 0; ally.Weapon.Range *= 2; ally.Weapon.ProjectileLifetime *= 2; ally.Weapon.ProjectilePierce += 3; }
            if (MechanicStacks(RelicMechanic.HalfHealthSeed) > 0 && State.Phase == SessionPhase.Combat) State.Health = Mathf.Min(State.Health,CombatCatalog.Champion.Health * .5f);
            if (MechanicStacks(RelicMechanic.ChestOnlyRecovery) > 0) { CombatCatalog.PassiveHeal = 0; CombatCatalog.HeartHealing = -100000; }
        }

        private void ValidateRelicState(RunSnapshot snapshot)
        {
            if (snapshot == null) return;
            if (snapshot.RelicWave < 0 || snapshot.RelicWave > snapshot.Wave || snapshot.RelicDiscountWave < 0 || snapshot.RelicDiscountWave > snapshot.Wave) throw new InvalidOperationException("Invalid relic wave journal");
            if (snapshot.RelicWeaponIds != null && (snapshot.RelicWeaponIds.Count != 0 && snapshot.RelicWeaponIds.Count != 6 || snapshot.RelicWeaponIds.Exists(id => catalog.Weapon(id) == null))) throw new InvalidOperationException("Invalid phantom weapon journal");
            foreach (List<StatModifier> modifiers in new[] { snapshot.RelicGains,snapshot.RelicFloors })
            {
                if (modifiers == null) continue;
                if (modifiers.Count > 256) throw new InvalidOperationException("Relic stat journal too large");
                var keys = new HashSet<string>();
                foreach (StatModifier modifier in modifiers)
                    if (modifier == null || !Enum.IsDefined(typeof(ItemEffect),modifier.Effect) || !Enum.IsDefined(typeof(ModifierScope),modifier.Scope) || !float.IsFinite(modifier.Value) || Mathf.Abs(modifier.Value) > 100000 || !keys.Add(modifier.Effect + "/" + modifier.Scope)) throw new InvalidOperationException("Invalid relic stat journal");
            }
            if (snapshot.RelicLeases == null) return;
            if (snapshot.RelicLeases.Count > 128) throw new InvalidOperationException("Relic lease journal too large");
            foreach (RelicLease lease in snapshot.RelicLeases)
                if (lease == null || !Enum.IsDefined(typeof(RelicMechanic),lease.Mechanic) || lease.Wave < 1 || (long)lease.Wave > (long)snapshot.Wave + 1 || lease.Count < 1 || lease.Count > 8) throw new InvalidOperationException("Invalid relic lease");
        }
    }
}
