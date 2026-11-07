using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class ArenaController
    {
        public SecretEventRoom SecretRoom { get; private set; }
        public bool SecretAction(Func<bool> operation, string title)
        {
            if (PresentationPaused || TransitionPending || HallExitPending) return false;
            RunSnapshot before = MergeReveal.Capture(session.State);
            var previousStats = before.Phase == SessionPhase.Shop ? RunBuildReadModel.CaptureShopStats(session,runCatalog) : null;
            if (!operation()) return false;
            try { session.Validate(session.State); }
            catch { session.Restore(before); throw; }
            if (!PersistBoundary()) { session.Restore(before); return false; }
            if (previousStats != null && session.State.Phase == SessionPhase.Shop) session.PublishShopStatChanges(previousStats,RunBuildReadModel.CaptureShopStats(session,runCatalog));
            bool battle = session.State.Phase == SessionPhase.Combat;
            if (battle) Begin(); else Begin(true);
            CheckpointAudio.PlayInterfaceId(battle ? "ui.cutin.boss" : "ui.confirm");
            return true;
        }
        public bool EnterSecret(SecretEventKind kind)
        {
            bool ok = SecretAction(() => session.EnterSecretEvent(kind), "Enter secret room");
            if (ok) CheckpointAudio.PlayInterfaceId(SecretEventAssets.EntryCues[(int)kind]);
            return ok;
        }
        public bool ChooseSecret(int option) => SecretAction(() => session.ChooseSecretEvent(option), "Secret agreement");
        public bool ClaimSecret() => SecretAction(session.ClaimSecretBattleReward, "Secret reward");
        public bool LeaveSecret() => SecretAction(session.LeaveSecretEvent, "Return from secret room");
        public bool SecretTarget(string item, string weapon = "") => SecretAction(() => session.SetSecretTarget(item, weapon), "Select secret target");
    }
}
