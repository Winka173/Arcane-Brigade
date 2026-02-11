using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        public bool EndingPending => State.Nightmare?.Ending == NightmareEndingStage.Dawn;
        public bool EndingChoicePending => EndingPending && State.Nightmare.NarrativeRevision > 0 && State.Nightmare.EndingPage == 0 && State.Nightmare.Farewell == NightmareFarewell.Unchosen;
        public bool ChooseFarewell(NightmareFarewell choice)
        {
            if (!EndingChoicePending || State.Phase != SessionPhase.Victory || choice != NightmareFarewell.CloseShop && choice != NightmareFarewell.LeaveLight) return false;
            State.Nightmare.Farewell = choice;
            return true;
        }
        private void RememberNightmareChoice(SecretEventKind kind, int option, bool resolved)
        {
            State.Nightmare ??= new NightmareProgress();
            State.Nightmare.Choices[(int)kind] = option;
            State.Nightmare.ChoiceMask |= 1 << (int)kind;
            if (resolved) State.Nightmare.ResolvedMask |= 1 << (int)kind;
        }
        private void PrepareNightmare()
        {
            State.Nightmare ??= new NightmareProgress();
            State.Nightmare.Depth = NightmareRules.Depth(State.Wave);
            NightmareAssets assets = NightmareAssets.Current;
            if (assets == null || assets.Customer == null || !NightmareRules.FinalHall(State,catalog)) return;
            State.Nightmare.NarrativeRevision = 1;
            State.Nightmare.Ending = NightmareEndingStage.Customer;
            CombatCatalog.Boss = BuildEnemy(assets.Customer,1 + State.Difficulty * .2f);
            CombatCatalog.Boss.Health = assets.Customer.Health * .8f * (1 + State.Difficulty * .2f) * WaveCadence.StoryBossBulk(20);
            CombatCatalog.EnemyBudget = 0;
            CombatCatalog.BossSwarmBudget = 42; // the waiting keep coming through the whole duel
            CombatCatalog.FinalDuel = true;     // and stand up in walls at 70% and 35%
            CombatCatalog.EnableArenaToys = CombatCatalog.EnableSpectacles = CombatCatalog.EnableBossArenaPhase = false;
            CombatCatalog.ExpandedContent = true;
            CombatCatalog.ArenaSize = new Vector2(28,20);
            CombatCatalog.WaveDuration = 0;
        }
        private void ValidateNightmare(RunSnapshot snapshot)
        {
            MienChoStory.Validate(snapshot);
            NightmareProgress progress = snapshot?.Nightmare;
            if (progress == null) return;
            int finalPage = progress.Farewell == NightmareFarewell.LeaveLight ? 5 : 4;
            if (progress.NarrativeRevision < 0 || progress.NarrativeRevision > 1 || !Enum.IsDefined(typeof(NightmareFarewell), progress.Farewell) || progress.NarrativeRevision == 0 && progress.Farewell != NightmareFarewell.Unchosen) throw new InvalidOperationException("Invalid farewell revision");
            if (progress.Farewell != NightmareFarewell.Unchosen && (!progress.CustomerDefeated || snapshot.Phase != SessionPhase.Victory && !snapshot.Endless)) throw new InvalidOperationException("Farewell requires actual victory");
            if (progress.NarrativeRevision > 0 && progress.Ending == NightmareEndingStage.Dawn && progress.EndingPage > 0 && progress.Farewell == NightmareFarewell.Unchosen) throw new InvalidOperationException("Farewell must be chosen before dawn pages");
            if (progress.NarrativeRevision > 0 && progress.Ending == NightmareEndingStage.Complete && progress.Farewell == NightmareFarewell.Unchosen || progress.Farewell != NightmareFarewell.Unchosen && progress.Ending != NightmareEndingStage.Dawn && progress.Ending != NightmareEndingStage.Complete) throw new InvalidOperationException("Inconsistent farewell stage");
            if (progress.Depth < 0 || progress.Depth > 4 || !Enum.IsDefined(typeof(NightmareEndingStage),progress.Ending) || progress.EndingPage < 0 || progress.EndingPage > finalPage || progress.LastWhisperWave < 0 || progress.LastWhisperWave > snapshot.Wave || progress.WhisperCursor < 0) throw new InvalidOperationException("Invalid nightmare journal");
            if (progress.ChoiceMask < 0 || progress.ChoiceMask > 255 || progress.ResolvedMask < 0 || (progress.ResolvedMask & ~progress.ChoiceMask) != 0 || progress.Choices == null || progress.Choices.Length != 8 || Array.Exists(progress.Choices,choice => choice < -1 || choice > 2)) throw new InvalidOperationException("Invalid nightmare memory");
            for (int index = 0; index < progress.Choices.Length; index++)
                if (((progress.ChoiceMask & (1 << index)) != 0) != (progress.Choices[index] >= 0)) throw new InvalidOperationException("Inconsistent nightmare choice memory");
            if (progress.Ending == NightmareEndingStage.Dawn && progress.EndingPage >= finalPage || progress.Ending == NightmareEndingStage.Complete && progress.EndingPage != finalPage) throw new InvalidOperationException("Invalid ending page boundary");
            if ((progress.Ending == NightmareEndingStage.Dawn || progress.Ending == NightmareEndingStage.Complete) && !progress.CustomerDefeated) throw new InvalidOperationException("Dawn requires defeating the Last Customer");
            if (progress.Ending == NightmareEndingStage.Dawn && snapshot.Phase != SessionPhase.Victory) throw new InvalidOperationException("Invalid ending phase");
        }
        public bool AdvanceEnding()
        {
            if (!EndingPending || State.Phase != SessionPhase.Victory || EndingChoicePending) return false;
            int finalPage = State.Nightmare.Farewell == NightmareFarewell.LeaveLight ? 5 : 4;
            State.Nightmare.EndingPage = Mathf.Min(finalPage,State.Nightmare.EndingPage + 1);
            if (State.Nightmare.EndingPage == finalPage) State.Nightmare.Ending = NightmareEndingStage.Complete;
            return true;
        }
    }
}
