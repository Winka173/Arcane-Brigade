using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Act I progress that is not journal text: the warm chair's hidden fight and the lanterns it relit.</summary>
    [Serializable]
    public sealed class ActOneProgress
    {
        public const int None = 0, Fighting = 1, Won = 2, Lost = 3;
        public int Revision = 1;
        public int SitCloser;   // the warm chair: None, Fighting, Won (stood up), Lost (sat down: the run ended)
        public int SatWave;     // the shop where the traveller sat down
        public int RelitMask;   // lanterns burning again, by canon lamp index (0..7)
    }
    public sealed partial class RunSnapshot { public ActOneProgress ActOne = new ActOneProgress(); }
    public sealed partial class CheckpointCatalog
    {
        public UnitDefinition[] StoryCompanions = new UnitDefinition[0];
        public float KnockDeadline; // SIT CLOSER: the Bedside must fall before the knocking stops
    }

    public sealed partial class RunSession
    {
        /// <summary>Hob, Latch, Postscript and Tally travel in their own places: they are not roster bodies, so they are
        /// never bought, merged, sold, counted toward quotas or bonds. Their strength follows the zone.</summary>
        private void PrepareStoryParty()
        {
            var party = new System.Collections.Generic.List<UnitDefinition>();
            bool shop = State.Phase == SessionPhase.Shop;
            for (int index = 0; index < ActOneCanon.StoryIds.Length; index++)
            {
                if (!ActOneCanon.StoryPresent(index, State.Wave, shop, State.Endless)) continue;
                UnitDefinition source = catalog.Unit(ActOneCanon.StoryIds[index]);
                if (source == null || source.Weapon == null) continue;
                UnitDefinition unit = BuildUnit(source, source.Weapon, ActOneCanon.StoryTier(State.Wave), true);
                unit.DisplayName = ActOneCanon.StoryNames[index];
                party.Add(unit);
            }
            CombatCatalog.StoryCompanions = party.ToArray();
        }

        public bool SitCloserAvailable => !State.Endless && State.Phase == SessionPhase.Shop && !SecretRoomOpen
            && State.Wave >= ActOneCanon.SitFrom && State.Wave <= ActOneCanon.SitUntil
            && (State.ActOne?.SitCloser ?? 0) == ActOneProgress.None && State.OutsideStage != OutsideStageStatus.Fighting && State.OutsideStage != OutsideStageStatus.Reward;

        /// <summary>The traveller sits in the warm chair. The fight happens on this same shop visit; the next wave is not used.</summary>
        public bool AcceptSitCloser()
        {
            if (!SitCloserAvailable) return false;
            State.ActOne ??= new ActOneProgress();
            State.ActOne.SitCloser = ActOneProgress.Fighting;
            State.ActOne.SatWave = State.Wave;
            State.Phase = SessionPhase.Combat;
            return true;
        }

        /// <summary>THE NIGHT YOU HID: the Bedside and the seated travellers it wakes are as strong as a wave eight further on,
        /// and the knocking at the door is the clock.</summary>
        private void PrepareSitCloser()
        {
            if ((State.ActOne?.SitCloser ?? 0) != ActOneProgress.Fighting) return;
            int real = State.Wave;
            State.Wave = Mathf.Min(real + ActOneCanon.SitWaveBonus, 40);
            try
            {
                float scaling = (1f + (State.Wave - 1) * 0.10f) * JourneyBalance.EnemyHealth(State.Difficulty, State.Wave);
                UnitDefinition body = Array.Find(catalog.Bosses, boss => boss != null && boss.Id == "UmbraCradle") ?? catalog.Bosses[0];
                CombatCatalog.Boss = BuildEnemy(body, scaling);
                CombatCatalog.Boss.DisplayName = "THE BEDSIDE";
                CombatCatalog.MeleeEnemy = BuildEnemy(catalog.Enemies[(State.Wave - 1) % catalog.Enemies.Length], scaling);
                CombatCatalog.RangedEnemy = BuildEnemy(catalog.Enemies[State.Wave % catalog.Enemies.Length], scaling);
            }
            finally { State.Wave = real; }
            CombatCatalog.EnemyBudget = 140;
            CombatCatalog.WaveDuration = ActOneCanon.KnockSeconds;
            CombatCatalog.KnockDeadline = ActOneCanon.KnockSeconds;
            CombatCatalog.ArenaSize = new Vector2(24, 13);
            CombatCatalog.EnableArenaToys = CombatCatalog.EnableSpectacles = CombatCatalog.EnableBossArenaPhase = false;
        }

        private bool SettleSitCloser(CombatSimulation simulation)
        {
            if ((State.ActOne?.SitCloser ?? 0) != ActOneProgress.Fighting) return false;
            State.Health = simulation.Champion.Health;
            State.CombatSeconds += simulation.Elapsed;
            State.LastDamageSource = simulation.LastDamageSource;
            State.LastDamageKind = simulation.LastDamageKind;
            State.LastDamageAmount = simulation.LastDamageAmount;
            ContributionLedger.Merge(State.Contributions, simulation.Contributions);
            if (simulation.Phase == RunPhase.Defeat) { State.ActOne.SitCloser = ActOneProgress.Lost; State.Phase = SessionPhase.Defeat; return true; }
            if (!simulation.BossDefeated) throw new InvalidOperationException("Standing up from the warm chair requires the Bedside to fall");
            State.ActOne.SitCloser = ActOneProgress.Won;
            int lamp = ActOneCanon.RelightTarget(State.Wave, State.ActOne.RelitMask);
            if (lamp >= 0) State.ActOne.RelitMask |= 1 << lamp;
            State.Gold = BoundedCounter((long)State.Gold + simulation.Gold);
            State.Health = MaximumHealth();
            State.Phase = SessionPhase.Shop;
            return true;
        }

        public static void ValidateActOne(RunSnapshot state)
        {
            ActOneProgress progress = state?.ActOne;
            if (progress == null) return;
            if (progress.Revision != 1 || progress.SitCloser < ActOneProgress.None || progress.SitCloser > ActOneProgress.Lost || progress.RelitMask < 0 || progress.RelitMask > 0x7f)
                throw new InvalidOperationException("Invalid Act I progress");
            if (progress.SitCloser == ActOneProgress.None ? progress.SatWave != 0 : progress.SatWave < ActOneCanon.SitFrom || progress.SatWave > ActOneCanon.SitUntil || progress.SatWave > state.Wave)
                throw new InvalidOperationException("Invalid warm chair visit");
            if (progress.SitCloser == ActOneProgress.Fighting && (state.Phase != SessionPhase.Combat || state.Wave != progress.SatWave))
                throw new InvalidOperationException("The warm chair fight belongs to its own shop visit");
            if (progress.SitCloser == ActOneProgress.Lost && state.Phase != SessionPhase.Defeat)
                throw new InvalidOperationException("Sitting down ends the journey");
            for (int lamp = 0; lamp < 8; lamp++)
                if ((progress.RelitMask & (1 << lamp)) != 0 && state.Wave < ActOneCanon.BreakWave[lamp]) throw new InvalidOperationException("A lantern cannot be relit before it breaks");
        }
    }
}
