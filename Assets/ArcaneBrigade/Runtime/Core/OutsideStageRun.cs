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
            if (snapshot.OutsideStage != OutsideStageStatus.Reward) return;
            if (snapshot.Phase != SessionPhase.Shop || snapshot.OutsideRewards == null || snapshot.OutsideRewards.Count != 3 || new HashSet<string>(snapshot.OutsideRewards).Count != 3) throw new InvalidOperationException("Invalid prismatic choices");
            foreach (string id in snapshot.OutsideRewards)
            {
                ItemDefinition item = catalog.Item(id); UnitDefinition ally = catalog.Unit(id);
                if (item != null && item.Rarity == ItemRarity.Rainbow) continue;
                if (ally == null || ally.Rarity != ItemRarity.Rainbow || catalog.Unit(snapshot.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) throw new InvalidOperationException("Invalid prismatic reward");
            }
        }
        public bool OutsideChallengeAvailable => !SecretRoomOpen && State.Phase == SessionPhase.Shop && State.OutsideStage == OutsideStageStatus.Unseen && (State.Endless || State.Wave > ActOneCanon.Waves) && State.OutsideTransfers >= 3 && OutsideStageAssets.Current != null;
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
            CombatCatalog.Boss.Health = assets.Boss.Health * 20 * .8f * 1.8f * (1 + State.Difficulty * 0.25f + Mathf.Max(0, State.Wave - 13) * 0.08f) * OctoberBalance.EndlessHealth(State.Wave, State.Endless);
            CombatCatalog.Boss.BodyHeight = assets.Boss.BodyHeight * 2.45f;
            CombatCatalog.Boss.Radius*=1.4f;
            CombatCatalog.Boss.Weapon.Damage *= 2.8f; CombatCatalog.Boss.ContactDamage *= 2.5f;
            CombatCatalog.Boss.KnockbackResistance = 1;
            CombatCatalog.EnemyBudget = 0;
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
            if (!simulation.BossDefeated || simulation.Boss == null || simulation.Boss.Definition.Id != "OutsideStageKeeper") throw new InvalidOperationException("Outside-stage victory requires its boss");
            State.Health = MaximumHealth();
            State.OutsideStage = OutsideStageStatus.Reward; State.Phase = SessionPhase.Shop;
            var choices = new List<string> { "RainbowCrown", "RainbowHeart", "RainbowThread", "RainbowPrism" };
            if (catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw)
            {
                choices.Insert(0, "RainbowTailor"); choices.Insert(1, "RainbowBell");
            }
            State.OutsideRewards = new List<string>();
            uint seed = unchecked((uint)State.Seed ^ 0x728a53b1);
            while (State.OutsideRewards.Count < 3)
            {
                seed = unchecked(seed * 1664525 + 1013904223);
                int index = (int)(seed % (uint)choices.Count);
                State.OutsideRewards.Add(choices[index]); choices.RemoveAt(index);
            }
            if (catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw && !State.OutsideRewards.Exists(id => id == "RainbowTailor" || id == "RainbowBell")) State.OutsideRewards[2] = "RainbowTailor";
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
                var entry = new RosterEntry { UnitId = id, WeaponId = ally.Weapon.Id, Tier = 1 };
                State.Roster.Add(entry); RememberVariant(entry); OctoberPurchase(ally);
            }
            State.OutsideStage = OutsideStageStatus.Completed;
            RecountBonds();
            State.Health = Mathf.Min(MaximumHealth(), State.Health + Mathf.Max(0, MaximumHealth() - previousMaximum));
            State.OutsideRewards.Clear();
            return true;
        }
    }
}
