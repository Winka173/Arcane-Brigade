using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum SessionPhase { Combat, Shop, Victory, Defeat }
    public enum OfferKind { Companion, Item }
    public enum ShopCurrency { Gold, Eggs, Health }

    [Serializable]
    public sealed class RosterEntry
    {
        public string UnitId;
        public string WeaponId;
        public int Tier = 1;
    }

    [Serializable]
    public sealed class ItemStack { public string Id; public int Count; }

    [Serializable]
    public sealed class ShopOffer
    {
        public int Token;
        public OfferKind Kind;
        public string Id;
        public int Price;
        public bool Sold;
        public ShopCurrency Currency;
    }

    [Serializable]
    public sealed class RunSnapshot
    {
        public const int CurrentSchemaRevision = 3;
        public int SchemaRevision = CurrentSchemaRevision;
        public string Version;
        public string RunId;
        public string ChampionId;
        public int Seed;
        public int Wave = 1;
        public int Gold;
        public int Eggs;
        public int TotalKills;
        public int BossKills;
        public float Health;
        public float CombatSeconds;
        public bool Endless;
        public int Difficulty;
        public string ChallengeId = "";
        public string ParentRunId = "";
        public int StartHall = 1;
        public string LastDamageKind = "";
        public List<string> DefeatedBosses = new List<string>();
        public bool LegacyContributionKills;
        public List<RosterEntry> AcquiredVariants = new List<RosterEntry>();
        public SessionPhase Phase;
        public uint ShopRandom;
        public uint WeaponRandom;
        public int NextOffer;
        public int RerollsThisShop;
        public int NewRecruitsThisWave;
        public int NextReceipt;
        public List<RunTransactionReceipt> Receipts = new List<RunTransactionReceipt>();
        public int LastIncome;
        public int LastInterest;
        public float BuildScoreMultiplier = 1f;
        public bool SpecialShopVisited;
        public string LastDamageSource;
        public float LastDamageAmount;
        public List<RosterEntry> Roster = new List<RosterEntry>();
        public List<ItemStack> Items = new List<ItemStack>();
        public List<ShopOffer> Offers = new List<ShopOffer>();
        public List<ShopOffer> SpecialOffers = new List<ShopOffer>();
        public List<ContributionEntry> Contributions = new List<ContributionEntry>();
    }

    public sealed class RunSession : IDisposable
    {
        private readonly RunCatalog catalog;
        private readonly List<UnityEngine.Object> runtimeAssets = new List<UnityEngine.Object>();
        public RunSnapshot State { get; private set; }
        public CheckpointCatalog CombatCatalog { get; private set; }
        public readonly Dictionary<string, int> BondCounts = new Dictionary<string, int>();
        public string Notice { get; private set; } = "";
        public WaveModifier ActiveModifier
        {
            get
            {
                foreach (WaveModifier modifier in catalog.WaveModifiers)
                    if (modifier.Every > 0 && State.Wave % modifier.Every == 0) return modifier;
                return null;
            }
        }

        public RunSession(RunCatalog definition) { RunModifierRules.Validate(definition); catalog = definition; }

        public void Start(string championId, int seed, int difficulty = 0, string challengeId = "")
        {
            if (difficulty < 0 || difficulty > 4 || !Array.Exists(RunMeta.ChallengeIds, id => id == challengeId)) throw new ArgumentException("Invalid run mode");
            int challengeIndex = Array.IndexOf(RunMeta.ChallengeIds, challengeId);
            if (challengeIndex > 0 && (difficulty != 0 || seed != RunMeta.ChallengeSeed(challengeIndex))) throw new ArgumentException("Challenge requires its fixed seed and difficulty");
            UnitDefinition champion = catalog.Unit(championId);
            if (champion == null || !Array.Exists(catalog.Champions, unit => unit.Id == championId)) throw new ArgumentException("Unknown champion");
            State = new RunSnapshot
            {
                Version = catalog.Version, RunId = Guid.NewGuid().ToString("N"), ChampionId = championId, Seed = seed,
                Gold = catalog.StartingGold, Health = champion.Health, Phase = SessionPhase.Combat, Difficulty = difficulty, ChallengeId = challengeId,
                ShopRandom = unchecked((uint)seed ^ 0xAC731u), WeaponRandom = unchecked((uint)seed ^ 0xE517u)
            };
            State.Roster.Add(catalog.HeroRevision > 0 ? K11HeroRules.Starter(catalog, champion) : new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = catalog.Weapons[1].Id });
            RememberVariant(State.Roster[0]);
            RecountBonds();
            State.Health = MaximumHealth();
        }

        public void Restore(RunSnapshot snapshot)
        {
            Validate(snapshot);
            State = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(snapshot));
            if (snapshot.SchemaRevision < 3)
            {
                State.StartHall = State.Endless ? catalog.NormalWaves + 1 : 1;
                State.ChallengeId = "";
                State.Difficulty = 0;
                if (State.Endless) State.Contributions = new List<ContributionEntry>();
                else State.LegacyContributionKills = State.Contributions != null && State.Contributions.Count > 0;
            }
            if (State.DefeatedBosses == null) State.DefeatedBosses = new List<string>();
            if (State.AcquiredVariants == null) State.AcquiredVariants = new List<RosterEntry>();
            if (snapshot.SchemaRevision < 3) foreach (RosterEntry entry in State.Roster) RememberVariant(entry);
            State.SchemaRevision = RunSnapshot.CurrentSchemaRevision;
            if (State.Receipts == null) State.Receipts = new List<RunTransactionReceipt>();
            if (State.SpecialOffers == null) State.SpecialOffers = new List<ShopOffer>();
            if (State.Contributions == null) State.Contributions = new List<ContributionEntry>();
            if (State.Offers.Count > catalog.ShopSlots) State.Offers.RemoveRange(catalog.ShopSlots, State.Offers.Count - catalog.ShopSlots);
            RecountBonds();
            State.Health = Mathf.Min(State.Health, MaximumHealth());
        }

        public void Validate(RunSnapshot snapshot)
        {
            if (snapshot != null && snapshot.SchemaRevision >= 3 && (snapshot.Difficulty < 0 || snapshot.Difficulty > 4 || !Array.Exists(RunMeta.ChallengeIds, id => id == snapshot.ChallengeId) || snapshot.StartHall < 1 || snapshot.StartHall > snapshot.Wave + (long)1 || snapshot.DefeatedBosses == null || !float.IsFinite(snapshot.LastDamageAmount) || snapshot.LastDamageAmount < 0)) throw new InvalidOperationException("Invalid run meta identity");
            if (snapshot != null && snapshot.SchemaRevision >= 3)
            {
                int challengeIndex = Array.IndexOf(RunMeta.ChallengeIds, snapshot.ChallengeId);
                if (challengeIndex > 0 && (snapshot.Difficulty != 0 || snapshot.Seed != RunMeta.ChallengeSeed(challengeIndex))) throw new InvalidOperationException("Invalid challenge seed");
                var bosses = new HashSet<string>();
                foreach (string id in snapshot.DefeatedBosses) if (!Array.Exists(catalog.Bosses, boss => boss.Id == id) || !bosses.Add(id)) throw new InvalidOperationException("Invalid boss discovery");
                if (snapshot.AcquiredVariants == null) throw new InvalidOperationException("Missing variant discovery journal");
                var variantKeys = new HashSet<string>();
                foreach (RosterEntry entry in snapshot.AcquiredVariants)
                    if (entry == null || !Array.Exists(catalog.Companions, unit => unit.Id == entry.UnitId) || catalog.Weapon(entry.WeaponId) == null || entry.Tier < 1 || entry.Tier > catalog.MaximumTier || !variantKeys.Add(entry.UnitId + "/" + entry.WeaponId + "/" + entry.Tier)) throw new InvalidOperationException("Invalid variant discovery");
            }
            if (snapshot != null && (snapshot.SchemaRevision < 0 || snapshot.SchemaRevision > RunSnapshot.CurrentSchemaRevision)) throw new InvalidOperationException("Unsupported run schema");
            if (snapshot == null || snapshot.Version != catalog.Version || string.IsNullOrEmpty(snapshot.RunId) || catalog.Unit(snapshot.ChampionId) == null || snapshot.Wave < 1 || snapshot.Gold < 0 || snapshot.Eggs < 0 || !float.IsFinite(snapshot.Health) || snapshot.Health < 0 || !float.IsFinite(snapshot.CombatSeconds) || snapshot.CombatSeconds < 0 || !Enum.IsDefined(typeof(SessionPhase), snapshot.Phase)) throw new InvalidOperationException("Invalid or incompatible run save");
            if (!Array.Exists(catalog.Champions, unit => unit.Id == snapshot.ChampionId)) throw new InvalidOperationException("Invalid champion identity");
            if (snapshot.Roster == null || snapshot.Items == null || snapshot.Offers == null || snapshot.TotalKills < 0 || snapshot.BossKills < 0 || snapshot.NextOffer < 0) throw new InvalidOperationException("Incomplete or invalid save counters");
            if (snapshot.NewRecruitsThisWave < 0 || snapshot.NewRecruitsThisWave > 3) throw new InvalidOperationException("Invalid wave recruitment quota");
            var rosterKeys = new HashSet<string>();
            if (snapshot.Contributions != null)
            {
                var contributionKeys = new HashSet<string>();
                foreach (ContributionEntry entry in snapshot.Contributions)
                    if (entry == null || string.IsNullOrEmpty(entry.UnitId) || entry.WeaponId == null || string.IsNullOrEmpty(entry.EffectId) || entry.Tier < 0 || entry.Tier > catalog.MaximumTier || !float.IsFinite(entry.Damage) || entry.Damage < 0 || !float.IsFinite(entry.Healing) || entry.Healing < 0 || entry.Coins < 0 || entry.Activations < 0 || entry.Kills < 0 || !contributionKeys.Add(entry.Key)) throw new InvalidOperationException("Invalid contribution ledger");
            }
            if (snapshot.NextReceipt < 0 || snapshot.RerollsThisShop < 0) throw new InvalidOperationException("Invalid transaction counters");
            if (snapshot.SchemaRevision >= 2 && snapshot.Receipts == null) throw new InvalidOperationException("Missing transaction history");
            if (snapshot.Receipts != null)
            {
                if (snapshot.Receipts.Count > RunTransactionReceipt.HistoryLimit) throw new InvalidOperationException("Invalid transaction history length");
                int previousReceipt = 0;
                foreach (RunTransactionReceipt receipt in snapshot.Receipts)
                {
                    if (receipt == null || receipt.Id <= previousReceipt || receipt.Id > snapshot.NextReceipt || receipt.Hall < 1 || string.IsNullOrEmpty(receipt.Title) || receipt.Changes == null) throw new InvalidOperationException("Invalid transaction receipt");
                    foreach (string change in receipt.Changes) if (string.IsNullOrEmpty(change)) throw new InvalidOperationException("Invalid transaction delta");
                    previousReceipt = receipt.Id;
                }
            }
            foreach (RosterEntry entry in snapshot.Roster)
            {
                if (entry == null || !Array.Exists(catalog.Companions, unit => unit.Id == entry.UnitId) || catalog.Weapon(entry.WeaponId) == null || entry.Tier < 1 || entry.Tier > catalog.MaximumTier || !rosterKeys.Add(entry.UnitId + "/" + entry.WeaponId)) throw new InvalidOperationException("Invalid roster entry");
            }
            var items = new HashSet<string>();
            foreach (ItemStack stack in snapshot.Items) if (stack == null || catalog.Item(stack.Id) == null || stack.Count < 1 || stack.Count > 100000 || !items.Add(stack.Id)) throw new InvalidOperationException("Invalid item stack");
            var tokens = new HashSet<int>();
            var allOffers = new List<ShopOffer>(snapshot.Offers);
            if (snapshot.SpecialOffers != null) allOffers.AddRange(snapshot.SpecialOffers);
            foreach (ShopOffer offer in allOffers)
                if (offer == null || !Enum.IsDefined(typeof(OfferKind), offer.Kind) || !Enum.IsDefined(typeof(ShopCurrency), offer.Currency) || offer.Price < 0 || offer.Token < 1 || offer.Token > snapshot.NextOffer || !tokens.Add(offer.Token) || (offer.Kind == OfferKind.Item ? catalog.Item(offer.Id) == null : !Array.Exists(catalog.Companions, unit => unit.Id == offer.Id))) throw new InvalidOperationException("Invalid shop offer");
        }

        public int StackCount(string id) { foreach (ItemStack stack in State.Items) if (stack.Id == id) return stack.Count; return 0; }

        public float Bonus(ItemEffect effect)
        {
            return Bonus(effect, RunModifierRules.DefaultScope(effect));
        }

        public float Bonus(ItemEffect effect, ModifierScope target)
        {
            float value = 0f;
            foreach (StatModifier modifier in catalog.Unit(State.ChampionId).ChampionModifiers ?? new StatModifier[0])
                if (modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value;
            foreach (RosterEntry entry in State.Roster)
                foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers ?? new StatModifier[0])
                    if (modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value * UnitDefinition.ModifierScale(entry.Tier);
            foreach (ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                foreach (StatModifier modifier in item.EffectiveModifiers)
                    if (modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value * item.EffectiveStacks(stack.Count);
            }
            foreach (BondDefinition bond in catalog.Bonds)
                if (bond.Effect == effect && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && BondCounts.TryGetValue(bond.Id, out int count)) value += bond.Reward(count);
            return value;
        }

        private void RecountBonds()
        {
            BondCounts.Clear();
            foreach (RosterEntry entry in State.Roster)
                foreach (string tag in catalog.Unit(entry.UnitId).Tags)
                    BondCounts[tag] = BondCounts.TryGetValue(tag, out int count) ? count + 1 : 1;
        }

        public CheckpointCatalog PrepareCombat()
        {
            DisposeAssets();
            RecountBonds();
            CombatCatalog = Clone(catalog.Arena);
            CombatCatalog.EnableHeroMechanics = catalog.HeroRevision > 0;
            CombatCatalog.ArenaSize = catalog.WorldSize;
            CombatCatalog.StartingCompanions = new UnitDefinition[State.Roster.Count];
            CombatCatalog.ExplicitStartingRoster = true;
            CombatCatalog.Champion = BuildUnit(catalog.Unit(State.ChampionId), catalog.Unit(State.ChampionId).Weapon, 1, false);
            for (int index = 0; index < State.Roster.Count; index++)
            {
                RosterEntry entry = State.Roster[index];
                CombatCatalog.StartingCompanions[index] = BuildUnit(catalog.Unit(entry.UnitId), catalog.Weapon(entry.WeaponId), entry.Tier, true);
            }
            float scaling = (1f + (State.Wave - 1) * 0.10f) * (1f + State.Difficulty * 0.18f) * (1f + State.Difficulty * State.Difficulty * 0.07f);
            CombatCatalog.MeleeEnemy = BuildEnemy(catalog.Enemies[(State.Wave - 1) % catalog.Enemies.Length], scaling);
            CombatCatalog.RangedEnemy = BuildEnemy(catalog.Enemies[State.Wave % catalog.Enemies.Length], scaling);
            if (State.Wave == 1)
            {
                UnitDefinition firstContact = Array.Find(catalog.Enemies, enemy => enemy.Weapon.Kind == WeaponKind.Contact && enemy.UnlockWave <= 1);
                if (firstContact == null) throw new InvalidOperationException("Wave1 requires an unlocked contact enemy");
                CombatCatalog.MeleeEnemy = BuildEnemy(firstContact, scaling);
                CombatCatalog.RangedEnemy = CombatCatalog.MeleeEnemy;
            }
            CombatCatalog.Boss = State.Wave % catalog.BossEvery == 0 ? BuildEnemy(catalog.Bosses[(State.Wave / catalog.BossEvery - 1) % catalog.Bosses.Length], scaling) : null;
            CombatCatalog.WaveDuration = Mathf.Min(catalog.WaveSeconds, catalog.FirstWaveSeconds + (State.Wave - 1) * catalog.WaveIncrement);
            CombatCatalog.EnemyBudget = Mathf.CeilToInt((30 + Mathf.Min(State.Wave, 30) * 3) * (State.Wave >= 5 ? 4 : State.Wave == 4 ? 3 : 1) * (1f + State.Difficulty * 0.12f));
            CombatCatalog.SpawnInterval = State.Wave >= 4 ? Mathf.Max(0.22f, 0.46f - (State.Wave - 4) * 0.012f) : 1.4f - State.Wave * 0.035f;
            CombatCatalog.CompanionLeash = catalog.Arena.CompanionLeash + Bonus(ItemEffect.Roam);
            CombatCatalog.CompanionRoamOuterRadius = CombatCatalog.CompanionLeash * 0.84f;
            CombatCatalog.PickupRadius = catalog.Arena.PickupRadius + Bonus(ItemEffect.Pickup);
            CombatCatalog.CompanionInterceptor = Bonus(ItemEffect.Interceptor) > 0f;
            CombatCatalog.PassiveHeal = Bonus(ItemEffect.Heal);
            CombatCatalog.Armor = Bonus(ItemEffect.Armor);
            if (catalog.StatRevision > 0)
            {
                CombatCatalog.PassiveHeal *= K10StatRules.Factor(Bonus(ItemEffect.HealCorrection));
                CombatCatalog.Armor *= K10StatRules.Factor(Bonus(ItemEffect.ArmorCorrection));
                CombatCatalog.Dodge = Mathf.Clamp(Bonus(ItemEffect.Dodge), 0, 0.6f);
                CombatCatalog.HeartHealing = Bonus(ItemEffect.HeartHealing);
                CombatCatalog.ChestQuality = Bonus(ItemEffect.ChestQuality) + Mathf.Max(0, Bonus(ItemEffect.TreasureDetection)) * 0.25f;
                CombatCatalog.EnemyBudget = Mathf.CeilToInt(CombatCatalog.EnemyBudget * K10StatRules.Factor(Bonus(ItemEffect.EnemyCount), 0.2f, 4));
                CombatCatalog.SpawnInterval /= K10StatRules.Factor(Bonus(ItemEffect.EnemyCount), 0.2f, 4);
            }
            CombatCatalog.Seed = unchecked(State.Seed + State.Wave * 7919);
            CombatCatalog.GoldPerKill = 2;
            CombatCatalog.ExtendedLoot = catalog.LootRevision > 0;
            CombatCatalog.Luck = Bonus(ItemEffect.Luck);
            CombatCatalog.TreasureDetection = Bonus(ItemEffect.TreasureDetection);
            CombatCatalog.ChestOpenRadius = catalog.Arena.ChestOpenRadius * K10StatRules.Factor(Bonus(ItemEffect.ChestReach), 0.5f, 4);
            var procs = new List<ActiveProc>();
            foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
            {
                int stacks = catalog.Item(rule.ItemId).EffectiveStacks(StackCount(rule.ItemId));
                if (stacks > 0) procs.Add(new ActiveProc { Rule = rule, Stacks = stacks });
            }
            CombatCatalog.Procs = procs.ToArray();
            CombatCatalog.EnableArenaToys = catalog.SignatureRevision > 0;
            CombatCatalog.RunWave = State.Wave;
            CombatCatalog.EnableBossArenaPhase = catalog.SignatureRevision > 0;
            CombatCatalog.ExpandedContent = catalog.ExpansionRevision > 0;
            if (CombatCatalog.ExpandedContent)
            {
                var pool = new List<UnitDefinition>();
                foreach (UnitDefinition enemy in catalog.Enemies)
                    if (enemy.UnlockWave <= State.Wave && (State.Wave > 1 || enemy.Weapon.Kind == WeaponKind.Contact))
                    {
                        UnitDefinition candidate = BuildEnemy(enemy, scaling); pool.Add(candidate);
                        bool emphasis = State.Wave < 7 ? enemy.Behavior == EnemyBehavior.Standard && enemy.Weapon.Kind == WeaponKind.Contact : State.Wave < 14 ? enemy.Behavior == EnemyBehavior.Door || enemy.Behavior == EnemyBehavior.Magnet : enemy.Behavior != EnemyBehavior.Standard;
                        if (emphasis) pool.Add(candidate);
                    }
                CombatCatalog.EnemyPool = pool.ToArray();
            }
            foreach (ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id); int count = item.EffectiveStacks(stack.Count);
                if (item.SignatureRole == SignatureRelicKind.HarvestReceipt) CombatCatalog.HarvestStacks += count;
                if (item.SignatureRole == SignatureRelicKind.ReflectiveInk) CombatCatalog.ReflectiveInkStacks += count;
                if (item.SignatureRole == SignatureRelicKind.CarbonRibbon && count > 0) CombatCatalog.CarbonRibbon = true;
                if (item.SignatureRole == SignatureRelicKind.OfficeLens) CombatCatalog.OfficeLensStacks += count;
                if (item.SignatureRole == SignatureRelicKind.TeaCoaster) CombatCatalog.TeaCoasterStacks += count;
                if (item.SignatureRole == SignatureRelicKind.ScratchRecord) CombatCatalog.ChorusStacks += count;
                if (item.SignatureRole == SignatureRelicKind.TreasureRibbon) CombatCatalog.TreasureStacks += count;
                if (item.SignatureRole == SignatureRelicKind.PatchSpring && count > 0) CombatCatalog.PatchSpring = true;
                if (item.SignatureRole == SignatureRelicKind.ThermalInk && count > 0) CombatCatalog.ThermalInk = true;
                if (item.SignatureRole == SignatureRelicKind.RepairCoupon) CombatCatalog.RepairStacks += count;
            }
            WaveModifier active = ActiveModifier;
            if (active != null)
            {
                CombatCatalog.SpawnInterval *= active.SpawnMultiplier;
                CombatCatalog.EnemyBudget = Mathf.CeilToInt(CombatCatalog.EnemyBudget * active.BudgetMultiplier);
                CombatCatalog.GoldPerKill = active.GoldPerKill;
                CombatCatalog.PassiveHeal += active.Healing;
            }
            return CombatCatalog;
        }

        private UnitDefinition BuildUnit(UnitDefinition source, WeaponDefinition weapon, int tier, bool companion)
        {
            UnitDefinition unit = Clone(source);
            ModifierScope target = companion ? ModifierScope.Companions : ModifierScope.Champion;
            unit.Weapon = Clone(weapon);
            if (companion && catalog.ContentRevision > 0 && tier > 1)
                CompanionTierScaling.Apply(unit.Weapon, tier);
            unit.CombatTier = tier;
            unit.PassiveStrength = (source.Passive == PassiveKind.Scavenge ? Mathf.Round(source.PassiveStrength) : source.PassiveStrength) * Mathf.Pow(catalog.TierSupportMultiplier, tier - 1);
            unit.Weapon.Damage *= DamageMultiplier(source, tier, companion);
            unit.Weapon.Cooldown /= Mathf.Max(0.1f, 1f + Bonus(ItemEffect.AttackSpeed, target) + (companion ? Bonus(ItemEffect.CompanionAttackSpeed) : 0f));
            unit.Weapon.Windup = Mathf.Min(unit.Weapon.Windup, unit.Weapon.Cooldown * 0.35f);
            unit.Weapon.Recovery = Mathf.Min(unit.Weapon.Recovery, unit.Weapon.Cooldown * 0.4f);
            unit.Weapon.ProjectileCount += Mathf.FloorToInt(Bonus(ItemEffect.Projectiles, target));
            unit.CriticalChance = Mathf.Clamp01(unit.CriticalChance + Bonus(ItemEffect.Crit, target));
            unit.Speed *= Mathf.Max(0.1f, 1f + Bonus(ItemEffect.MoveSpeed, target));
            if (!companion) unit.Health = Mathf.Max(1f, unit.Health + Bonus(ItemEffect.Health));
            if (companion) unit.BodyHeight *= CompanionTierScaling.Body(tier);
            K10StatRules.ApplyUnit(this, catalog, unit, source, weapon, tier, companion);
            if (companion && source.SpecialRecruit) unit.Weapon.Damage *= 1.35f;
            if (unit.Weapon.Kind == WeaponKind.Melee)
            {
                MeleeBalance.Apply(unit.Weapon);
                if (companion) unit.Speed *= MeleeBalance.Speed;
            }
            return unit;
        }

        public float DamageMultiplier(UnitDefinition unit, int tier, bool companion)
        {
            float value = Mathf.Pow(catalog.TierDamageMultiplier, tier - 1) * Mathf.Max(0.1f, 1f + Bonus(ItemEffect.Damage, companion ? ModifierScope.Companions : ModifierScope.Champion) + Bonus(companion ? ItemEffect.CompanionDamage : ItemEffect.ChampionDamage));
            if (unit.Passive == PassiveKind.Rally) value *= 1f + unit.PassiveStrength * Mathf.Pow(catalog.TierSupportMultiplier, tier - 1) * 0.01f * State.Roster.Count;
            return value;
        }

        private UnitDefinition BuildEnemy(UnitDefinition source, float scaling)
        {
            UnitDefinition unit = Clone(source);
            unit.Weapon = Clone(source.Weapon);
            unit.KnockbackResistance = Mathf.Max(source.KnockbackResistance, source.IsBoss ? 0.85f : source.BodyHeight >= 1.2f ? 0.6f : 0.15f);
            unit.Weapon.ProjectileSpeed = Mathf.Min(2.4f, unit.Weapon.ProjectileSpeed);
            unit.Health *= scaling * (source.IsBoss ? catalog.BossHealthMultiplier : 1);
            if (source.IsBoss) unit.Health *= 4f;
            unit.Health *= 1 + Mathf.Max(0, State.Wave - 3) * 0.06f;
            unit.Speed *= 1 + 1.5f * (1 - Mathf.Exp(-(State.Wave - 1) * 0.015f));
            float damageGrowth = Mathf.Max(0, State.Wave - 10);
            unit.ContactDamage = Mathf.Min(14f + damageGrowth * 0.12f, source.ContactDamage + State.Wave * 0.35f);
            unit.Weapon.Damage = Mathf.Min((source.IsBoss ? 26f : 14f) + damageGrowth * (source.IsBoss ? 0.2f : 0.12f), source.Weapon.Damage + State.Wave * 0.35f);
            float pressure = 0.62f + State.Difficulty * 0.095f;
            unit.ContactDamage *= pressure; unit.Weapon.Damage *= pressure;
            if (catalog.StatRevision > 0)
            {
                float late = Mathf.Max(0, State.Wave - 10);
                unit.Health *= (1 + late * 0.055f) * K10StatRules.Factor(Bonus(ItemEffect.EnemyHealth), 0.2f, 10);
                unit.Speed *= K10StatRules.Factor(Bonus(ItemEffect.EnemySpeed), 0.25f, 2);
            }
            return unit;
        }

        private T Clone<T>(T source) where T : UnityEngine.Object { T copy = UnityEngine.Object.Instantiate(source); runtimeAssets.Add(copy); return copy; }

        public void Settle(CombatSimulation simulation)
        {
            if (State.Phase != SessionPhase.Combat || (simulation.Phase != RunPhase.Victory && simulation.Phase != RunPhase.Defeat)) return;
            State.Health = simulation.Champion.Health;
            State.BuildScoreMultiplier = catalog.StatRevision > 0 ? K10StatRules.Factor(Bonus(ItemEffect.ScoreMultiplier), 0.1f, 100) : 1;
            State.LastDamageSource = simulation.LastDamageSource;
            State.LastDamageAmount = simulation.LastDamageAmount;
            State.LastDamageKind = simulation.LastDamageKind;
            State.Gold = BoundedCounter((long)State.Gold + simulation.Gold);
            State.TotalKills = BoundedCounter((long)State.TotalKills + simulation.Kills);
            State.CombatSeconds += simulation.Elapsed;
            ContributionLedger.Merge(State.Contributions, simulation.Contributions);
            State.Eggs = BoundedCounter((long)State.Eggs + simulation.Eggs + (simulation.BossDefeated ? 3 : 1));
            if (simulation.BossDefeated)
            {
                State.BossKills = BoundedCounter((long)State.BossKills + 1);
                RunMeta.Add(State.DefeatedBosses, simulation.Boss.Definition.Id);
            }
            if (simulation.Phase == RunPhase.Defeat) { State.Phase = SessionPhase.Defeat; return; }
            State.Health = MaximumHealth();
            if (!State.Endless && State.Wave >= catalog.NormalWaves) { State.Phase = SessionPhase.Victory; return; }
            State.LastInterest = Mathf.Min(catalog.InterestCap + Mathf.FloorToInt(Bonus(ItemEffect.Interest)), State.Gold / Mathf.Max(1, catalog.InterestDivisor));
            State.LastIncome = BoundedCounter((long)catalog.Income + (long)State.Wave * 2 + Mathf.FloorToInt(Bonus(ItemEffect.Income)));
            State.Gold = BoundedCounter((long)State.Gold + State.LastInterest + State.LastIncome);
            State.Phase = SessionPhase.Shop;
            State.SpecialShopVisited = false;
            State.NewRecruitsThisWave = 0;
            RollShop(true);
        }

        public void ContinueWave()
        {
            if (State.Phase != SessionPhase.Shop) return;
            if (State.Wave == int.MaxValue) { Notice = "Technical hall counter limit reached. Records remain saved."; return; }
            State.Wave++;
            State.Phase = SessionPhase.Combat;
            State.Offers.Clear();
            State.SpecialOffers.Clear();
            Notice = "";
        }

        public void EnterEndless()
        {
            if (State.Phase != SessionPhase.Victory) return;
            State.Endless = true;
            State.NewRecruitsThisWave = 0;
            State.ParentRunId = State.RunId;
            State.RunId = Guid.NewGuid().ToString("N");
            State.StartHall = State.Wave + 1;
            State.Contributions.Clear();
            State.LegacyContributionKills = false;
            State.DefeatedBosses.Clear();
            State.LastDamageSource = State.LastDamageKind = "";
            State.LastDamageAmount = 0;
            State.LastIncome = BoundedCounter((long)catalog.Income + Mathf.FloorToInt(Bonus(ItemEffect.Income)));
            State.LastInterest = 0;
            State.TotalKills = State.BossKills = 0;
            State.CombatSeconds = 0f;
            State.Phase = SessionPhase.Shop;
            State.Gold = BoundedCounter((long)State.Gold + State.LastIncome);
            RollShop(true);
        }

        public bool Buy(int token)
        {
            if (State.Phase != SessionPhase.Shop) return false;
            ShopOffer offer = State.Offers.Find(candidate => candidate.Token == token) ?? State.SpecialOffers.Find(candidate => candidate.Token == token);
            if (!CanBuy(offer)) { Notice = "Offer expired, capped or insufficient currency. HP purchases must leave at least 1 HP."; return false; }
            float previousMaximum = MaximumHealth();
            if (offer.Kind == OfferKind.Companion)
            {
                var choices = new List<WeaponDefinition>();
                foreach (WeaponDefinition weapon in catalog.Weapons)
                    if (RecruitWeaponEligible(offer.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == offer.Id && entry.WeaponId == weapon.Id)) choices.Add(weapon);
                if (choices.Count == 0) { Notice = "No unique weapon variant available."; return false; }
                WeaponDefinition chosen = BalancedWeapon(choices, catalog.Unit(offer.Id).SpecialRecruit);
                bool newSpecies = !State.Roster.Exists(entry => entry.UnitId == offer.Id);
                State.Roster.Add(new RosterEntry { UnitId = offer.Id, WeaponId = chosen.Id });
                if (newSpecies) State.NewRecruitsThisWave++;
                RememberVariant(State.Roster[State.Roster.Count - 1]);
                MergeTriples(offer.Id);
            }
            else
            {
                ItemStack stack = State.Items.Find(candidate => candidate.Id == offer.Id);
                ItemDefinition item = catalog.Item(offer.Id);
                if (stack != null && stack.Count >= item.StackLimit) { Notice = "Item stack limit reached."; return false; }
                if (stack == null) State.Items.Add(new ItemStack { Id = offer.Id, Count = 1 }); else stack.Count++;
            }
            if (offer.Currency == ShopCurrency.Eggs) State.Eggs -= offer.Price;
            else if (offer.Currency == ShopCurrency.Health) State.Health -= offer.Price;
            else State.Gold -= offer.Price;
            offer.Sold = true;
            RecountBonds();
            ReconcileHealth(previousMaximum);
            State.BuildScoreMultiplier = catalog.StatRevision > 0 ? K10StatRules.Factor(Bonus(ItemEffect.ScoreMultiplier), 0.1f, 100) : 1;
            Notice = "Purchased. " + (offer.Kind == OfferKind.Companion ? "Weapon rolled after purchase." : "Upgrade stacks for this run.");
            return true;
        }

        public bool CanBuy(ShopOffer offer)
        {
            if (offer != null && offer.Kind == OfferKind.Companion && State.NewRecruitsThisWave >= 3 && !State.Roster.Exists(entry => entry.UnitId == offer.Id)) return false;
            if (offer == null || offer.Sold || State.Phase != SessionPhase.Shop) return false;
            if (offer.Currency == ShopCurrency.Health ? State.Health - offer.Price < 1f : (offer.Currency == ShopCurrency.Eggs ? State.Eggs : State.Gold) < offer.Price) return false;
            if (offer.Kind == OfferKind.Item) return StackCount(offer.Id) < catalog.Item(offer.Id).StackLimit;
            return Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(offer.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == offer.Id && entry.WeaponId == weapon.Id));
        }

        private void MergeTriples(string id)
        {
            for (int tier = 1; tier < catalog.MaximumTier; tier++)
            {
                List<RosterEntry> materials = State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == tier);
                while (materials.Count >= 3)
                {
                    List<RosterEntry> consumed = materials.GetRange(0, 3);
                    var choices = new List<WeaponDefinition>();
                    foreach (WeaponDefinition weapon in catalog.Weapons)
                        if (RecruitWeaponEligible(id, weapon) && !State.Roster.Exists(entry => entry.UnitId == id && entry.WeaponId == weapon.Id && !consumed.Contains(entry))) choices.Add(weapon);
                    if (choices.Count == 0) break;
                    for (int index = 0; index < 3; index++) State.Roster.Remove(materials[index]);
                    materials.RemoveRange(0, 3);
                    State.Roster.Add(new RosterEntry { UnitId = id, Tier = tier + 1, WeaponId = BalancedWeapon(choices).Id });
                    RememberVariant(State.Roster[State.Roster.Count - 1]);
                }
            }
        }

        public bool Reroll()
        {
            int price = RerollCost;
            if (State.Phase != SessionPhase.Shop || State.Gold < price) return false;
            if (State.RerollsThisShop == int.MaxValue) return false;
            State.Gold -= price;
            RollShop();
            State.RerollsThisShop++;
            Notice = "Shop refreshed.";
            return true;
        }

        public int RerollCost => RunSession.BoundedCounter((long)catalog.RerollPrice + (long)State.RerollsThisShop * catalog.RerollPriceStep);
        private void RememberVariant(RosterEntry entry)
        {
            if (!State.AcquiredVariants.Exists(candidate => candidate.UnitId == entry.UnitId && candidate.WeaponId == entry.WeaponId && candidate.Tier == entry.Tier))
                State.AcquiredVariants.Add(new RosterEntry { UnitId = entry.UnitId, WeaponId = entry.WeaponId, Tier = entry.Tier });
        }
        public bool RerollAltar()
        {
            if (State.Phase != SessionPhase.Shop || State.SpecialOffers.Count == 0 || State.Gold < RerollCost || State.RerollsThisShop == int.MaxValue) return false;
            State.Gold -= RerollCost;
            RollSpecialOffers();
            State.RerollsThisShop++;
            Notice = "Altar refreshed with gold. Eggs and HP unchanged.";
            return true;
        }
        public float AltarActivationChance => (float)Math.Min(1d, catalog.AltarBaseChance + (double)State.Eggs * catalog.AltarChancePerEgg);

        private bool Eligible(ItemDefinition item, ShopPool pool) => item.UnlockWave <= State.Wave && (catalog.EconomyRevision == 0 || item.ShopWeight > 0 && (item.Pools & pool) != 0) && StackCount(item.Id) < item.StackLimit;

        private int WeightedItem(List<ItemDefinition> items)
        {
            long total = 0;
            foreach (ItemDefinition item in items) total += ItemWeight(item);
            if (total <= 0 || total > int.MaxValue) throw new InvalidOperationException("Invalid weighted shop pool");
            int ticket = Next(ref State.ShopRandom, (int)total);
            for (int index = 0; index < items.Count; index++)
            {
                ticket -= ItemWeight(items[index]);
                if (ticket < 0) return index;
            }
            throw new InvalidOperationException("Weighted selection failed");
        }

        private int ItemWeight(ItemDefinition item) => catalog.EconomyRevision == 0 ? 1 : Mathf.Max(1, Mathf.RoundToInt(item.ShopWeight * K10StatRules.Factor((Bonus(ItemEffect.ShopRarity) + (catalog.StatRevision > 0 ? Bonus(ItemEffect.Luck) * 0.25f : 0)) * (int)item.Rarity, 0.1f, 4)));

        public float NewRecruitChance
        {
            get
            {
                int count = State.Roster.Count;
                float chance = count >= 20 ? 0.03f : count >= 16 ? 0.1f : count >= 14 ? 0.18f : count >= 12 ? 0.3f : count >= 10 ? 0.45f : 0.75f;
                return Mathf.Clamp(chance * Mathf.Max(0.25f, 1 + Bonus(ItemEffect.Luck)), 0.01f, 0.95f);
            }
        }

        public float StagePriceFactor => 1 + 0.07f * Mathf.Max(0, State.Wave - 1);
        public float SpecialStagePriceFactor => 1 + 0.035f * Mathf.Max(0, State.Wave - 1);

        private void RollShop(bool rollSpecial = false)
        {
            State.Offers.Clear();
            if (State.NextOffer > int.MaxValue - catalog.ShopSlots - 3) throw new InvalidOperationException("Shop token counter exhausted");
            var units = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions) if (!unit.SpecialRecruit && unit.UnlockWave <= State.Wave && (catalog.HeroRevision == 0 || K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit)) && (State.NewRecruitsThisWave < 3 || State.Roster.Exists(entry => entry.UnitId == unit.Id)) && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == unit.Id && entry.WeaponId == weapon.Id))) units.Add(unit);
            var items = new List<ItemDefinition>();
            foreach (ItemDefinition item in catalog.Items) if (Eligible(item, ShopPool.Gold)) items.Add(item);
            if (items.Count == 0 && units.Count == 0) throw new InvalidOperationException("No eligible shop offers");
            for (int index = 0; index < catalog.ShopSlots; index++)
            {
                bool newRecruit = Next(ref State.ShopRandom, 1000000) < NewRecruitChance * 1000000;
                var available = units.FindAll(unit => State.Roster.Exists(entry => entry.UnitId == unit.Id) || newRecruit);
                var mergeReady = available.FindAll(unit => State.Roster.FindAll(entry => entry.UnitId == unit.Id && entry.Tier == 1).Count >= 2);
                if (mergeReady.Count > 0 && Next(ref State.ShopRandom, 100) < 85) available = mergeReady;
                bool recruit = available.Count > 0 && (items.Count == 0 || index == 0);
                int choice = recruit ? Next(ref State.ShopRandom, available.Count) : WeightedItem(items);
                int basePrice = Mathf.CeilToInt((recruit ? available[choice].Price : items[choice].Price * (1 + (int)items[choice].Rarity * 0.4f)) * StagePriceFactor);
                float discount = Bonus(ItemEffect.Discount) + Bonus(recruit ? ItemEffect.RecruitDiscount : ItemEffect.RelicDiscount);
                State.Offers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = recruit ? OfferKind.Companion : OfferKind.Item, Id = recruit ? available[choice].Id : items[choice].Id, Price = Mathf.Max(1, Mathf.CeilToInt(basePrice * (1f - Mathf.Clamp(discount, -1, 0.5f)))) });
            }
            if (!rollSpecial) return;
            State.RerollsThisShop = 0;
            State.SpecialOffers.Clear();
            if (catalog.AltarRevision > 0)
            {
                if (Next(ref State.ShopRandom, 1000000) >= Mathf.RoundToInt(AltarActivationChance * 1000000)) return;
            }
            else if (catalog.SpecialShopEvery <= 0 || State.Wave % catalog.SpecialShopEvery != 0) return;
            RollSpecialOffers();
        }

        private void RollSpecialOffers()
        {
            if (State.NextOffer > int.MaxValue - 3) throw new InvalidOperationException("Shop token counter exhausted");
            var specialItems = new List<ItemDefinition>();
            foreach (ItemDefinition candidate in catalog.Items) if (Eligible(candidate, ShopPool.Eggs)) specialItems.Add(candidate);
            if (specialItems.Count == 0) throw new InvalidOperationException("No eligible altar offers");
            State.SpecialOffers.Clear();
            var specialUnits = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions)
                if (unit.SpecialRecruit && (!unit.RecruitThemeLocked || K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit)) && unit.UnlockWave <= State.Wave && (State.NewRecruitsThisWave < 3 || State.Roster.Exists(entry => entry.UnitId == unit.Id)) && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && !State.Roster.Exists(entry => entry.UnitId == unit.Id && entry.WeaponId == weapon.Id))) specialUnits.Add(unit);
            for (int index = 0; index < 3; index++)
            {
                var recruits = specialUnits.FindAll(unit => State.Roster.Exists(entry => entry.UnitId == unit.Id));
                if (Next(ref State.ShopRandom, 1000000) < NewRecruitChance * 1000000) recruits = specialUnits;
                if (index == 0 && recruits.Count > 0)
                {
                    UnitDefinition recruit = recruits[Next(ref State.ShopRandom, recruits.Count)];
                    State.SpecialOffers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Companion, Id = recruit.Id, Price = Mathf.CeilToInt(4 * SpecialStagePriceFactor), Currency = ShopCurrency.Eggs });
                    continue;
                }
                ItemDefinition item = specialItems[WeightedItem(specialItems)];
                State.SpecialOffers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = Mathf.Max(1, Mathf.CeilToInt((catalog.AltarRevision > 0 ? item.AltarPrice : catalog.SpecialShopPrice) * (1 + (int)item.Rarity * 0.25f) * SpecialStagePriceFactor)), Currency = catalog.AltarRevision > 0 ? item.AltarCurrency : ShopCurrency.Eggs });
            }
        }

        public bool Altar(string recipeId)
        {
            if (!CanAltar(recipeId)) { Notice = "Altar needs eggs, eligible materials and a unique result weapon."; return false; }
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            RosterEntry first = LowestTier(recipe.FirstUnit, null, recipe.MinimumMaterialTier);
            RosterEntry second = LowestTier(recipe.SecondUnit, first, recipe.MinimumMaterialTier);
            if (first == null || second == null) { Notice = "Altar needs both listed companions."; return false; }
            var choices = new List<WeaponDefinition>();
            foreach (WeaponDefinition weapon in catalog.Weapons) if (RecruitWeaponEligible(recipe.ResultUnit, weapon) && !State.Roster.Exists(entry => entry != first && entry != second && entry.UnitId == recipe.ResultUnit && entry.WeaponId == weapon.Id)) choices.Add(weapon);
            if (choices.Count == 0) return false;
            float previousMaximum = MaximumHealth();
            State.Roster.Remove(first);
            State.Roster.Remove(second);
            State.Roster.Add(new RosterEntry { UnitId = recipe.ResultUnit, Tier = recipe.ResultTier, WeaponId = BalancedWeapon(choices).Id });
            RememberVariant(State.Roster[State.Roster.Count - 1]);
            State.Eggs -= recipe.EggCost;
            RecountBonds();
            Notice = "Altar fusion completed.";
            ReconcileHealth(previousMaximum);
            return true;
        }

        public bool RecruitWeaponEligible(string unitId, WeaponDefinition weapon) => !weapon.SpecialOnly || catalog.Unit(unitId).SpecialRecruit;

        private WeaponDefinition BalancedWeapon(List<WeaponDefinition> choices, bool preferSpecial = false)
        {
            if (catalog.HeroRevision > 0)
            {
                int melee = State.Roster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Melee).Count;
                int ranged = State.Roster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Projectile).Count;
                WeaponKind kind = melee == ranged ? Next(ref State.WeaponRandom, 2) == 0 ? WeaponKind.Melee : WeaponKind.Projectile : melee < ranged ? WeaponKind.Melee : WeaponKind.Projectile;
                List<WeaponDefinition> preferred = choices.FindAll(weapon => weapon.Kind == kind);
                if (preferred.Count > 0) choices = preferred;
            }
            if (preferSpecial)
            {
                var special = choices.FindAll(weapon => weapon.SpecialOnly);
                if (special.Count > 0) choices = special;
            }
            return choices[Next(ref State.WeaponRandom, choices.Count)];
        }

        private float MaximumHealth() => catalog.StatRevision > 0 ? K10StatRules.MaximumHealth(this, catalog) : Mathf.Max(1f, catalog.Unit(State.ChampionId).Health + Bonus(ItemEffect.Health));

        private void ReconcileHealth(float previousMaximum)
        {
            float maximum = MaximumHealth();
            State.Health = Mathf.Clamp(State.Health + maximum - previousMaximum, 1f, maximum);
        }

        public bool CanAltar(string recipeId)
        {
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            if (recipe == null || State.Phase != SessionPhase.Shop || State.Eggs < recipe.EggCost) return false;
            RosterEntry first = LowestTier(recipe.FirstUnit, null, recipe.MinimumMaterialTier);
            RosterEntry second = LowestTier(recipe.SecondUnit, first, recipe.MinimumMaterialTier);
            return first != null && second != null && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(recipe.ResultUnit, weapon) && !State.Roster.Exists(entry => entry != first && entry != second && entry.UnitId == recipe.ResultUnit && entry.WeaponId == weapon.Id));
        }

        public string RecipePreview(string recipeId)
        {
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            if (recipe == null) return "Recipe not found.";
            RosterEntry first = LowestTier(recipe.FirstUnit, null, recipe.MinimumMaterialTier);
            RosterEntry second = LowestTier(recipe.SecondUnit, first, recipe.MinimumMaterialTier);
            string Describe(RosterEntry entry, string id) => entry == null ? catalog.Unit(id).DisplayName + " · missing" : catalog.Unit(id).DisplayName + " / " + entry.WeaponId + " / tier " + entry.Tier;
            return "Spend " + recipe.EggCost + " eggs and two companions:\n− " + Describe(first, recipe.FirstUnit) + "\n− " + Describe(second, recipe.SecondUnit) + "\n→ " + catalog.Unit(recipe.ResultUnit).DisplayName + " tier " + recipe.ResultTier + "; weapon is rolled after fusion, not selected.\n" + (CanAltar(recipeId) ? "READY" : "NOT READY");
        }

        public AltarRecipe[] ActiveRecipes()
        {
            var active = new List<AltarRecipe>();
            foreach (AltarRecipe recipe in catalog.Recipes)
                if (CanAltar(recipe.Id)) active.Add(recipe);
            return active.ToArray();
        }

        public string TierAttackDescription(string weaponId, int tier)
        {
            WeaponDefinition weapon = catalog.Weapon(weaponId);
            if (weapon == null || tier < 1 || tier > catalog.MaximumTier) return "Invalid variant.";
            if (catalog.ContentRevision == 0 || tier == 1) return "Base attack; tier bonuses apply only to companions.";
            int steps = tier - 1;
            return weapon.Kind == WeaponKind.Projectile
                ? "Companions: +" + steps * 18 + "% body, range and bullet size; +" + steps * 15 + "% weapon width; basic volleys +" + steps + " projectiles and pierce. Signature effects retain their own rules."
                : "Companions: +" + steps * 18 + "% body and attack range; +" + steps * 15 + "% weapon width; wider sweeps; attack cycle divided by " + (1f + steps * 0.12f).ToString("0.00") + ". Signature effects retain their own rules.";
        }

        public RosterEntry LowestTier(string id, RosterEntry excluded = null, int minimumTier = 1)
        {
            RosterEntry result = null;
            foreach (RosterEntry entry in State.Roster)
                if (entry.UnitId == id && entry != excluded && entry.Tier >= minimumTier && (result == null || entry.Tier < result.Tier)) result = entry;
            return result;
        }

        public static int BoundedCounter(long value) => (int)Math.Max(0L, Math.Min(int.MaxValue, value));

        private static int Next(ref uint state, int count)
        {
            if (count <= 0) throw new InvalidOperationException("Empty random pool");
            if (state == 0) state = 0xA731u;
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            return (int)(state % (uint)count);
        }

        private void DisposeAssets()
        {
            foreach (UnityEngine.Object asset in runtimeAssets)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(asset); else UnityEngine.Object.DestroyImmediate(asset);
            }
            runtimeAssets.Clear();
        }
        public void Dispose() { DisposeAssets(); }
    }
}
