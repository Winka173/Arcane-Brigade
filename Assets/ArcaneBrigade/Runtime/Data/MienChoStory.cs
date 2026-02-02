using System;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class MienChoMemory
    {
        public const int CurrentRevision = 2;
        public int Revision = CurrentRevision;
        public int ReadMask;
        public int FinishedMask;
        public int EncounterMask;
        public int[] Pages = new int[MienChoStory.Chapters.Length];
        public int[] Choices = new int[MienChoStory.Chapters.Length]; // 0 undecided, 1 or 2: the option taken
    }
    public sealed partial class RunSnapshot { public MienChoMemory MienCho = new MienChoMemory(); }

    public sealed class WaitingChapter
    {
        public readonly int Wave;
        public readonly string Title, Evidence, Image;
        public readonly string[] Pages;
        public bool Shop = true; // false: opens at the start of its wave instead of in the shop after it
        public WaitingChapter(int wave, string title, string evidence, string image, params string[] pages)
        { Wave = wave; Title = title; Evidence = evidence; Image = image; Pages = pages; }
    }

    public static class MienChoStory
    {
        // Act I canon lives in ActOneCanon; the journal keeps one entry per story beat.
        public static readonly WaitingChapter[] Chapters = ActOneCanon.Beats();
        public static WaitingChapter[] For(RunSnapshot state) => Chapters;
        public static int Available(RunSnapshot state)
        {
            int count = 0;
            foreach (var chapter in For(state))
                if (state.Wave > chapter.Wave || state.Wave == chapter.Wave && (!chapter.Shop || state.Phase == SessionPhase.Shop || state.Phase == SessionPhase.Victory)) count++;
            return count;
        }
        public static int NextUnread(RunSnapshot state)
        {
            if (state.Endless) return -1;
            for (int i = 0; i < Available(state); i++) if (((state.MienCho?.ReadMask ?? 0) & (1 << i)) == 0) return i;
            return -1;
        }
        /// <summary>Saves from the first Act I canon (revision 1, ten chapters) keep their place: every beat that is
        /// already behind the traveller counts as read, so only the current beat opens.</summary>
        public static void Migrate(RunSnapshot state)
        {
            var memory = state?.MienCho;
            if (memory == null || memory.Revision == MienChoMemory.CurrentRevision) return;
            int available = Available(state);
            int behind = Math.Max(0, available - 1);
            memory.Revision = MienChoMemory.CurrentRevision;
            memory.ReadMask = memory.FinishedMask = behind == 0 ? 0 : (1 << behind) - 1;
            memory.EncounterMask = 0;
            memory.Pages = new int[Chapters.Length];
            memory.Choices = new int[Chapters.Length];
        }
        public static int Choice(RunSnapshot state, int chapter)
            => state?.MienCho?.Choices != null && chapter >= 0 && chapter < state.MienCho.Choices.Length ? state.MienCho.Choices[chapter] : 0;
        /// <summary>Pages of a choice chapter: page 0, then the branch taken, then the shared pages after both branches.</summary>
        public static int NextPage(int chapter, int page, int choice)
        {
            if (ActOneCanon.ChoiceOptions[chapter] == null) return page + 1;
            if (page == 0) return choice == 0 ? -1 : choice;
            return page <= 2 ? 3 : page + 1;
        }
        public static int PreviousPage(int chapter, int page, int choice)
        {
            if (ActOneCanon.ChoiceOptions[chapter] == null) return Math.Max(0, page - 1);
            if (page >= 3) return page == 3 ? Math.Max(1, choice) : page - 1;
            return 0;
        }

        public static void Validate(RunSnapshot state)
        {
            WaitingCampaign.Validate(state);
            Migrate(state);
            if (state.MienCho == null) return;
            if (state.MienCho.Revision != MienChoMemory.CurrentRevision || state.MienCho.ReadMask < 0 || (state.MienCho.ReadMask >> Available(state)) != 0) throw new InvalidOperationException("Invalid Waiting Realm journal");
            var memory = state.MienCho;
            if (memory.FinishedMask < 0 || (memory.FinishedMask >> Available(state)) != 0 || memory.EncounterMask < 0 || memory.EncounterMask > 7) throw new InvalidOperationException("Invalid Waiting Realm progress");
            int knownEncounters = state.Wave >= 10 ? 7 : state.Wave >= 6 ? 3 : state.Wave >= 4 ? 1 : 0;
            if ((memory.EncounterMask & ~knownEncounters) != 0) throw new InvalidOperationException("Future Waiting Realm encounter");
            if (memory.Choices != null && memory.Choices.Length != 0)
            {
                if (memory.Choices.Length != Chapters.Length) throw new InvalidOperationException("Invalid journal choice count");
                for (int i = 0; i < memory.Choices.Length; i++)
                    if (memory.Choices[i] < 0 || memory.Choices[i] > 2 || memory.Choices[i] != 0 && (ActOneCanon.ChoiceOptions[i] == null || i >= Available(state))) throw new InvalidOperationException("Invalid journal choice");
            }
            if (memory.Pages == null || memory.Pages.Length == 0) return; // Unity serializes a legacy null array as [].
            if (memory.Pages.Length != Chapters.Length) throw new InvalidOperationException("Invalid journal page count");
            for (int i = 0; i < memory.Pages.Length; i++)
                if (memory.Pages[i] < 0 || memory.Pages[i] >= For(state)[i].Pages.Length || i >= Available(state) && memory.Pages[i] != 0) throw new InvalidOperationException("Invalid journal reading position");
        }
    }
}
