using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How a gate opens a hero (requirement 27/10/2026: heroes open in many ways, and some wait for a later update).</summary>
    public enum HeroGate { Start, Wave, Boss, Win, WinWith, Difficulty, Secrets, Kills, Mastery, Daily, Endless, Soon, Act }

    public sealed class HeroRule { public HeroGate Gate; public int N; public string Id; }

    /// <summary>Which hero opens how. The table is the one place to edit: move a hero out of <see cref="HeroGate.Soon"/> to release
    /// it in an update, or change its gate. A hero missing from the table keeps the old rule (reach night UnlockWave).</summary>
    public static class HeroUnlocks
    {
        private static HeroRule R(HeroGate gate, int n = 0, string id = null) => new HeroRule { Gate = gate, N = n, Id = id };

        // requirement 3/11/2026: a few heroes to start, most open one by one through play, some held back for content updates
        // (hidden until their update ships) and some for Act II and III (hidden until that act exists)
        private static readonly Dictionary<string, HeroRule> Rules = new Dictionary<string, HeroRule>
        {
            // the roster you start with
            ["LanternKnight"] = R(HeroGate.Start), ["RootOracle"] = R(HeroGate.Start), ["GlassDice"] = R(HeroGate.Start), ["CometRunner"] = R(HeroGate.Start),
            // reach a night
            ["LoneJelly"] = R(HeroGate.Wave, 3), ["TreasureAcorn"] = R(HeroGate.Wave, 4), ["Postmaster"] = R(HeroGate.Wave, 5), ["TideSinger"] = R(HeroGate.Wave, 7),
            ["MirrorKeeper"] = R(HeroGate.Wave, 9), ["DrowsySeer"] = R(HeroGate.Wave, 10), ["VelvetRegent"] = R(HeroGate.Wave, 12), ["ThornTreasurer"] = R(HeroGate.Wave, 14),
            ["IvorySovereign"] = R(HeroGate.Wave, 16),
            // beat a boss
            ["ChinaMarionette"] = R(HeroGate.Boss, 0, "CoinWidow"), ["ButtonCavalier"] = R(HeroGate.Boss, 0, "SuitcaseLeviathan"),
            ["ArchiveCurator"] = R(HeroGate.Boss, 0, "UndeliveredKing"), ["TinVirtuoso"] = R(HeroGate.Boss, 0, "MarqueeMaw"),
            // finish things
            ["NightMaw"] = R(HeroGate.Win, 1), ["PetalCitadel"] = R(HeroGate.Difficulty, 2), ["LanternWeaver"] = R(HeroGate.WinWith, 1, "LanternKnight"),
            ["SoapSkater"] = R(HeroGate.Secrets, 1), ["PatchworkHost"] = R(HeroGate.Secrets, 4), ["CitrusMarshal"] = R(HeroGate.Daily, 1),
            ["CopperCauldron"] = R(HeroGate.Kills, 1500),
            // content updates (see UpdateOf): their gate once the update ships
            ["StormFencer"] = R(HeroGate.Mastery, 2), ["MoonProwler"] = R(HeroGate.Endless, 25), ["PennyBroker"] = R(HeroGate.Kills, 3000), ["VerdantMatron"] = R(HeroGate.Win, 2),
            // Act II and Act III (see ActOf): they join on reaching their act
            ["SupperMortar"] = R(HeroGate.Act, 2), ["HourglassStag"] = R(HeroGate.Act, 2), ["RainArchivist"] = R(HeroGate.Act, 2), ["KiteAdmiral"] = R(HeroGate.Act, 2),
            ["SundaeDuchess"] = R(HeroGate.Act, 3), ["BonsaiClocksmith"] = R(HeroGate.Act, 3), ["JugglingJester"] = R(HeroGate.Act, 3), ["BellDiver"] = R(HeroGate.Act, 3),
        };

        /// <summary>The content update the game is at (0 at release). Raise it when an update ships: the heroes of that update appear.</summary>
        public const int ContentUpdate = 0;
        /// <summary>The acts the game contains.</summary>
        public const int ActsReleased = 1;
        /// <summary>The update table: heroes held back until a content update (update number).</summary>
        private static readonly Dictionary<string, int> UpdateOf = new Dictionary<string, int>
        {
            ["StormFencer"] = 1, ["MoonProwler"] = 1, ["PennyBroker"] = 2, ["VerdantMatron"] = 2,
        };

        /// <summary>The hero is in the game today: not waiting for a content update or an act that has not shipped. Hidden heroes
        /// appear nowhere (hero select, the Wardrobe, the journal, the shop's hero relics).</summary>
        public static bool Listed(UnitDefinition hero)
        {
            if (hero == null || hero.Id == null) return false;
            if (UpdateOf.TryGetValue(hero.Id, out int update) && update > ContentUpdate) return false;
            HeroRule rule = RuleOf(hero);
            return rule.Gate != HeroGate.Act || rule.N <= ActsReleased;
        }

        public static HeroRule RuleOf(UnitDefinition hero) =>
            hero != null && hero.Id != null && Rules.TryGetValue(hero.Id, out HeroRule rule) ? rule : R(HeroGate.Wave, hero != null ? hero.UnlockWave : 0);

        public static bool Coming(UnitDefinition hero) => RuleOf(hero).Gate == HeroGate.Soon;

        private static string BossName(string id) => id switch
        {
            "CoinWidow" => "the Coin Widow", "SuitcaseLeviathan" => "the Chair Porter", "UndeliveredKing" => "the Door That Stayed Shut",
            "MarqueeMaw" => "the Marquee Maw", _ => "a boss"
        };

        private static int FoundCount(PlayerProfile profile, string prefix)
        {
            int n = 0;
            foreach (string key in MetaProgression.Ensure(profile).Found) if (key != null && key.StartsWith(prefix, System.StringComparison.Ordinal)) n++;
            return n;
        }

        private static int MasteryBest(PlayerProfile profile)
        {
            int best = 0;
            foreach (HeroMastery h in MetaProgression.Ensure(profile).Mastery) best = Mathf.Max(best, MetaProgression.MasteryRank(h.Waves));
            return best;
        }

        /// <summary>How far the profile is on a rule's count (what it has, and what it needs); (0, 0) for a rule without a count.</summary>
        public static (int have, int need) Progress(PlayerProfile profile, HeroRule rule)
        {
            switch (rule.Gate)
            {
                case HeroGate.Wave: case HeroGate.Endless: return (Mathf.Min(profile.BestWave, rule.N), rule.N);
                case HeroGate.Win: return (Mathf.Min(profile.Wins, rule.N), rule.N);
                case HeroGate.WinWith: return (Mathf.Min(MetaProgression.Mastery(profile, rule.Id)?.Wins ?? 0, rule.N), rule.N);
                case HeroGate.Secrets: return (Mathf.Min(FoundCount(profile, "secret/"), rule.N), rule.N);
                case HeroGate.Daily: return (Mathf.Min(FoundCount(profile, "daily/"), rule.N), rule.N);
                case HeroGate.Kills: return (Mathf.Min(profile.TotalKills, rule.N), rule.N);
                case HeroGate.Mastery: return (Mathf.Min(MasteryBest(profile), rule.N), rule.N);
                default: return (0, 0);
            }
        }

        public static bool Open(PlayerProfile profile, UnitDefinition hero)
        {
            if (hero == null || !Listed(hero)) return false;
            HeroRule rule = RuleOf(hero);
            switch (rule.Gate)
            {
                case HeroGate.Start: return true;
                case HeroGate.Soon: return false;
                case HeroGate.Act: return rule.N <= ActsReleased && profile.Wins >= rule.N - 1; // a win of the act before
                case HeroGate.Boss: return profile.SeenBosses.Contains(rule.Id);
                case HeroGate.Difficulty: return (MetaProgression.Ensure(profile).WonMask & (1 << Mathf.Clamp(rule.N, 0, 30))) != 0;
                default: (int have, int need) = Progress(profile, rule); return have >= need;
            }
        }

        /// <summary>A short chip for the hero card ("NIGHT 5", "BEAT A BOSS", "COMING SOON").</summary>
        public static string Short(UnitDefinition hero)
        {
            HeroRule rule = RuleOf(hero);
            switch (rule.Gate)
            {
                case HeroGate.Wave: return "NIGHT " + rule.N;
                case HeroGate.Boss: return "BEAT A BOSS";
                case HeroGate.Win: return "WIN A RUN";
                case HeroGate.WinWith: return "WIN WITH A HERO";
                case HeroGate.Difficulty: return "WIN ON " + RunMeta.DifficultyNames[Mathf.Clamp(rule.N, 0, RunMeta.DifficultyNames.Length - 1)].ToUpperInvariant();
                case HeroGate.Secrets: return "SECRET ROOMS";
                case HeroGate.Kills: return rule.N.ToString("N0") + " KILLS";
                case HeroGate.Mastery: return "HERO MASTERY";
                case HeroGate.Daily: return "DAILY DREAM";
                case HeroGate.Endless: return "ENDLESS " + rule.N;
                case HeroGate.Soon: return "COMING SOON";
                case HeroGate.Act: return "ACT " + (rule.N == 2 ? "II" : "III");
                default: return "";
            }
        }

        /// <summary>The whole condition in a sentence, with the count so far; empty when the hero is open.</summary>
        public static string Full(PlayerProfile profile, UnitDefinition hero, RunCatalog catalog, bool withCount = true)
        {
            if (Open(profile, hero)) return "";
            HeroRule rule = RuleOf(hero);
            (int have, int need) = Progress(profile, rule);
            string count = withCount && need > 1 ? " (" + have.ToString("N0") + "/" + need.ToString("N0") + ")" : "";
            switch (rule.Gate)
            {
                case HeroGate.Wave: return "Reach night " + rule.N + count;
                case HeroGate.Boss: return "Defeat " + BossName(rule.Id);
                case HeroGate.Win: return "Win a run" + count;
                case HeroGate.WinWith: return "Win with " + (catalog?.Unit(rule.Id) != null ? RunUICopy.UnitName(catalog.Unit(rule.Id)) : "another hero") + count;
                case HeroGate.Difficulty: return "Win on " + RunMeta.DifficultyNames[Mathf.Clamp(rule.N, 0, RunMeta.DifficultyNames.Length - 1)];
                case HeroGate.Secrets: return "Find " + rule.N + " secret room" + (rule.N > 1 ? "s" : "") + count;
                case HeroGate.Kills: return "Defeat " + rule.N.ToString("N0") + " enemies" + count;
                case HeroGate.Mastery: return "Reach mastery " + rule.N + " with a hero";
                case HeroGate.Daily: return "Finish a Daily Dream";
                case HeroGate.Endless: return "Reach night " + rule.N + " in Endless" + count;
                case HeroGate.Soon: return "Arrives in a later update";
                case HeroGate.Act: return "Reach Act " + (rule.N == 2 ? "II" : "III");
                default: return "";
            }
        }

        /// <summary>How far the player is, in words (release testing 3/11/2026: a locked hero showed only "NIGHT 5" and a small
        /// line; tapping it now says what is needed and how far along the player is). Empty for an open hero.</summary>
        public static string ProgressLine(PlayerProfile profile, UnitDefinition hero, RunCatalog catalog)
        {
            if (Open(profile, hero)) return "";
            HeroRule rule = RuleOf(hero);
            (int have, int need) = Progress(profile, rule);
            switch (rule.Gate)
            {
                case HeroGate.Wave: return "Your best night: " + have + " of " + need;
                case HeroGate.Endless: return "Your best Endless night: " + have + " of " + need;
                case HeroGate.Win: return "Runs won: " + have + " of " + need;
                case HeroGate.WinWith: return "Wins with " + (catalog?.Unit(rule.Id) != null ? RunUICopy.UnitName(catalog.Unit(rule.Id)) : "that hero") + ": " + have + " of " + need;
                case HeroGate.Secrets: return "Secret rooms found: " + have + " of " + need;
                case HeroGate.Daily: return "Daily Dreams finished: " + have + " of " + need;
                case HeroGate.Kills: return "Enemies defeated: " + have.ToString("N0") + " of " + need.ToString("N0");
                case HeroGate.Mastery: return "Best hero mastery: " + have + " of " + need;
                case HeroGate.Boss: return "Not defeated yet";
                case HeroGate.Difficulty: return "Not won on " + RunMeta.DifficultyNames[Mathf.Clamp(rule.N, 0, RunMeta.DifficultyNames.Length - 1)] + " yet";
                default: return "";
            }
        }
        /// <summary>The share of the way to unlocking (0..1); -1 for a gate without a count (a boss, a difficulty, coming soon).</summary>
        public static float Share(PlayerProfile profile, UnitDefinition hero)
        {
            (int have, int need) = Progress(profile, RuleOf(hero));
            return need > 0 ? Mathf.Clamp01(have / (float)need) : -1;
        }

        // ------------------------------------------------------------------ Endless milestones and the endgame relics
        /// <summary>Endless nights that pay Keepsakes once each (night, Keepsakes).</summary>
        public static readonly (int night, int keepsakes)[] EndlessMilestones = { (25, 3), (30, 3), (40, 4), (50, 5), (75, 6), (100, 8) };

        /// <summary>How many Endless milestones the profile has reached (0 to 6).</summary>
        public static int EndgameTier(PlayerProfile profile) => FoundCount(profile, "endless/");

        /// <summary>Relics that join the shops only after an Endless milestone: the tier (1 = night 25, 3 = night 40) a relic needs, or 0.</summary>
        public static int EndgameNeeds(string itemId)
        {
            switch (itemId)
            {
                case "CurioVoidDoubloon": case "CurioSixfoldRoot": case "CurioWanderLetter": case "CurioLastHopeMotor": return 1;
                case "CurioEmeraldInvoice": case "CurioOathParcel": case "CurioFeastBucket": case "CurioGambleSandwich": return 3;
                default: return 0;
            }
        }
    }
}
