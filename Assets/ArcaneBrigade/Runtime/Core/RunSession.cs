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
        /// <summary>The fourth form, past 3 stars (LucidRite).</summary>
        public bool Lucid;
    }

    [Serializable]
    public sealed class ItemStack { public string Id; public int Count; }
    [Serializable]
    public sealed class MergeDroughtEntry { public string UnitId; public int Misses; }

    [Serializable]
    public sealed partial class ShopOffer
    {
        public int Token;
        public OfferKind Kind;
        public string Id;
        public int Price;
        public bool Sold;
        public ShopCurrency Currency;
        public int Tier = 1;
    }

    [Serializable]
    public sealed partial class RunSnapshot
    {
        public const int CurrentSchemaRevision = 5;
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
        public int NewSpeciesRecruitsThisWave;
        public int SpecialNewSpeciesRecruitsThisWave;
        public int RegularNewSpeciesPurchased;
        public int RegularAllowanceCarryover;
        public int LastNewShopSpeciesWave;
        public int LastRegularNewSpeciesWave = -10; // regular shop: one new species every two waves (19/10/2026)
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
        public List<MergeDroughtEntry> MergeDrought = new List<MergeDroughtEntry>();
        public List<ShopOffer> Offers = new List<ShopOffer>();
        public List<ShopOffer> SpecialOffers = new List<ShopOffer>();
        public List<ContributionEntry> Contributions = new List<ContributionEntry>();
        public OutsideStageStatus OutsideStage;
        public List<string> OutsideRewards = new List<string>();
        public int OutsideTransfers;
        public int PrismShards;             // three make a rainbow gift at the next shop
        public int PrismShardDrops;         // shards elites have dropped this run (two at most)
        public int OutsideBeforeGift = -1;  // the Keeper's status to return to once a prism gift is taken (-1: the gift is the Keeper's own)
        public int LastOutsideTransferWave;
        public int RelicWave;
        public int RelicDiscountWave;
        public List<StatModifier> RelicGains = new List<StatModifier>();
        public List<StatModifier> RelicFloors = new List<StatModifier>();
        public List<RelicLease> RelicLeases = new List<RelicLease>();
        public List<string> RelicWeaponIds = new List<string>();
    }

    public sealed partial class RunSession : IDisposable
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
            ResetShopStatFeedback();
            if (difficulty < 0 || difficulty >= RunMeta.DifficultyNames.Length || !Array.Exists(RunMeta.ChallengeIds, id => id == challengeId)) throw new ArgumentException("Invalid run mode");
            int challengeIndex = Array.IndexOf(RunMeta.ChallengeIds, challengeId);
            if (challengeIndex > 0 && (difficulty != 0 || seed != RunMeta.ChallengeSeed(challengeIndex))) throw new ArgumentException("Challenge requires its fixed seed and difficulty");
            UnitDefinition champion = catalog.Unit(championId);
            if (champion == null || !Array.Exists(catalog.Champions, unit => unit.Id == championId)) throw new ArgumentException("Unknown hero");
            State = new RunSnapshot
            {
                Version = catalog.Version, RunId = Guid.NewGuid().ToString("N"), ChampionId = championId, Seed = seed,
                Gold = catalog.StartingGold, Health = champion.Health, Phase = SessionPhase.Combat, Difficulty = difficulty, ChallengeId = challengeId,
                ShopRandom = unchecked((uint)seed ^ 0xAC731u), WeaponRandom = unchecked((uint)seed ^ 0xE517u)
            };
            State.Nightmare.NarrativeRevision = 1;
            InitializeSpecialShopSchedule(false);
            if (champion.HeroMechanic != HeroMechanic.SoloMaw)
            {
                State.Roster.Add(catalog.HeroRevision > 0 ? K11HeroRules.Starter(catalog, champion) : new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = catalog.Weapons[1].Id });
                RememberVariant(State.Roster[0]);
                // a second starting friend for the heroes whose gauge runs on friends
                if (catalog.HeroRevision > 0 && HeroLoadout.SecondFriend.TryGetValue(champion.Id, out var second) && catalog.Unit(second.Friend) is UnitDefinition two
                    && K11HeroRules.Eligible(champion, two) && !OwnsCompanionSpecies(second.Friend)) MetaAddFriend(second.Friend, catalog.Weapon(second.Weapon));
            }
            RecountBonds();
            State.Health = MaximumHealth();
        }

        /// <summary>Story bosses that can be fought but are not roster bosses, so never boss discoveries.</summary>
        public static bool IsStoryBossId(string id) => id == "OutsideStageKeeper" || id == NightmareRules.CustomerId;
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
            if (snapshot.SchemaRevision < 4)
            {
                // Old saves did not distinguish gifted from purchased species: conservatively count owned species except the starter.
                State.RegularNewSpeciesPurchased = Mathf.Max(0, CompanionDeployment.Select(State.Roster).Count - 1);
            }
            if (snapshot.SchemaRevision < 5)
            {
                // Keep all allowance earned under the old two-wave schedule, including its unspent balance.
                State.RegularAllowanceCarryover = Math.Max(0, 1 + (State.Wave - 1) / 2 - BaseNewSpeciesAllowance(State.Wave));
                InitializeSpecialShopSchedule(true);
            }
            State.SchemaRevision = RunSnapshot.CurrentSchemaRevision;
            State.OctoberGains ??= new List<StatModifier>();
            State.HeroIdentity ??= new HeroIdentityState();
            State.Meta ??= new MetaRunState();
            State.Meta.HaulLines ??= new List<string>(); State.Meta.Heirloom ??= ""; State.Meta.StartFriend ??= ""; State.Meta.AltStarter ??= ""; State.Meta.RewindShop ??= "";
            State.AbsorbedSkills ??= new List<string>();
            State.AchievementEvidence ??= new List<AchievementMetric>();
            State.RelicGains ??= new List<StatModifier>();
            State.RelicFloors ??= new List<StatModifier>();
            State.RelicLeases ??= new List<RelicLease>();
            State.RelicWeaponIds ??= new List<string>();
            if (State.Receipts == null) State.Receipts = new List<RunTransactionReceipt>();
            if (State.MergeDrought == null) State.MergeDrought = new List<MergeDroughtEntry>();
            if (State.SpecialOffers == null) State.SpecialOffers = new List<ShopOffer>();
            if (State.Contributions == null) State.Contributions = new List<ContributionEntry>();
            int shelf = catalog.ShopSlots + KitExtraSlots + MetaExtraSlots;
            if (State.Offers.Count > shelf) State.Offers.RemoveRange(shelf, State.Offers.Count - shelf);
            RecountBonds();
            State.Health = Mathf.Min(State.Health, MaximumHealth());
        }

        public void Validate(RunSnapshot snapshot)
        {
            // runs saved while a story boss was wrongly recorded as a discovery: drop it here, where loading and every
            // transaction check validate, so the run loads and plays on
            snapshot?.DefeatedBosses?.RemoveAll(id => IsStoryBossId(id) && !Array.Exists(catalog.Bosses, boss => boss.Id == id));
            ValidateHeroIdentity(snapshot);
            ValidateSecretEvents(snapshot);
            ValidateOutsideStage(snapshot);
            ValidateNightmare(snapshot);
            ValidateRelicState(snapshot);
            if (snapshot != null && (!float.IsFinite(snapshot.BloodPaid) || snapshot.BloodPaid < 0)) throw new InvalidOperationException("Invalid altar HP record");
            ValidateOctoberState(snapshot);
            if (snapshot != null && snapshot.SchemaRevision >= 3 && (snapshot.Difficulty < 0 || snapshot.Difficulty >= RunMeta.DifficultyNames.Length || !Array.Exists(RunMeta.ChallengeIds, id => id == snapshot.ChallengeId) || snapshot.StartHall < 1 || snapshot.StartHall > snapshot.Wave + (long)1 || snapshot.DefeatedBosses == null || !float.IsFinite(snapshot.LastDamageAmount) || snapshot.LastDamageAmount < 0)) throw new InvalidOperationException("Invalid run meta identity");
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
            if (!Array.Exists(catalog.Champions, unit => unit.Id == snapshot.ChampionId)) throw new InvalidOperationException("Invalid hero identity");
            if (snapshot.Roster == null || snapshot.Items == null || snapshot.Offers == null || snapshot.TotalKills < 0 || snapshot.BossKills < 0 || snapshot.NextOffer < 0) throw new InvalidOperationException("Incomplete or invalid save counters");
            if (snapshot.NewRecruitsThisWave < 0 || snapshot.NewRecruitsThisWave > 3) throw new InvalidOperationException("Invalid wave recruitment quota");
            if (snapshot.NewSpeciesRecruitsThisWave < 0 || snapshot.NewSpeciesRecruitsThisWave > 2 || snapshot.NewSpeciesRecruitsThisWave > snapshot.NewRecruitsThisWave) throw new InvalidOperationException("Invalid new-species quota");
            if(snapshot.SpecialNewSpeciesRecruitsThisWave<0 || snapshot.SpecialNewSpeciesRecruitsThisWave>1 || snapshot.LastNewShopSpeciesWave<0 || snapshot.LastNewShopSpeciesWave>snapshot.Wave)throw new InvalidOperationException("Invalid shared new-species cooldown");
            if (snapshot.MergeDrought != null)
            {
                var mergeKeys = new HashSet<string>();
                foreach (MergeDroughtEntry entry in snapshot.MergeDrought)
                    if (entry == null || entry.Misses < 0 || entry.Misses > 6 || !Array.Exists(catalog.Companions, unit => unit.Id == entry.UnitId) || !mergeKeys.Add(entry.UnitId)) throw new InvalidOperationException("Invalid merge assistance journal");
            }
            if (snapshot.RegularNewSpeciesPurchased < 0) throw new InvalidOperationException("Invalid saved recruitment allowance");
            if (snapshot.RegularAllowanceCarryover < 0) throw new InvalidOperationException("Invalid recruitment allowance carryover");
            if (snapshot.SchemaRevision >= 5 && (snapshot.NextSpecialShopWave < 1 || snapshot.SpecialShopRollWave < -1 || snapshot.SpecialShopRollWave > snapshot.Wave)) throw new InvalidOperationException("Invalid saved dream-shop schedule");
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
                if (entry == null || !Array.Exists(catalog.Companions, unit => unit.Id == entry.UnitId) || catalog.Weapon(entry.WeaponId) == null || entry.Tier < 1 || entry.Tier > catalog.MaximumTier || snapshot.SchemaRevision < 4 && !rosterKeys.Add(entry.UnitId + "/" + entry.WeaponId)) throw new InvalidOperationException("Invalid roster entry");
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
            => RelicBonus(effect, target, RawBonus(effect, target));

        private float RawBonus(ItemEffect effect, ModifierScope target)
        {
            float value = 0f;
            foreach (StatModifier modifier in catalog.Unit(State.ChampionId).ChampionModifiers ?? new StatModifier[0])
                if (modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value;
            foreach (RosterEntry entry in ActiveRoster)
                foreach (StatModifier modifier in catalog.Unit(entry.UnitId).CompanionModifiers ?? new StatModifier[0])
                    if (modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value * UnitDefinition.ModifierScale(entry.Tier);
            bool loom = MechanicStacks(RelicMechanic.DreamLoom) > 0; // Dream Loom: Common items give double
            foreach (ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                float woven = loom && item.Rarity == ItemRarity.Common ? 2 : 1;
                foreach (StatModifier modifier in item.EffectiveModifiers)
                    if (!SecretItemSuspended(stack.Id) && modifier.Effect == effect && RunModifierRules.Applies(modifier.Scope, target)) value += modifier.Value * woven * item.EffectiveStacks(stack.Count) * (modifier.Value > 0 && SecretItemImproved(stack.Id) ? 1.25f : 1) * (modifier.Value > 0 ? KitItemScale(item, stack.Count, target) : 1);
            }
            foreach (BondDefinition bond in catalog.Bonds)
                if (bond.Effect == effect && RunModifierRules.Applies(RunModifierRules.DefaultScope(bond.Effect), target) && BondCounts.TryGetValue(bond.Id, out int count)) value += bond.Reward(count);
            value += SecretBonus(effect, target);
            value += OctoberBonus(effect, target);
            foreach (string id in State.AbsorbedSkills ?? new List<string>())
                foreach (StatModifier m in catalog.Unit(id).CompanionModifiers ?? Array.Empty<StatModifier>())
                    if (m.Effect == effect && RunModifierRules.Applies(m.Scope, target)) value += m.Value;
            value += KitBonus(effect, target);
            value += MetaBonus(effect, target); // the Waiting Room's Lantern and Ledger (play-test item 27)
            if (OctoberHero == "Wise" && target == ModifierScope.Champion && value > 0) value *= 1.25f;
            return value;
        }

        private void RecountBonds()
        {
            ResolveStoredCopies();
            BondCounts.Clear();
            foreach (RosterEntry entry in ActiveRoster)
                if (!Stranger(entry.UnitId))
                foreach (string tag in catalog.Unit(entry.UnitId).Tags)
                    BondCounts[tag] = BondCounts.TryGetValue(tag, out int count) ? count + 1 : 1;
            // a lucid friend counts one more member in its main bond (LucidRite)
            foreach (RosterEntry entry in ActiveRoster)
                if (entry.Lucid && !Stranger(entry.UnitId) && catalog.Unit(entry.UnitId).Tags.Length > 0 && BondCounts.TryGetValue(catalog.Unit(entry.UnitId).Tags[0], out int lucidCount))
                    BondCounts[catalog.Unit(entry.UnitId).Tags[0]] = lucidCount + 1;
            // a Rare friend counts twice in its minor bond once that bond is already active (content expansion v5): it adds a
            // level but cannot start one on its own
            if (catalog.PoolRevision > 0)
                foreach (RosterEntry entry in ActiveRoster)
                {
                    UnitDefinition unit = catalog.Unit(entry.UnitId);
                    if (Stranger(entry.UnitId) || unit.Rarity != ItemRarity.Rare || unit.Tags.Length < 2) continue;
                    BondDefinition bond = Array.Find(catalog.Bonds, b => b.Id == unit.Tags[1]);
                    if (bond != null && BondCounts.TryGetValue(bond.Id, out int members) && members >= bond.Threshold) BondCounts[bond.Id] = members + 1;
                }
            // Moon Clock Key: every active bond counts one more member
            if (MechanicStacks(RelicMechanic.MoonClockKey) > 0)
                foreach (BondDefinition bond in catalog.Bonds)
                    if (BondCounts.TryGetValue(bond.Id, out int active) && active >= bond.Threshold) BondCounts[bond.Id] = active + 1;
            // the busker's song: for the wave it is sung, the chosen bond counts one more friend
            if (State.Phase == SessionPhase.Combat && VisitorEffectFor("Song", State.Wave) is VisitorEffect song && BondCounts.TryGetValue(song.Target, out int sung)) BondCounts[song.Target] = sung + 1;
            if (Talent("\"The All-Knowing\" Walter") > 0)
                foreach (BondDefinition bond in catalog.Bonds) BondCounts[bond.Id] = BondCounts.TryGetValue(bond.Id, out int count) ? count + 1 : 1;
            MetaBonds();
            KitBonds();
        }

        public CheckpointCatalog PrepareCombat()
        {
            DisposeAssets();
            RecountBonds();
            BeginRelicWave();
            CombatCatalog = Clone(catalog.Arena);
            CombatCatalog.EnableHeroMechanics = catalog.HeroRevision > 0;
            CombatCatalog.ArenaSize = catalog.WorldSize;
            List<RosterEntry> deployed = ActiveRoster;
            CombatCatalog.StartingCompanions = new UnitDefinition[deployed.Count];
            CombatCatalog.ExplicitStartingRoster = true;
            CombatCatalog.Champion = BuildUnit(catalog.Unit(State.ChampionId), catalog.Unit(State.ChampionId).Weapon, 1, false);
            for (int index = 0; index < deployed.Count; index++)
            {
                RosterEntry entry = deployed[index];
                CombatCatalog.StartingCompanions[index] = BuildUnit(catalog.Unit(entry.UnitId), catalog.Weapon(entry.WeaponId), entry.Tier, true);
                if (entry.Lucid) ApplyLucid(CombatCatalog.StartingCompanions[index]);
            }
            AddMemoryExhibits();
            PrepareStoryParty();
            float scaling = (1f + (State.Wave - 1) * JourneyBalance.WaveHealthStep) * JourneyBalance.EnemyHealth(State.Difficulty, State.Wave);
            CombatCatalog.MeleeEnemy = BuildEnemy(catalog.Enemies[(State.Wave - 1) % catalog.Enemies.Length], scaling);
            CombatCatalog.RangedEnemy = BuildEnemy(catalog.Enemies[State.Wave % catalog.Enemies.Length], scaling);
            if (State.Wave == 1)
            {
                UnitDefinition firstContact = Array.Find(catalog.Enemies, enemy => enemy.Weapon.Kind == WeaponKind.Contact && enemy.UnlockWave <= 1);
                if (firstContact == null) throw new InvalidOperationException("Wave1 requires an unlocked contact enemy");
                CombatCatalog.MeleeEnemy = BuildEnemy(firstContact, scaling);
                CombatCatalog.RangedEnemy = CombatCatalog.MeleeEnemy;
            }
            // Act I closes each of its four zones with a story boss (waves 5, 10, 15, 20); endless keeps the catalog rhythm.
            bool actOne = !State.Endless && State.Wave <= catalog.NormalWaves;
            bool bossWave = actOne ? ActOneCanon.BossWave(State.Wave) : State.Wave % catalog.BossEvery == 0;
            int bossStage = actOne ? State.Wave / 5 - 1 : State.Wave / catalog.BossEvery - 1;
            int bossIndex = Mathf.Max(0, bossStage) % catalog.Bosses.Length;
            if (catalog.Bosses.Length == 10) bossIndex = Mathf.Max(0, bossStage) % 5 + (((State.Seed ^ bossStage) & 1) == 0 ? 0 : 5);
            CombatCatalog.Boss = bossWave ? BuildEnemy(catalog.Bosses[bossIndex], scaling) : null;
            if (actOne && bossWave) CombatCatalog.Boss = StoryBoss(CombatCatalog.Boss, scaling);
            if (actOne && bossWave && CombatCatalog.Boss != null) CombatCatalog.Boss.Health *= WaveCadence.StoryBossBulk(State.Wave);
            CombatCatalog.WaveDuration = actOne && !bossWave ? WaveCadence.WaveLength(State.Wave) : OctoberBalance.WaveSeconds;
            CombatCatalog.EnemyBudget = Mathf.CeilToInt((30 + Mathf.Min(State.Wave, 30) * 3) * CarnivalBalance.EnemyDensity(State.Wave) * JourneyBalance.EnemyDensity(State.Difficulty, State.Wave));
            CombatCatalog.EnemyBudget = Mathf.Min(Mathf.CeilToInt(600 * CarnivalBalance.EnemyCountScale), Mathf.CeilToInt(CombatCatalog.EnemyBudget * (State.Wave < 3 ? 1.4f : 1.65f * WaveCadence.SwarmPressure(State.Wave))));
            // waves are 15% longer (D9): the budget grows with them so enemies per second stay the same
            CombatCatalog.EnemyBudget = Mathf.Min(Mathf.CeilToInt(690 * CarnivalBalance.EnemyCountScale), Mathf.CeilToInt(CombatCatalog.EnemyBudget * WaveCadence.LengthScale));
            CombatCatalog.SpawnInterval = CarnivalBalance.SpawnInterval(State.Wave);
            CombatCatalog.CompanionLeash = catalog.Arena.CompanionLeash + Bonus(ItemEffect.Roam);
            CombatCatalog.CompanionRoamOuterRadius = CombatCatalog.CompanionLeash * 0.84f;
            CombatCatalog.PickupRadius = Mathf.Max(3.2f, catalog.Arena.PickupRadius * 2.4f) + Bonus(ItemEffect.Pickup);
            CombatCatalog.CompanionInterceptor = Bonus(ItemEffect.Interceptor) > 0f;
            CombatCatalog.PassiveHeal = Bonus(ItemEffect.Heal);
            CombatCatalog.Armor = Bonus(ItemEffect.Armor);
            if (catalog.StatRevision > 0)
            {
                CombatCatalog.PassiveHeal *= K10StatRules.Factor(Bonus(ItemEffect.HealCorrection));
                CombatCatalog.Armor *= K10StatRules.Factor(Bonus(ItemEffect.ArmorCorrection));
                CombatCatalog.Armor = Mathf.Min(2.5f, OctoberBalance.Soften(CombatCatalog.Armor, .8f, Mathf.Lerp(1, .25f, OctoberBalance.Late(State.Wave))));
                CombatCatalog.Dodge = Mathf.Clamp(Bonus(ItemEffect.Dodge), 0, OctoberHero == "Sir Purrcival" ? .9f : .6f); // original: his dodge cap is 90%
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
            if (CombatCatalog.Champion.HeroMechanic == HeroMechanic.RootedOracle)
            {
                CombatCatalog.PickupRadius = CombatCatalog.ChestOpenRadius = catalog.WorldSize.magnitude;
                CombatCatalog.PassiveHeal += 1.5f;
            }
            var procs = new List<ActiveProc>();
            foreach (ProcRule rule in catalog.ProcRules ?? new ProcRule[0])
            {
                int stacks = catalog.Item(rule.ItemId).EffectiveStacks(StackCount(rule.ItemId));
                if (stacks > 0 && !SecretItemSuspended(rule.ItemId)) procs.Add(new ActiveProc { Rule = rule, Stacks = stacks });
            }
            CombatCatalog.Procs = procs.ToArray();
            CombatCatalog.EnableArenaToys = false; // no arena toys in the fight any more (requirement 19/10/2026): no bell, no cart; scenery stays
            CombatCatalog.RunWave = State.Wave;
            CombatCatalog.RunDifficulty = State.Difficulty;
            CombatCatalog.ChestGoldScale = GoldScale;
            CombatCatalog.ChestGold = (float)MeanShopPrice; // an average chest is worth about one mid-priced offer of the last shop
            CombatCatalog.EnableBossArenaPhase = catalog.SignatureRevision > 0;
            CombatCatalog.ExpandedContent = catalog.ExpansionRevision > 0;
            CombatCatalog.EnableSpectacles = catalog.ExpansionRevision > 0;
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
                AddWaitingEnemies(pool, scaling); // enemy roster v2: only species whose art exists, in their own zone
                EnemyVarietyRun.EndlessTraits(pool, State.Wave, State.Endless, Clone<UnitDefinition>);
                CombatCatalog.MapElite = EnemyVarietyRun.MapElite(catalog, State.Wave, State.Endless, enemy => BuildEnemy(enemy, scaling));
                CombatCatalog.EnemyPool = pool.ToArray();
                var empowered = new List<UnitDefinition>();
                foreach (UnitDefinition candidate in pool)
                {
                    if (empowered.Exists(unit => unit.Id == candidate.Id)) continue;
                    UnitDefinition variant = Clone(candidate); variant.Weapon = Clone(candidate.Weapon);
                    variant.Health *= 2.4f; variant.BodyHeight *= 1.3f; variant.Radius *= 1.25f;
                    variant.Speed *= 1.08f; variant.ContactDamage *= 1.45f; variant.Weapon.Damage *= 1.45f;
                    empowered.Add(variant);
                }
                CombatCatalog.AffixedEnemies = empowered.ToArray();
            }
            foreach (ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id); int count = item.EffectiveStacks(stack.Count);
                if (SecretItemSuspended(stack.Id)) continue;
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
                CombatCatalog.SpawnInterval = Mathf.Max(0.25f, CombatCatalog.SpawnInterval * Mathf.Lerp(1, active.SpawnMultiplier, 0.5f));
                CombatCatalog.EnemyBudget = Mathf.CeilToInt(CombatCatalog.EnemyBudget * Mathf.Lerp(1, active.BudgetMultiplier, 0.5f));
                CombatCatalog.GoldPerKill = active.GoldPerKill;
                CombatCatalog.PassiveHeal += active.Healing;
            }
            // Raw rewards grow slowly (1 -> 3); kill coins are batched below to keep the floor clear.
            CombatCatalog.GoldPerKill = Mathf.Max(1, Mathf.RoundToInt(CombatCatalog.GoldPerKill * 0.5f * (1 + Mathf.Min(2, Mathf.Max(0, State.Wave - 1) / 6f))));
            CombatCatalog.MinionCoin = 1;
            CombatCatalog.KillCoinBatchSize = CarnivalBalance.KillCoinBatch(State.Wave);
            CombatCatalog.KillCoinRewardPercent = 90;
            CombatCatalog.CoinValueScale = CarnivalBalance.CoinGoldValue;
            CombatCatalog.EnemyBudget = Mathf.Min(690, CombatCatalog.EnemyBudget);
            PrepareVisitors();
            CombatCatalog.BossSwarmBudget = bossWave ? Mathf.CeilToInt(CombatCatalog.EnemyBudget * 0.22f) : 0;
            CombatCatalog.FinalDuel = false;
            CombatCatalog.PrismDrops = State.Endless ? 0 : Mathf.Max(0, 2 - State.PrismShardDrops);
            PrepareOutsideStage();
            PrepareSecretCombat();
            PrepareSitCloser();
            PrepareWaitingEnemies();
            PrepareMapIdentity(bossWave);
            ConfigureRelics();
            PrepareNightmare();
            PrepareBuildMorphs();
            CompileOctoberRoster();
            PrepareHeroIdentity();
            PrepareMeta();
            ApplyEnemyDifficultyRelief();
            RunAchievements.Observe(this, catalog, CombatCatalog);
            // the arena's fixed props (none in secret rooms): toys and spectacles keep off them
            CombatCatalog.Scenery = SecretRoomOpen ? (ArenaScenery?)null : ArenaScenery.For(State, CombatCatalog.ArenaSize);
            return CombatCatalog;
        }

        private void ApplyEnemyDifficultyRelief()
        {
            // Apply last: special encounters replace boss health after BuildEnemy, and pools can share definitions.
            var enemies = new HashSet<UnitDefinition>();
            void Add(UnitDefinition unit) { if (unit != null) enemies.Add(unit); }
            Add(CombatCatalog.MeleeEnemy); Add(CombatCatalog.RangedEnemy); Add(CombatCatalog.Boss); Add(CombatCatalog.SecretPartner);
            foreach (var unit in CombatCatalog.EnemyPool ?? Array.Empty<UnitDefinition>()) Add(unit);
            foreach (var unit in CombatCatalog.HerdPool ?? Array.Empty<UnitDefinition>()) Add(unit); // the swarm's unthemed fodder (item 24)
            foreach (var unit in CombatCatalog.AffixedEnemies ?? Array.Empty<UnitDefinition>()) Add(unit);
            foreach (var unit in enemies)
                unit.Health *= OctoberBalance.EnemyHealthRelief * (unit.IsBoss ? OctoberBalance.BossHealthRelief : OctoberBalance.NormalEnemyHealth);
        }

        private UnitDefinition BuildUnit(UnitDefinition source, WeaponDefinition weapon, int tier, bool companion)
        {
            UnitDefinition unit = Clone(source);
            ModifierScope target = companion ? ModifierScope.Companions : ModifierScope.Champion;
            unit.Weapon = Clone(weapon);
            if (companion && catalog.ContentRevision > 0 && tier > 1)
                CompanionTierScaling.Apply(unit.Weapon, tier);
            unit.CombatTier = tier;
            if (!companion) unit.CollisionDamage = Mathf.Max(3, source.CollisionDamage);
            unit.PassiveStrength = source.Passive == PassiveKind.Scavenge ? ScavengeGold(source.PassiveStrength, tier, catalog.TierSupportMultiplier, GoldScale)
                : source.PassiveStrength * Mathf.Pow(catalog.TierSupportMultiplier, tier - 1);
            unit.Weapon.Damage *= FriendlyWeaponBalance.Scale(weapon.Id) * DamageMultiplier(source, tier, companion);
            unit.Weapon.Cooldown /= Mathf.Clamp(1f + OctoberBalance.Soften(Bonus(ItemEffect.AttackSpeed, target) + (companion ? Bonus(ItemEffect.CompanionAttackSpeed) : 0f), .6f, PlayerBuildBalance.AttackSpeedSlope(companion)), .25f, PlayerBuildBalance.AttackSpeedCap(companion));
            unit.Weapon.Windup = Mathf.Min(unit.Weapon.Windup, unit.Weapon.Cooldown * 0.35f);
            unit.Weapon.Recovery = Mathf.Min(unit.Weapon.Recovery, unit.Weapon.Cooldown * 0.4f);
            unit.Weapon.ProjectileCount += Mathf.FloorToInt(Bonus(ItemEffect.Projectiles, target));
            unit.CriticalChance = Mathf.Clamp01(unit.CriticalChance + Bonus(ItemEffect.Crit, target));
            unit.Speed *= Mathf.Max(0.1f, 1f + CarnivalBalance.MoveBonus(Bonus(ItemEffect.MoveSpeed, target))) * CarnivalBalance.UnitPace;
            if (!companion) unit.Health = Mathf.Max(1f, unit.Health + Bonus(ItemEffect.Health));
            if (companion) unit.BodyHeight *= CompanionTierScaling.Body(tier);
            K10StatRules.ApplyUnit(this, catalog, unit, source, weapon, tier, companion);
            if (!companion && source.HeroMechanic == HeroMechanic.SoloMaw) { unit.Weapon.Damage *= 1.6f * (1 + Mathf.Min(1, Mathf.Max(0, State.Wave - 1) * .04f)); unit.Weapon.Range *= 1.4f; }
            if (!companion && source.Id == "LoneJelly") unit.Weapon.Damage *= JourneyBalance.LoneJellyDamage(State.Wave);
            if (!companion && source.HeroMechanic == HeroMechanic.RootedOracle) { unit.Weapon.Damage *= 1.8f; unit.Weapon.Range *= 1.6f; }
            if (companion && source.SpecialRecruit) unit.Weapon.Damage *= 1.35f;
            if (unit.Weapon.Kind == WeaponKind.Melee)
            {
                MeleeBalance.Apply(unit.Weapon);
                if (companion) unit.Speed *= MeleeBalance.Speed;
            }
            else if (companion) { unit.Weapon.Range *= CarnivalBalance.RangedAllyReach; unit.Weapon.ProjectileLifetime *= CarnivalBalance.RangedAllyReach; }
            if (!companion)
            {
                // requirement (22/10/2026): the hero's hits were too far apart; faster swings for less per hit (about the same damage a second)
                unit.Weapon.Cooldown *= HeroTempo; unit.Weapon.Damage *= HeroTempoDamage;
                unit.Weapon.Windup = Mathf.Min(unit.Weapon.Windup, unit.Weapon.Cooldown * 0.35f);
                unit.Weapon.Recovery = Mathf.Min(unit.Weapon.Recovery, unit.Weapon.Cooldown * 0.4f);
            }
            KitCompileUnit(unit, source, companion);
            ApplySecretUnit(unit, companion);
            OctoberBalance.CompileAlly(unit, State.Wave);
            return unit;
        }

        public float DamageMultiplier(UnitDefinition unit, int tier, bool companion)
        {
            float value = Mathf.Pow(Mathf.Min(2.7f, catalog.TierDamageMultiplier), tier - 1) * Mathf.Max(0.1f, 1f + OctoberBalance.Soften(Bonus(ItemEffect.Damage, companion ? ModifierScope.Companions : ModifierScope.Champion) + Bonus(companion ? ItemEffect.CompanionDamage : ItemEffect.ChampionDamage), 1.2f, PlayerBuildBalance.DamageSlope(companion)));
            if (unit.Passive == PassiveKind.Rally) value *= 1f + unit.PassiveStrength * Mathf.Pow(catalog.TierSupportMultiplier, tier - 1) * 0.01f * ActiveRoster.Count;
            return value * (companion ? CarnivalBalance.RecruitDamage(unit.Rarity) : PlayerBuildBalance.HeroPower(unit.Id));
        }

        /// <summary>The Keeper guards the gate at wave 5 with the strength of an ordinary wave-5 boss; the Chair Porter and the
        /// Door That Stayed Shut use stand-in bodies (Suitcase Leviathan, Undelivered King) until their own art lands.</summary>
        private UnitDefinition StoryBoss(UnitDefinition reference, float scaling)
        {
            if (reference == null) return null;
            // wave 5: the Coin Widow, who collects for the Last Customer and has chained the Keeper to its own gate. The Keeper
            // is not fought on the road; it carries the shop (hand, carpet, eye) and tests the traveller once, at shop 17.
            string stand = State.Wave == 5 ? "CoinWidow" : State.Wave == 10 ? "SuitcaseLeviathan" : State.Wave == 15 ? "UndeliveredKing" : null;
            UnitDefinition body = stand == null ? null : Array.Find(catalog.Bosses, boss => boss != null && boss.Id == stand);
            UnitDefinition story = body != null && body.Id != reference.Id ? BuildEnemy(body, scaling) : reference;
            // the first boss night keeps the first boss's weight: the Coin Widow borrows its health and blows (as the Keeper did)
            if (State.Wave == 5 && story != reference) { story.Health = reference.Health; story.ContactDamage = reference.ContactDamage; story.Weapon.Damage = reference.Weapon.Damage; }
            if (ActOneCanon.BossName(State.Wave) != null) story.DisplayName = ActOneCanon.BossName(State.Wave);
            return story;
        }

        private UnitDefinition BuildEnemy(UnitDefinition source, float scaling)
        {
            UnitDefinition unit = Clone(source);
            unit.Weapon = Clone(source.Weapon);
            EnemyEvolutionLibrary.Apply(unit, State.Wave);
            EnemyAffixLibrary.ApplyArt(unit, State.Wave);
            WaitingEnemyRoster.ApplyArt(unit, State.Wave);
            unit.KnockbackResistance = source.IsBoss ? 1 : Mathf.Max(source.KnockbackResistance, source.BodyHeight >= 1.2f ? 0.6f : 0.15f);
            unit.Weapon.ProjectileSpeed = Mathf.Min(2.4f, unit.Weapon.ProjectileSpeed);
            unit.Health *= scaling * (source.IsBoss ? catalog.BossHealthMultiplier : 1);
            if (source.IsBoss) { unit.Health *= CarnivalBalance.BossBulk(State.Wave) * BossEncounterRules.HealthMultiplier; unit.BodyHeight *= BossEncounterRules.BodyScale(source.BossPattern) * .8f; unit.Radius *= BossEncounterRules.CollisionScale * .8f; }
            else if (source.Behavior >= EnemyBehavior.Hopper) unit.Health *= 0.9f;
            unit.Health *= CarnivalBalance.EnemyHealth(State.Wave) * 1.1f * .8f * OctoberBalance.EndlessHealth(State.Wave, State.Endless);
            if (source.SpecialEnemy != SpecialEnemyKind.None) { unit.Health *= 1.6f; unit.BodyHeight *= 1.2f; unit.Radius *= 1.15f; }
            unit.Speed = (source.IsBoss ? unit.Speed : CarnivalBalance.CapEnemySpeed(unit.Speed)) * CarnivalBalance.EnemySpeed(State.Wave) * CarnivalBalance.UnitPace * CarnivalBalance.EnemyPace * MetaEnemySpeed; // Night Pact: Swift night
            float damageGrowth = Mathf.Max(0, State.Wave - 10);
            unit.ContactDamage = Mathf.Min(14f + damageGrowth * 0.12f, source.ContactDamage + State.Wave * 0.35f);
            unit.Weapon.Damage = Mathf.Min((source.IsBoss ? 26f : 14f) + damageGrowth * (source.IsBoss ? 0.2f : 0.12f), source.Weapon.Damage + State.Wave * 0.35f);
            float pressure = JourneyBalance.EnemyDamage(State.Difficulty, State.Wave) * CarnivalBalance.EnemyDamage(State.Wave) * 1.08f * OctoberBalance.EndlessDamage(State.Wave, State.Endless);
            pressure *= MetaEnemyDamage; // the Lamp Tree's Night hunter
            unit.ContactDamage *= pressure*1.08f*1.6f; unit.Weapon.Damage *= pressure*1.08f;
            unit.ContactInterval=Mathf.Max(.45f,unit.ContactInterval*.9f);
            if (source.IsBoss) { unit.ContactDamage *= BossEncounterRules.DamageMultiplier; unit.Weapon.Damage *= BossEncounterRules.DamageMultiplier; unit.Speed *= BossEncounterRules.SpeedMultiplier; }
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
            OctoberSettled(simulation);
            MetaSettled(simulation);
            if (SettleOutsideStage(simulation)) return;
            if (SettleSecretCombat(simulation)) return;
            if (SettleSitCloser(simulation)) return;
            State.Health = simulation.Champion.Health;
            State.BuildScoreMultiplier = catalog.StatRevision > 0 ? K10StatRules.Factor(Bonus(ItemEffect.ScoreMultiplier), 0.1f, 100) : 1;
            State.LastDamageSource = simulation.LastDamageSource;
            State.LastDamageAmount = simulation.LastDamageAmount;
            State.LastDamageKind = simulation.LastDamageKind;
            simulation.SweepRemaining();
            State.Gold = BoundedCounter((long)State.Gold + simulation.Gold + MetaGold(simulation.Gold)); // + the Museum's gold gift
            SettleNewRelics(simulation); SettleVisitors(simulation); SettleMapIdentity(simulation);
            State.TotalKills = BoundedCounter((long)State.TotalKills + simulation.Kills);
            State.CombatSeconds += simulation.Elapsed;
            ContributionLedger.Merge(State.Contributions, simulation.Contributions);
            State.Eggs = BoundedCounter((long)State.Eggs + simulation.Eggs + (simulation.BossDefeated ? 3 : 1));
            State.PrismShards = Mathf.Min(99, State.PrismShards + simulation.PrismShards); State.PrismShardDrops = Mathf.Min(99, State.PrismShardDrops + simulation.PrismShards);
            // a story boss (wave 10 or 15) beaten without the traveller ever falling below half health gives a shard
            if (simulation.BossDefeated && !State.Endless && (State.Wave == 10 || State.Wave == 15) && simulation.LowestChampionShare >= 0.5f) State.PrismShards = Mathf.Min(99, State.PrismShards + 1);
            if (simulation.BossDefeated)
            {
                State.BossKills = BoundedCounter((long)State.BossKills + 1);
                if (simulation.Boss.Definition.Id == NightmareRules.CustomerId)
                {
                    State.Nightmare ??= new NightmareProgress();
                    State.Nightmare.CustomerDefeated = true; State.Nightmare.Ending = NightmareEndingStage.Dawn; State.Nightmare.EndingPage = 0;
                }
                // only roster bosses are discoveries; story bosses outside the roster (the Keeper of the outside stage,
                // like the Last Customer above) were once recorded here and made every later save fail validation
                else if (Array.Exists(catalog.Bosses, boss => boss.Id == simulation.Boss.Definition.Id)) RunMeta.Add(State.DefeatedBosses, simulation.Boss.Definition.Id);
            }
            if (simulation.Phase == RunPhase.Defeat) { State.Phase = SessionPhase.Defeat; return; }
            State.Health = MaximumHealth();
            if (!State.Endless && State.Wave >= catalog.NormalWaves) { State.LastIncome = State.LastInterest = 0; SettleSecretContracts(); State.Phase = SessionPhase.Victory; return; }
            State.LastInterest = Mathf.Min(Mathf.Min(10, catalog.InterestCap) + Mathf.FloorToInt(Bonus(ItemEffect.Interest)), KitInterestBase(State.Gold) / Mathf.Max(1, catalog.InterestDivisor));
            State.LastIncome = BoundedCounter((long)catalog.Income * 13 / 20 + State.Wave + Mathf.FloorToInt(Bonus(ItemEffect.Income)) + KitWaveEnd(simulation.BossDefeated));
            State.Gold = BoundedCounter((long)State.Gold + State.LastInterest + State.LastIncome);
            SettleSecretContracts();
            State.Phase = SessionPhase.Shop;
            State.DrinksBeforeShop = State.OctoberDrinks; // Red Scale: drinks bought in this shop make the next fight busier
            State.SpecialShopVisited = false;
            State.NewRecruitsThisWave = 0;
            State.NewSpeciesRecruitsThisWave = 0;
            State.SpecialNewSpeciesRecruitsThisWave = 0;
            RollShop(true);
            OfferPrismGift();
            VisitorNewShop();
        }

        public void ContinueWave()
        {
            if (State.Phase != SessionPhase.Shop) return;
            if (State.OutsideStage == OutsideStageStatus.Reward) return;
            if (SecretRoomOpen) return;
            if (State.Wave == int.MaxValue) { Notice = "Technical hall counter limit reached. Records remain saved."; return; }
            VisitorLeaveShop();
            MetaRememberShop();
            State.Wave++;
            State.Phase = SessionPhase.Combat;
            State.Offers.Clear();
            State.SpecialOffers.Clear();
            Notice = "";
        }

        public void EnterEndless()
        {
            if (EndingPending) return;
            if (State.Phase != SessionPhase.Victory) return;
            ResetShopStatFeedback();
            State.Endless = true;
            State.NewRecruitsThisWave = 0;
            State.NewSpeciesRecruitsThisWave = 0;
            State.SpecialNewSpeciesRecruitsThisWave = 0;
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
            bool countsAsNewRecruit = CountsAsNewRegularRecruit(offer);
            bool countsAsNewSpecialRecruit=offer.Kind==OfferKind.Companion && State.SpecialOffers.Contains(offer) && !OwnsCompanionSpecies(offer.Id);
            bool countsTowardRecruitLimit = CountsTowardRecruitLimit(offer);
            float previousMaximum = MaximumHealth();
            bool stork = false;
            if (offer.Kind == OfferKind.Companion)
            {
                var choices = new List<WeaponDefinition>();
                foreach (WeaponDefinition weapon in catalog.Weapons)
                    if (RecruitWeaponEligible(offer.Id, weapon) && VariantFree(offer.Id, weapon)) choices.Add(weapon);
                if (choices.Count == 0) { Notice = "No unique weapon variant available."; return false; }
                // the weapon shown on the offer before buying (rolled once per offer); never rolled again here
                WeaponDefinition chosen = OfferWeapon(offer) ?? BalancedWeapon(choices, catalog.Unit(offer.Id).SpecialRecruit);
                var recruited = new RosterEntry { UnitId = offer.Id, WeaponId = chosen.Id, Tier = Mathf.Clamp(offer.Tier, 1, 3) };
                stork = recruited.Tier == 1 && MetaStork(offer);
                if (stork) recruited.Tier = 2;
                MetaWelcomeStar(recruited);
                bool ownedBefore = OwnsCompanionSpecies(offer.Id);
                State.Roster.Add(recruited);
                KitRecruited(recruited, ownedBefore, choices);
                if (countsTowardRecruitLimit) State.NewRecruitsThisWave++;
                if (countsAsNewRecruit || countsAsNewSpecialRecruit) RegisterNewShopSpecies(countsAsNewSpecialRecruit);
                RememberVariant(State.Roster[State.Roster.Count - 1]);
                OctoberPurchase(catalog.Unit(offer.Id));
                if (offer.Id == "BottleGull") State.OctoberDrinks++; // Bottle Gull counts as a drink
                VisitorBoughtCompanion(offer);
                if (OctoberAbsorbs) { State.AbsorbedSkills.Add(offer.Id); State.Roster.Remove(recruited); KitAbsorbed(); }
                MergeCompanions(offer.Id);
                TrackMergeDrought(false);
            }
            else
            {
                ItemStack stack = State.Items.Find(candidate => candidate.Id == offer.Id);
                ItemDefinition item = catalog.Item(offer.Id);
                if (stack != null && stack.Count >= item.StackLimit) { Notice = "Item stack limit reached."; return false; }
                if (stack == null) State.Items.Add(new ItemStack { Id = offer.Id, Count = 1 }); else stack.Count++;
                BuyRelic(item, offer.Token);
                if (item.Category == RelicCategory.Drink) State.OctoberDrinks++;
                if (item.Category == RelicCategory.Food) State.OctoberFood++;
                RememberRelicPurchase(offer);
                OctoberBoughtRelic(item);
            }
            if (offer.Currency == ShopCurrency.Eggs) State.Eggs -= offer.Price;
            else if (offer.Currency == ShopCurrency.Health) State.BloodPaid += offer.Price; // round 4: maximum HP drops for good; ReconcileHealth below takes the same amount off current HP
            else State.Gold -= offer.Price;
            offer.Sold = true;
            KitBought(offer);
            HeroIdentityBought(offer);
            RecountBonds();
            ReconcileHealth(previousMaximum);
            State.BuildScoreMultiplier = catalog.StatRevision > 0 ? K10StatRules.Factor(Bonus(ItemEffect.ScoreMultiplier), 0.1f, 100) : 1;
            Notice = offer.Kind == OfferKind.Companion ? "Purchased. One fighter per species; extra copies wait for merging." : "Upgrade stacks for this run.";
            if (stork) Notice = "The stork brought it at 2★.";
            RetireClosedNewSpecies(); // the quota may have just closed: no unbuyable new friend stays on the shelf
            return true;
        }

        public bool CanBuy(ShopOffer offer)
        {
            if (SecretRoomOpen) return false;
            if (State.OutsideStage == OutsideStageStatus.Reward) return false;
            if (RecruitQuotaBlocks(offer)) return false;
            if (offer != null && offer.Kind == OfferKind.Companion && OctoberAbsorbs && State.AbsorbedSkills.Count >= 2048) return false;
            if (CompanionCapacityBlocks(offer)) return false;
            if (offer == null || offer.Sold || State.Phase != SessionPhase.Shop) return false;
            if (offer.Kind == OfferKind.Companion && catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw && !OctoberAbsorbs) return false;
            if (offer.Currency == ShopCurrency.Health ? !BloodAffordable(offer.Price) : (offer.Currency == ShopCurrency.Eggs ? State.Eggs : State.Gold) < offer.Price) return false;
            if (offer.Kind == OfferKind.Item) return StackCount(offer.Id) < catalog.Item(offer.Id).StackLimit;
            return Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(offer.Id, weapon) && VariantFree(offer.Id, weapon));
        }

        private void MergeCompanions(string id)
        {
            if (catalog.Unit(id).ReferenceCompanion == "Evil") return;
            for (int tier = 1; tier < catalog.MaximumTier; tier++)
            {
                List<RosterEntry> materials = State.Roster.FindAll(entry => entry.UnitId == id && entry.Tier == tier);
                while (materials.Count >= CompanionDeployment.MergeCopies)
                {
                    List<RosterEntry> consumed = materials.GetRange(0, CompanionDeployment.MergeCopies);
                    var choices = new List<WeaponDefinition>();
                    foreach (RosterEntry material in consumed)
                        choices.Add(catalog.Weapon(material.WeaponId));
                    if (choices.Count == 0) break;
                    for (int index = 0; index < CompanionDeployment.MergeCopies; index++) State.Roster.Remove(materials[index]);
                    materials.RemoveRange(0, CompanionDeployment.MergeCopies);
                    string resultId = id;
                    if (catalog.Unit(id).ReferenceCompanion == "Little Cabbage")
                    {
                        UnitDefinition[] pool = Array.FindAll(catalog.Companions, u => !u.SpecialRecruit && !StorySpecies(u.Id) && string.IsNullOrEmpty(u.ExclusiveHero) && u.UnlockWave <= State.Wave && K11HeroRules.Eligible(catalog.Unit(State.ChampionId), u));
                        pool = HeroNurseryPool(pool);
                        if (pool.Length > 0) resultId = pool[Next(ref State.WeaponRandom, pool.Length)].Id;
                    }
                    string inherited = choices[Next(ref State.WeaponRandom, choices.Count)].Id;
                    State.Roster.Add(new RosterEntry { UnitId = resultId, Tier = tier + 1, WeaponId = inherited });
                    RememberVariant(State.Roster[State.Roster.Count - 1]);
                    KitMerged(); RelicMerged(resultId, tier + 1);
                    if (catalog.Unit(id).ReferenceCompanion == "Little Cabbage")
                    {
                        var nurseryWeapons = choices;
                        if (nurseryWeapons.Count > 0)
                        {
                            var grown = new RosterEntry { UnitId = "GrandSprout", Tier = 2, WeaponId = nurseryWeapons[Next(ref State.WeaponRandom, nurseryWeapons.Count)].Id };
                            State.Roster.Add(grown); RememberVariant(grown);
                        }
                    }
                }
            }
        }

        public bool Reroll()
        {
            if (SecretRoomOpen) return false;
            if (State.OutsideStage == OutsideStageStatus.Reward) return false;
            int price = RerollCost;
            if (State.Phase != SessionPhase.Shop || State.Gold < price) return false;
            if (State.RerollsThisShop == int.MaxValue) return false;
            State.Gold -= price;
            KitRerollLuck(price);
            RememberShelf(price);
            RollShop();
            State.RerollsThisShop++;
            if (MechanicStacks(RelicMechanic.HagglersAbacus) > 0) ApplyKitPrices();
            OctoberRefreshed();
            Notice = "Shop refreshed.";
            return true;
        }

        // originals: Wise and Nicole refresh +5 (Nicole's own discount line adds 2 of it), Belle -3 (her line takes ~1)
        public int RerollCost => MetaFreeRefresh ? 0 : Mathf.Max(0, KitRerollCost(RerollBaseCost) - (Meta?.RerollDiscount ?? 0) + (OctoberHero == "Wise" ? 5 : OctoberHero == "Nicole" ? 3 : OctoberHero == "Belle" ? -2 : 0));
        // balance pass 19/10/2026: the 2 + 2n refresh fee follows half the shop's price stage once that exceeds x2 (unchanged early)
        private float RerollStage => Mathf.Max(1, StagePriceFactor * .5f);
        // the stage factor applies after the relic/talent/hero adjustments, so Ink Feather and Mary still reach the 1-gold minimum
        // play-test 20/10/2026 (item 12): the first refresh costs ~8% of this shop's mean offer price, each further one in the
        // same shop 50% more; it resets every wave. Relic/talent/hero adjustments stay flat amounts on the old stage.
        public const float RerollShare = .08f, RerollGrowth = 1.5f;
        public double MeanShopPrice
        {
            get
            {
                double sum = 0; int n = 0;
                foreach (ShopOffer o in State.Offers) if (o.Currency == ShopCurrency.Gold && o.BasePrice > 0) { sum += o.BasePrice; n++; }
                return n > 0 ? sum / n : MidOfferPrice * .4f;
            }
        }
        // a feather or Mary takes one first-refresh fee off (the first refresh then costs the 1-gold minimum); Nicole adds one, Belle takes half off
        private int RerollBaseCost => Mathf.Max(1, (int)Math.Min(int.MaxValue, Math.Ceiling(MeanShopPrice * RerollShare * (Math.Pow(RerollGrowth, State.RerollsThisShop)
            - (MechanicStacks(RelicMechanic.RerollFeather) + Talent("\"Leisurely Walk\" Mary") - (OctoberHero == "Nicole" ? 1 : OctoberHero == "Belle" ? -.5 : 0))))));
        /// <summary>A typical relic's gold price in this shop (the median catalogue relic, its rarity markup, the wave's curve).</summary>
        public float MidOfferPrice
        {
            get
            {
                if (midOfferBase <= 0)
                {
                    var prices = new System.Collections.Generic.List<float>();
                    foreach (ItemDefinition item in catalog.Items) if (item.ShopWeight > 0 && item.Price > 0) prices.Add(item.Price * (1 + (int)item.Rarity * 0.4f));
                    prices.Sort(); midOfferBase = prices.Count == 0 ? 20 : prices[prices.Count / 2];
                }
                return midOfferBase * StagePriceFactor;
            }
        }
        private float midOfferBase;
        private void RememberVariant(RosterEntry entry)
        {
            if (!State.AcquiredVariants.Exists(candidate => candidate.UnitId == entry.UnitId && candidate.WeaponId == entry.WeaponId && candidate.Tier == entry.Tier))
                State.AcquiredVariants.Add(new RosterEntry { UnitId = entry.UnitId, WeaponId = entry.WeaponId, Tier = entry.Tier });
        }
        public bool RerollAltar()
        {
            if (SecretRoomOpen) return false;
            if (State.Phase != SessionPhase.Shop || State.SpecialOffers.Count == 0 || State.Gold < RerollCost || State.RerollsThisShop == int.MaxValue) return false;
            State.Gold -= RerollCost;
            TrackMergeDrought(true, State.SpecialOffers);
            RollSpecialOffers();
            State.RerollsThisShop++;
            OctoberRefreshed();
            Notice = "Altar refreshed with gold. Eggs and HP unchanged.";
            return true;
        }
        // Kept for read-model/API compatibility; availability now follows the saved 3–4-wave schedule, not held eggs.
        public float AltarActivationChance => !MetaNoDreamShop && (State.Wave >= State.NextSpecialShopWave || OctoberHero == "Periwinky") ? 1 : 0;

        private bool Eligible(ItemDefinition item, ShopPool pool) => item.UnlockWave <= State.Wave && OctoberItemEligible(item) && (catalog.EconomyRevision == 0 || item.ShopWeight > 0 && ReviewBalance.ExclusivePool(item) == pool) && StackCount(item.Id) < item.StackLimit;

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

        private int ItemWeight(ItemDefinition item) => Mathf.Max(1, Mathf.RoundToInt(BaseItemWeight(item) * LoneWeight(item) * FamilyWeight(item)));
        /// <summary>The thin hero families (requirement 23/10/2026: each hero's shop should fit it) also lean on shared relics that suit
        /// them: those come 2.5x as often for a hero of that family, without being taken from anyone else.</summary>
        private static readonly System.Collections.Generic.Dictionary<string, string[]> FamilyExtras = new System.Collections.Generic.Dictionary<string, string[]>
        {
            ["fam-kills"] = new[] { "TallyChalk", "CurioHungryLedger", "SpecimenPin", "FrostLedger" },
            ["fam-delay"] = new[] { "CuckooPendulum", "clock-spring", "CurioQuietHour", "OctoberRelic2", "LateFee" },
            ["fam-merge"] = new[] { "StarPolish", "SpliceKit", "RainbowHeart" },
            ["fam-freeze"] = new[] { "FrostSalt", "BrainFreeze", "MeltingSundae", "FrostLedger", "echo-postage" },
            ["fam-marks"] = new[] { "PostageStamp", "CurioFangPin", "echo-postage" },
        };
        private float FamilyWeight(ItemDefinition item)
        {
            foreach (string theme in catalog.Unit(State.ChampionId).RecruitThemes ?? System.Array.Empty<string>())
                if (FamilyExtras.TryGetValue(theme, out string[] ids) && System.Array.IndexOf(ids, item.Id) >= 0) return 2.5f;
            return 1;
        }
        /// <summary>The Lone line comes three times as often while two friends or fewer are deployed, less with a big brigade.</summary>
        private float LoneWeight(ItemDefinition item) => item.Mechanic < RelicMechanic.LoneLantern ? 1 : ActiveRoster.Count <= 2 ? 3 : ActiveRoster.Count >= 6 ? .4f : 1;
        private int BaseItemWeight(ItemDefinition item) => catalog.EconomyRevision == 0 ? 1 : Mathf.Max(1, Mathf.RoundToInt(item.ShopWeight * K10StatRules.Factor((Bonus(ItemEffect.ShopRarity) + KitRarityBonus + (catalog.StatRevision > 0 ? Bonus(ItemEffect.Luck) * 0.25f : 0)) * (int)item.Rarity, 0.1f, 4) * (item.Category == RelicCategory.Drink ? 1 + .05f * MechanicStacks(RelicMechanic.DrinkTempo) + (State.RelicDiscountWave == State.Wave ? 1 : 0) : 1)));

        public float NewRecruitChance
        {
            get
            {
                int count = ActiveRoster.Count;
                float chance = OctoberBalance.NewAllyChance(count, State.Wave);
                return Mathf.Clamp(chance * Mathf.Max(0.25f, 1 + Bonus(ItemEffect.Luck)), 0.01f, 0.95f);
            }
        }

        public float StagePriceFactor => CarnivalBalance.ShopPrice(State.Wave) * WaveCadence.PriceRamp(State.Wave);
        /// <summary>One price curve for gold (play-test 20/10/2026, items 12/17/20): the shop's own factor. Authored gold
        /// amounts (visitor prices and rewards, secret-event fees, chest gold) are wave-1 amounts scaled by it.</summary>
        public const float HeroTempo = .7f, HeroTempoDamage = .73f;
        public float GoldScale => StagePriceFactor;
        public int ScaledGold(int authored) => ShopVisitors.Scale(authored, GoldScale);
        /// <summary>A Scavenge passive's gold every 4s (requirement 22/10/2026: Kettle Bat "finds 0 gold"): the authored strength (0.5,
        /// once rounded half-to-even to 0) times the star support, on the gold curve, rounded half up and never below 1.</summary>
        public static int ScavengeGold(float strength, int tier, float support, float goldScale) =>
            Math.Max(1, (int)Math.Min(int.MaxValue, Math.Round(strength * Math.Pow(support, Mathf.Clamp(tier, 1, 3) - 1) * Math.Max(1f, goldScale * .15f), MidpointRounding.AwayFromZero))); // .15: late 3-star finds ~50 a tick (~10–15% of a wave's income), not a second income
        /// <summary>The Scavenge gold of a friend at its stars in this shop or wave (what its card says).</summary>
        public int ScavengeGold(UnitDefinition unit, int tier) => ScavengeGold(unit.PassiveStrength, tier, catalog.TierSupportMultiplier, GoldScale);
        /// <summary>What a visitor asks in this shop: its authored price on the gold curve (the world tag, the card and the
        /// rules all read this one number).</summary>
        public int VisitorCost(VisitorDefinition v) => v == null ? 0 : ScaledGold(v.Cost);
        // requirement 22/10/2026: eggs pile up late, so the dream shop's egg prices rise with the waves (x1.18 at wave 5, x1.63 at 10, x2.5 at 20)
        public const int MergeBias = 30, MergeSureAfter = 6;
        /// <summary>Wise's shop sells another Dream Seeker (from wave 2, half the shops, one at a time, up to five a run): it
        /// takes the place of the last relic on the shelf.</summary>
        private void OfferWiseHero()
        {
            if (OctoberHero != "Wise" || State.Wave < 2 || Next(ref State.ShopRandom, 100) >= 50) return;
            int owned = 0; foreach (ItemStack stack in State.Items) if (WiseHeroItems.Is(stack.Id)) owned++;
            if (owned >= 5) return;
            var heroes = new List<UnitDefinition>();
            foreach (UnitDefinition c in catalog.Champions) if (c != null && c.Id != State.ChampionId && WiseHeroItems.Offerable(c) && StackCount(WiseHeroItems.IdFor(c)) == 0) heroes.Add(c);
            int slot = State.Offers.FindLastIndex(o => o.Kind == OfferKind.Item && !o.Sold);
            if (heroes.Count == 0 || slot < 0) return;
            UnitDefinition pick = heroes[Next(ref State.ShopRandom, heroes.Count)];
            int price = Mathf.CeilToInt(60 * StagePriceFactor);
            State.Offers[slot] = new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Item, Id = WiseHeroItems.IdFor(pick), Tier = 1, Price = price, BasePrice = price };
        }
        public float SpecialStagePriceFactor => 1 + Mathf.Max(0, State.Wave - 3) * .09f;
        public float CompanionOfferChance => CompanionPurchasesRemaining == 0 ? 0 : State.NewRecruitsThisWave >= 2 ? .15f : 1;
        public int MergeMisses(string id) => State.MergeDrought.Find(entry => entry.UnitId == id)?.Misses ?? 0;
        public int HighestOwnedTier(string id)
        {
            int tier = 0; foreach (RosterEntry entry in State.Roster) if (entry.UnitId == id) tier = Mathf.Max(tier, entry.Tier);
            return tier;
        }
        public float VeteranPrice(string id) => HighestOwnedTier(id) >= 3 ? 3 : HighestOwnedTier(id) == 2 ? 2 : 1;
        private int RecruitChoice(List<UnitDefinition> units)
        {
            int Weight(UnitDefinition unit) => HighestOwnedTier(unit.Id) >= 3 ? 8 : HighestOwnedTier(unit.Id) == 2 ? 25 : 100;
            int total = 0; foreach (UnitDefinition unit in units) total += Weight(unit);
            int ticket = Next(ref State.ShopRandom, total);
            for (int index = 0; index < units.Count; index++) { ticket -= Weight(units[index]); if (ticket < 0) return index; }
            return units.Count - 1;
        }
        private void TrackMergeDrought(bool paidRefresh, List<ShopOffer> offers = null)
        {
            State.MergeDrought.RemoveAll(entry => !MergeReady(entry.UnitId));
            if (!paidRefresh || offers != State.SpecialOffers && CompanionPurchasesRemaining == 0 || catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) return;
            foreach (RosterEntry unit in State.Roster)
            {
                if (!MergeReady(unit.UnitId)) continue;
                MergeDroughtEntry drought = State.MergeDrought.Find(entry => entry.UnitId == unit.UnitId);
                if (drought == null) { drought = new MergeDroughtEntry { UnitId = unit.UnitId }; State.MergeDrought.Add(drought); }
            }
            foreach (MergeDroughtEntry drought in State.MergeDrought)
            {
                UnitDefinition unit = catalog.Unit(drought.UnitId);
                if (unit.SpecialRecruit != (offers == State.SpecialOffers) || unit.UnlockWave > State.Wave || !Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && VariantFree(unit.Id, weapon))) continue;
                drought.Misses = (offers ?? State.Offers).Exists(offer => !offer.Sold && offer.Kind == OfferKind.Companion && offer.Id == drought.UnitId) ? 0 : Mathf.Min(6, drought.Misses + 1);
            }
        }

        private void RollShop(bool rollSpecial = false)
        {
            TrackMergeDrought(!rollSpecial);
            if (rollSpecial) { State.KitPurchasesThisShop = 0; NewShopForRelics(); }
            State.Offers.Clear();
            if (State.NextOffer > int.MaxValue - catalog.ShopSlots - KitExtraSlots - MetaExtraSlots - 3) throw new InvalidOperationException("Shop token counter exhausted");
            var units = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions) if (RegularShelfSpecies(unit) && RegularRecruitOpen(unit.Id)) units.Add(unit);
            var items = new List<ItemDefinition>();
            foreach (ItemDefinition item in catalog.Items) if (Eligible(item, ShopPool.Gold)) items.Add(item);
            if (items.Count == 0 && units.Count == 0) throw new InvalidOperationException("No eligible shop offers");
            var foundation = State.Wave <= 5 ? items.FindAll(item => ShopFoundationRules.Useful(catalog.Unit(State.ChampionId), item)) : new List<ItemDefinition>();
            for (int index = 0; index < catalog.ShopSlots + KitExtraSlots + MetaExtraSlots; index++)
            {
                bool newRecruit = Next(ref State.ShopRandom, 1000000) < NewRecruitChance * 1000000;
                var available = units.FindAll(unit => OwnsCompanionSpecies(unit.Id) || newRecruit);
                var owedNewSpecies = NewRecruitsRemaining > 0 && index == 0 ? units.FindAll(unit => !OwnsCompanionSpecies(unit.Id)) : new List<UnitDefinition>();
                if (owedNewSpecies.Count > 0) available = owedNewSpecies;
                var owned = units.FindAll(unit => OwnsCompanionSpecies(unit.Id)
                    && !State.Offers.Exists(o => !o.Sold && o.Kind == OfferKind.Companion && o.Id == unit.Id));
                int offeredCopies = State.Offers.FindAll(o => !o.Sold && o.Kind == OfferKind.Companion && OwnsCompanionSpecies(o.Id)).Count;
                bool upgradeSlot = State.Wave >= 10 && index == 1 && offeredCopies < CopyRecruitsRemaining && owned.Count > 0 && Next(ref State.ShopRandom, 100) < 35; // a sometimes-slot (was every shop)
                if (upgradeSlot) available = owned;
                var mergeReady = available.FindAll(unit => MergeReady(unit.Id));
                int drought = 0; foreach (UnitDefinition unit in mergeReady) drought = Mathf.Max(drought, MergeMisses(unit.Id));
                // requirement 23/10/2026: a copy to merge came almost every refresh (85%); now 30% (+5% a refresh missed), sure after 6 misses
                if (mergeReady.Count > 0 && (drought >= MergeSureAfter || Next(ref State.ShopRandom, 100) < MergeBias + drought * 5)) available = drought >= MergeSureAfter ? mergeReady.FindAll(unit => MergeMisses(unit.Id) == drought) : mergeReady;
                bool recruit = available.Count > 0 && (upgradeSlot || owedNewSpecies.Count > 0 || items.Count == 0 || index == 0 && Next(ref State.ShopRandom, 1000000) < CompanionOfferChance * 1000000);
                var itemChoices = index == 1 && foundation.Count > 0 ? foundation : items;
                int choice = recruit ? RecruitChoice(available) : WeightedItem(itemChoices);
                int offerTier = recruit ? OctoberOfferTier(available[choice].Id) : 1;
                int basePrice = Mathf.CeilToInt((recruit ? available[choice].Price * VeteranPrice(available[choice].Id) * (offerTier == 3 ? 3.5f : offerTier == 2 ? 1.8f : 1) : itemChoices[choice].Price * 0.85f * (1 + (int)itemChoices[choice].Rarity * 0.4f)) * StagePriceFactor);
                float discount = Bonus(ItemEffect.Discount) + Bonus(recruit ? ItemEffect.RecruitDiscount : ItemEffect.RelicDiscount);
                State.Offers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = recruit ? OfferKind.Companion : OfferKind.Item, Id = recruit ? available[choice].Id : itemChoices[choice].Id, Tier = offerTier, Price = Mathf.Max(1, Mathf.CeilToInt(basePrice * (1f - Mathf.Clamp(discount, -1, 0.5f)))) });
            }
            MetaRareFind(items);
            MetaRegularDeal();
            OfferWiseHero();
            ApplyHeroOrder();
            ApplyKitPrices(); kitRarerRoll = false;
            if (!rollSpecial) return;
            if (State.SpecialShopRollWave == State.Wave) return;
            State.SpecialShopRollWave = State.Wave;
            State.RerollsThisShop = 0;
            State.SpecialOffers.Clear();
            if (MetaNoDreamShop) return; // Night Pact: no dream market
            if (State.Wave < State.NextSpecialShopWave && OctoberHero != "Periwinky") return; // original: Periwinky always finds the Dream Eater's Altar
            RollSpecialOffers();
            State.NextSpecialShopWave = BoundedCounter((long)State.Wave + 3 + Next(ref State.ShopRandom, 2) - (Meta != null && Meta.DreamSooner ? 1 : 0)); // the Lamp Tree's Early market
        }

        private void RollSpecialOffers()
        {
            if (State.NextOffer > int.MaxValue - 4) throw new InvalidOperationException("Shop token counter exhausted");
            var specialItems = new List<ItemDefinition>();
            foreach (ItemDefinition candidate in catalog.Items) if (Eligible(candidate, ShopPool.Eggs)) specialItems.Add(candidate);
            if (specialItems.Count == 0) throw new InvalidOperationException("No eligible altar offers");
            State.SpecialOffers.Clear();
            var specialUnits = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions)
                if ((catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw || OctoberAbsorbs) && unit.SpecialRecruit && K11HeroRules.Eligible(catalog.Unit(State.ChampionId), unit) && unit.UnlockWave <= State.Wave && Array.Exists(catalog.Weapons, weapon => RecruitWeaponEligible(unit.Id, weapon) && VariantFree(unit.Id, weapon))) specialUnits.Add(unit);
            for (int index = 0; index < 3 + Mathf.Clamp(Meta?.DreamExtra ?? 0, 0, 1); index++) // the Lamp Tree's Wide stall: a fourth
            {
                var recruits = specialUnits.FindAll(unit => OwnsCompanionSpecies(unit.Id));
                if (SpecialNewRecruitsRemaining>0 && Next(ref State.ShopRandom, 1000000) < NewRecruitChance * 1000000) recruits = specialUnits;
                var mergeReady = recruits.FindAll(unit => MergeReady(unit.Id));
                int drought = 0; foreach (UnitDefinition unit in mergeReady) drought = Mathf.Max(drought, MergeMisses(unit.Id));
                if (mergeReady.Count > 0 && (drought >= MergeSureAfter || Next(ref State.ShopRandom, 100) < MergeBias + drought * 5)) recruits = drought >= MergeSureAfter ? mergeReady.FindAll(unit => MergeMisses(unit.Id) == drought) : mergeReady;
                if (index == 0 && recruits.Count > 0)
                {
                    UnitDefinition recruit = recruits[RecruitChoice(recruits)];
                    int tier = OctoberOfferTier(recruit.Id);
                    State.SpecialOffers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Companion, Id = recruit.Id, Tier = tier, Price = Mathf.CeilToInt(4 * SpecialStagePriceFactor * VeteranPrice(recruit.Id) * (tier == 3 ? 3.5f : tier == 2 ? 1.8f : 1)), Currency = ShopCurrency.Eggs });
                    continue;
                }
                ItemDefinition item = specialItems[WeightedItem(specialItems)];
                State.SpecialOffers.Add(new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Item, Id = item.Id, Price = SpecialItemPrice(item), Currency = catalog.AltarRevision > 0 ? item.AltarCurrency : ShopCurrency.Eggs });
            }
        }

        /// <summary>An altar item paid in HP costs a share of the hero's maximum HP, never less than its authored price. Round 2:
        /// 8/12/18/25/32% (floors 8/13/18/27) of current HP. Round 4: the price is now taken from MAXIMUM HP for the run (BloodPrice),
        /// so the shares are 5/8/12/18/24% (floors 5/9/12/18): about a third of what the item is worth, and each purchase lowers the next.</summary>
        private static readonly float[] AltarHealthShare = { .05f, .08f, .12f, .18f, .18f, .24f };
        public int SpecialItemPrice(ItemDefinition item)
        {
            int authored = Mathf.Max(1, Mathf.CeilToInt((catalog.AltarRevision > 0 ? item.AltarPrice : catalog.SpecialShopPrice) * (1 + (int)item.Rarity * 0.25f) * SpecialStagePriceFactor));
            if (catalog.AltarRevision == 0 || item.AltarCurrency != ShopCurrency.Health) return authored;
            return Mathf.Max(authored, Mathf.CeilToInt(AltarHealthShare[Mathf.Clamp((int)item.Rarity, 0, AltarHealthShare.Length - 1)] * MaximumHealth()));
        }

        public bool Altar(string recipeId)
        {
            if (SecretRoomOpen) return false;
            if (!CanAltar(recipeId)) { Notice = "Altar needs eggs, eligible materials and a unique result weapon."; return false; }
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            List<RosterEntry> materials = AltarMaterials(recipe);
            if (materials == null) return false;
            var choices = new List<WeaponDefinition>();
            if (LockedWeapon(catalog.Unit(recipe.ResultUnit))) choices.Add(catalog.Unit(recipe.ResultUnit).Weapon); // a Mythic result carries its own weapon
            else foreach (RosterEntry r in materials)
                if (RecruitWeaponEligible(recipe.ResultUnit, catalog.Weapon(r.WeaponId))) choices.Add(catalog.Weapon(r.WeaponId));
            if (choices.Count == 0) return false;
            float previousMaximum = MaximumHealth();
            foreach (RosterEntry r in materials) State.Roster.Remove(r);
            var result = new RosterEntry { UnitId = recipe.ResultUnit, Tier = recipe.ResultTier, WeaponId = choices[Next(ref State.WeaponRandom, choices.Count)].Id };
            State.Roster.Add(result); RememberVariant(result); OctoberPurchase(catalog.Unit(recipe.ResultUnit));
            if (OctoberAbsorbs) { State.AbsorbedSkills.Add(result.UnitId); State.Roster.Remove(result); }
            State.Eggs -= recipe.EggCost;
            RecountBonds();
            Notice = "Altar fusion completed.";
            ReconcileHealth(previousMaximum);
            return true;
        }

        public bool RecruitWeaponEligible(string unitId, WeaponDefinition weapon)
        {
            UnitDefinition unit = catalog.Unit(unitId);
            // content expansion v5: a Mythic or Prismatic friend always carries its own weapon, and nobody else rolls it
            if (LockedWeapon(unit)) return unit.Weapon == weapon;
            if (catalog.PoolRevision > 0 && LockedWeaponIds.Contains(weapon.Id)) return false;
            return !weapon.SpecialOnly || unit.SpecialRecruit;
        }
        /// <summary>Copies are stored merge ingredients, including repeated weapons; max-star species need no more shop materials.</summary>
        public bool VariantFree(string unitId, WeaponDefinition weapon) => OctoberAbsorbs || !State.Roster.Exists(e => e.UnitId == unitId && e.Tier >= catalog.MaximumTier) && (catalog.Unit(unitId).ReferenceCompanion != "Evil" || !OwnsCompanionSpecies(unitId));
        private bool LockedWeapon(UnitDefinition unit) => catalog.PoolRevision > 0 && unit != null && unit.Rarity >= ItemRarity.Mythic && unit.Weapon != null;
        private HashSet<string> lockedWeaponIds;
        private HashSet<string> LockedWeaponIds
        {
            get
            {
                if (lockedWeaponIds != null) return lockedWeaponIds;
                lockedWeaponIds = new HashSet<string>();
                foreach (UnitDefinition unit in catalog.Companions) if (LockedWeapon(unit)) lockedWeaponIds.Add(unit.Weapon.Id);
                return lockedWeaponIds;
            }
        }

        private WeaponDefinition BalancedWeapon(List<WeaponDefinition> choices, bool preferSpecial = false)
        {
            if (catalog.HeroRevision > 0)
            {
                int melee = ActiveRoster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Melee).Count;
                int ranged = ActiveRoster.FindAll(entry => catalog.Weapon(entry.WeaponId).Kind == WeaponKind.Projectile).Count;
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

        private float MaximumHealth() => catalog.StatRevision > 0 ? K10StatRules.MaximumHealth(this, catalog) : BloodPrice.Apply(Mathf.Max(1f, catalog.Unit(State.ChampionId).Health + Bonus(ItemEffect.Health)), State.BloodPaid);

        private void ReconcileHealth(float previousMaximum)
        {
            float maximum = MaximumHealth();
            State.Health = Mathf.Clamp(State.Health + maximum - previousMaximum, 1f, maximum);
        }

        public bool CanAltar(string recipeId)
        {
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            if (recipe == null || State.Phase != SessionPhase.Shop || State.Eggs < recipe.EggCost) return false;
            List<RosterEntry> materials = AltarMaterials(recipe);
            return materials != null && (LockedWeapon(catalog.Unit(recipe.ResultUnit)) || materials.Exists(r => RecruitWeaponEligible(recipe.ResultUnit, catalog.Weapon(r.WeaponId))));
        }

        public string RecipePreview(string recipeId)
        {
            AltarRecipe recipe = Array.Find(catalog.Recipes, candidate => candidate.Id == recipeId);
            if (recipe == null) return "Recipe not found.";
            var result = new System.Text.StringBuilder("Spend " + recipe.EggCost + " eggs and the listed materials:\n");
            foreach (string id in new[] { recipe.FirstUnit, recipe.SecondUnit, recipe.ThirdUnit })
                if (!string.IsNullOrEmpty(id)) result.Append("- ").Append(catalog.Unit(id).DisplayName).Append(" (stars ").Append(recipe.MinimumMaterialTier).Append("+)\n");
            result.Append("Result: ").Append(catalog.Unit(recipe.ResultUnit).DisplayName).Append(" / stars ").Append(recipe.ResultTier).Append("\nKeeps one random ingredient weapon.\n").Append(catalog.Unit(recipe.ResultUnit).Description).Append("\n").Append(CanAltar(recipeId) ? "READY" : "NOT READY");
            return result.ToString();
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
            if (catalog.ContentRevision == 0 || tier == 1) return "Base attack; stars bonuses apply only to friends.";
            int steps = tier - 1;
            int size = Mathf.RoundToInt((CompanionTierScaling.Body(tier) - 1) * 100);
            return weapon.Kind == WeaponKind.Projectile
                ? "Friends: +" + size + "% body, range, weapon and bullet size; basic volleys +" + steps + " projectiles and pierce. Signature effects retain their own rules."
                : "Friends: +" + size + "% body, weapon and attack range; wider sweeps; attack cycle divided by " + (1f + steps * 0.12f).ToString("0.00") + ". Signature effects retain their own rules.";
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
