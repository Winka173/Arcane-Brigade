using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum OutsideStageStatus { Unseen, Declined, Fighting, Reward, Completed }

    public sealed partial class RunSession
    {
        private void ValidateOutsideStage(RunSnapshot snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("Missing run");
            if (!Enum.IsDefined(typeof(OutsideStageStatus), snapshot.OutsideStage) || snapshot.OutsideTransfers < 0 || snapshot.OutsideTransfers > 1000 || snapshot.LastOutsideTransferWave < 0 || snapshot.LastOutsideTransferWave > snapshot.Wave) throw new InvalidOperationException("Invalid outside-stage progress");
            if (snapshot.OutsideStage == OutsideStageStatus.Fighting && snapshot.Phase != SessionPhase.Combat && snapshot.Phase != SessionPhase.Defeat) throw new InvalidOperationException("Invalid challenge phase");
            if (snapshot.PrismShards < 0 || snapshot.PrismShards > 99 || snapshot.PrismShardDrops < 0 || snapshot.PrismShardDrops > 99 || snapshot.OutsideBeforeGift < -1 || snapshot.OutsideBeforeGift > (int)OutsideStageStatus.Completed
                || snapshot.OutsideBeforeGift >= 0 && snapshot.OutsideStage != OutsideStageStatus.Reward || snapshot.OutsideBeforeGift == (int)OutsideStageStatus.Reward || snapshot.OutsideBeforeGift == (int)OutsideStageStatus.Fighting)
                throw new InvalidOperationException("Invalid prism shards");
            if (snapshot.OutsideStage != OutsideStageStatus.Reward) return;
            if (snapshot.Phase != SessionPhase.Shop || snapshot.OutsideRewards == null || snapshot.OutsideRewards.Count != 3 || new HashSet<string>(snapshot.OutsideRewards).Count != 3) throw new InvalidOperationException("Invalid prismatic choices");
            foreach (string id in snapshot.OutsideRewards)
            {
                ItemDefinition item = catalog.Item(id); UnitDefinition ally = catalog.Unit(id);
                if (item != null && item.Rarity == ItemRarity.Rainbow) continue;
                if (ally == null || ally.Rarity != ItemRarity.Rainbow || catalog.Unit(snapshot.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) throw new InvalidOperationException("Invalid prismatic reward");
            }
        }
        public bool OutsideChallengeAvailable => !SecretRoomOpen && State.Phase == SessionPhase.Shop && State.OutsideStage == OutsideStageStatus.Unseen && (State.Endless || State.Wave > ActOneCanon.Waves ? State.OutsideTransfers >= 3 : State.Wave == ActOneCanon.KeeperReturnShop) && OutsideStageAssets.Current != null;
        /// <summary>The Keeper's hand in Act I (shop 17) rather than the endless rematch.</summary>
        private bool ActOneKeeperReturn => !State.Endless && State.Wave <= ActOneCanon.Waves;
        /// <summary>The whole Keeper's health as a share of this run's wave-15 story boss (with its x3.5 bulk).</summary>
        public const float KeeperHealthShare = 1.5f;
        public void RegisterOutsideTransfer()
        {
            if (State.LastOutsideTransferWave >= State.Wave) return;
            State.LastOutsideTransferWave = State.Wave;
            State.OutsideTransfers = Mathf.Min(1000, State.OutsideTransfers + 1);
        }
        public bool DeclineOutsideStage()
        {
            if (!OutsideChallengeAvailable) return false;
            State.OutsideStage = OutsideStageStatus.Declined; return true;
        }
        public bool AcceptOutsideStage()
        {
            if (!OutsideChallengeAvailable) return false;
            State.OutsideStage = OutsideStageStatus.Fighting;
            State.Phase = SessionPhase.Combat;
            return true;
        }
        private void PrepareOutsideStage()
        {
            if (State.OutsideStage != OutsideStageStatus.Fighting) return;
            OutsideStageAssets assets = OutsideStageAssets.Current;
            if (assets == null || assets.Boss == null) throw new InvalidOperationException("Missing outside-stage encounter");
            CombatCatalog.Boss = BuildEnemy(assets.Boss, 1 + Mathf.Max(0, State.Wave - 13) * 0.06f);
            if (ActOneKeeperReturn)
            {
                // THE KEEPER, WHOLE: it tests the traveller before it gives. Its body takes a fifth of each blow while its hand,
                // carpet and eye hold (KeeperGuard); break all three and it kneels.
                // Act I (shop 17): as strong as this run's wave-15 story boss and a little more, so it sits in the run's own curve
                float scaling = (1f + 14 * 0.10f) * JourneyBalance.EnemyHealth(State.Difficulty, 15);
                int stage = 2, index = catalog.Bosses.Length == 10 ? stage % 5 + (((State.Seed ^ stage) & 1) == 0 ? 0 : 5) : stage % catalog.Bosses.Length;
                UnitDefinition door = BuildEnemy(catalog.Bosses[index], scaling);
                CombatCatalog.Boss.Health = door.Health * WaveCadence.StoryBossBulk(15) * KeeperHealthShare; // see PreviewExports/Pacing/Keeper17.csv
                CombatCatalog.Boss.DisplayName = "THE KEEPER, WHOLE"; CombatCatalog.WholeKeeper = true;
                CombatCatalog.Boss.Weapon.Damage = door.Weapon.Damage * 1.6f; CombatCatalog.Boss.ContactDamage = door.ContactDamage * 1.6f;
            }
            else
            {
                CombatCatalog.Boss.Health = assets.Boss.Health * 20 * .8f * 1.8f * (1 + State.Difficulty * 0.25f + Mathf.Max(0, State.Wave - 13) * 0.08f) * OctoberBalance.EndlessHealth(State.Wave, State.Endless);
                CombatCatalog.Boss.Weapon.Damage *= 2.8f; CombatCatalog.Boss.ContactDamage *= 2.5f;
            }
            CombatCatalog.Boss.BodyHeight = assets.Boss.BodyHeight * 2.45f;
            CombatCatalog.Boss.Radius*=1.4f;
            CombatCatalog.Boss.KnockbackResistance = 1;
            CombatCatalog.EnemyBudget = 0; CombatCatalog.BossSwarmBudget = 0; // its guards are its swarm
            CombatCatalog.ArenaSize = new Vector2(22, 12);
            CombatCatalog.EnableArenaToys = CombatCatalog.EnableSpectacles = CombatCatalog.EnableBossArenaPhase = false;
            CombatCatalog.WaveDuration = 0;
        }
        private bool SettleOutsideStage(CombatSimulation simulation)
        {
            if (State.OutsideStage != OutsideStageStatus.Fighting) return false;
            State.Health = simulation.Champion.Health;
            State.CombatSeconds += simulation.Elapsed;
            State.LastDamageSource = simulation.LastDamageSource;
            State.LastDamageKind = simulation.LastDamageKind;
            State.LastDamageAmount = simulation.LastDamageAmount;
            ContributionLedger.Merge(State.Contributions, simulation.Contributions);
            if (simulation.Phase == RunPhase.Defeat) { State.Phase = SessionPhase.Defeat; return true; }
            if (simulation.KeeperWithdrew && !simulation.BossDefeated)
            {
                // 'Not yet.' The Keeper sets the traveller down: no gift, the run goes on (the hand does not come back)
                State.OutsideStage = OutsideStageStatus.Declined; State.Phase = SessionPhase.Shop; State.Health = MaximumHealth();
                Notice = "THE KEEPER: 'Not yet.' It sets you down gently. No gift this time."; return true;
            }
            if (!simulation.BossDefeated || simulation.Boss == null || simulation.Boss.Definition.Id != "OutsideStageKeeper") throw new InvalidOperationException("Outside-stage victory requires its boss");
            State.Health = MaximumHealth();
            State.OutsideStage = OutsideStageStatus.Reward; State.Phase = SessionPhase.Shop; State.OutsideBeforeGift = -1;
            State.OutsideRewards = RainbowChoices(0) ?? new List<string> { "RainbowCrown", "RainbowHeart", "RainbowThread" };
            return true;
        }
        /// <summary>Three Prismatic gifts not already owned, from the hero's own fixed pool (its themes: Prismatic items and
        /// Prismatic companions; an ally among them for a hero who keeps a brigade), topped up from the other Prismatic items when
        /// the pool runs dry; null when fewer than three are left anywhere. Deterministic per run and per gift.</summary>
        private List<string> RainbowChoices(int salt)
        {
            UnitDefinition hero = catalog.Unit(State.ChampionId);
            bool brigade = hero.HeroMechanic != HeroMechanic.SoloMaw;
            bool Owned(string id) => State.Items.Exists(stack => stack.Id == id) || State.Roster.Exists(entry => entry.UnitId == id);
            var items = new List<string>(); var allies = new List<string>(); var others = new List<string>();
            foreach (ItemDefinition item in catalog.Items)
                if (item.Rarity == ItemRarity.Rainbow && !Owned(item.Id)) (OctoberItemEligible(item) ? items : others).Add(item.Id);
            if (brigade)
                foreach (UnitDefinition ally in catalog.Companions)
                    if (ally.Rarity == ItemRarity.Rainbow && !ally.GiftOnly && ally.Body != null && !Owned(ally.Id) && K11HeroRules.Eligible(hero, ally)) allies.Add(ally.Id);
            var choices = new List<string>(items); choices.AddRange(allies);
            if (choices.Count + others.Count < 3) return null;
            var picked = new List<string>();
            uint seed = unchecked((uint)State.Seed ^ 0x728a53b1 ^ (uint)salt * 0x9e3779b9u);
            uint Next() { seed = unchecked(seed * 1664525 + 1013904223); return seed; }
            // the hero's own Prismatics first; the others only top up a thin pool
            foreach (var source in new[] { choices, others })
                while (picked.Count < 3 && source.Count > 0)
                {
                    int index = (int)(Next() % (uint)source.Count);
                    picked.Add(source[index]); source.RemoveAt(index);
                }
            if (allies.Count > 0 && !picked.Exists(allies.Contains))
                picked[2] = allies[(int)(Next() % (uint)allies.Count)];
            return picked;
        }
        /// <summary>Three prism shards become a rainbow gift: the shop opens on three choices (the same table as the Keeper's).
        /// The Keeper's own status is kept and comes back once the gift is taken.</summary>
        public bool OfferPrismGift(bool free = false)
        {
            if (State.Phase != SessionPhase.Shop || SecretRoomOpen || State.OutsideStage == OutsideStageStatus.Reward || State.OutsideStage == OutsideStageStatus.Fighting) return false;
            if (!free && State.PrismShards < 3) return false;
            List<string> choices = RainbowChoices(State.Wave * 7 + State.PrismShardDrops + (free ? 1000 : 0));
            if (choices == null) return false;
            if (!free) State.PrismShards -= 3;
            State.OutsideBeforeGift = (int)State.OutsideStage;
            State.OutsideStage = OutsideStageStatus.Reward; State.OutsideRewards = choices;
            Notice = "THE COLOURS: choose one rainbow gift.";
            return true;
        }
        public bool ClaimOutsideStageReward(int index)
        {
            if (State.Phase != SessionPhase.Shop || State.OutsideStage != OutsideStageStatus.Reward || State.OutsideRewards == null || State.OutsideRewards.Count != 3 || index < 0 || index >= 3) return false;
            string id = State.OutsideRewards[index];
            float previousMaximum = MaximumHealth();
            ItemDefinition item = catalog.Item(id);
            if (item != null && item.Rarity == ItemRarity.Rainbow)
            {
                ItemStack stack = State.Items.Find(candidate => candidate.Id == id);
                if (stack != null) return false;
                State.Items.Add(new ItemStack { Id = id, Count = 1 });
            }
            else
            {
                UnitDefinition ally = catalog.Unit(id);
                if (ally == null || ally.Rarity != ItemRarity.Rainbow || catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) return false;
                var entry = new RosterEntry { UnitId = id, WeaponId = ally.Weapon.Id, Tier = 2 }; // a rainbow ally arrives at 2 stars (owner 22/10/2026)
                State.Roster.Add(entry); RememberVariant(entry); OctoberPurchase(ally);
            }
            State.OutsideStage = State.OutsideBeforeGift >= 0 ? (OutsideStageStatus)State.OutsideBeforeGift : OutsideStageStatus.Completed; // a prism gift leaves the Keeper as it was
            State.OutsideBeforeGift = -1;
            RecountBonds();
            State.Health = Mathf.Min(MaximumHealth(), State.Health + Mathf.Max(0, MaximumHealth() - previousMaximum));
            State.OutsideRewards.Clear();
            return true;
        }
    }
}
