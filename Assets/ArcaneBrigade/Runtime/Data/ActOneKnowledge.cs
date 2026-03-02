using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Facts already discovered, shared by every narrative surface. Unlocking a wave is not reading its reveal.</summary>
    public static class ActOneKnowledge
    {
        public static bool Known(RunSnapshot state,int wave)
        {
            if (state == null) return false;
            if (wave == 20) return state.Nightmare?.CustomerDefeated == true;
            int finished=state.MienCho?.FinishedMask ?? 0;
            for (int i=MienChoStory.Available(state)-1;i>=0;i--)
                if (MienChoStory.Chapters[i].Wave==wave && MienChoStory.Chapters[i].Shop) return (finished & (1<<i))!=0;
            return false;
        }
        public static bool HobRescued(RunSnapshot state) => (state?.ActOne?.HobRescued ?? 0)>0;
        public static bool HasCompany(RunSnapshot state)
            => state != null && (state.Roster.Count>0 || ActOneCanon.StoryCount(state.Wave,state.Phase==SessionPhase.Shop,state.Endless)>0 || state.Secrets?.RescuedToy==true);
    }
}
