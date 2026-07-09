using System.Linq;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>How the heroes open (batch: -executeMethod ArcaneBrigade.Editor.HeroUnlockCheck.Run): a new profile, then a profile
    /// that has done a little of everything.</summary>
    public static class HeroUnlockCheck
    {
        private static void Require(bool ok, string what) { if (!ok) throw new System.InvalidOperationException("Hero unlocks: " + what); }

        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var log = new StringBuilder();
            var fresh = new PlayerProfile(); MetaProgression.Ensure(fresh);
            int start = catalog.Champions.Count(h => HeroUnlocks.Open(fresh, h)), soon = catalog.Champions.Count(HeroUnlocks.Coming);
            log.AppendLine("HEROCHECK new profile: " + start + " of " + catalog.Champions.Length + " heroes open, " + soon + " coming soon");
            foreach (UnitDefinition h in catalog.Champions.Where(h => !HeroUnlocks.Open(fresh, h)))
                log.AppendLine("HEROCHECK  locked " + h.Id + " | " + HeroUnlocks.Short(h) + " | " + HeroUnlocks.Full(fresh, h, catalog));
            Require(start >= 6 && start <= 10, "a new profile starts with 6 to 10 heroes");
            Require(soon >= 1 && catalog.Champions.Where(HeroUnlocks.Coming).All(h => !HeroUnlocks.Open(fresh, h)), "coming-soon heroes are shut");

            // a profile that has done a little of everything opens everything except the coming-soon heroes
            var done = new PlayerProfile(); MetaProgress m = MetaProgression.Ensure(done);
            done.BestWave = 60; done.Wins = 3; done.TotalKills = 99999;
            foreach (string boss in new[] { "CoinWidow", "SuitcaseLeviathan", "UndeliveredKing", "MarqueeMaw" }) done.SeenBosses.Add(boss);
            m.WonMask = 1 << 2; m.Found.AddRange(new[] { "secret/0", "secret/1", "secret/2", "secret/3", "daily/2026-10-27" });
            MetaProgression.Mastery(done, "LanternKnight", true).Waves = 25; MetaProgression.Mastery(done, "LanternKnight", true).Wins = 1;
            int open = catalog.Champions.Count(h => HeroUnlocks.Open(done, h));
            log.AppendLine("HEROCHECK full profile: " + open + " of " + catalog.Champions.Length + " heroes open");
            Require(open == catalog.Champions.Length - soon, "everything but the coming-soon heroes opens");
            Require(HeroUnlocks.EndgameTier(done) == 0 && HeroUnlocks.EndgameNeeds("CurioVoidDoubloon") == 1 && HeroUnlocks.EndgameNeeds("CurioFeastBucket") == 3 && HeroUnlocks.EndgameNeeds("TrophyShelf") == 0, "endgame relic tiers");

            // an Endless run to night 41 pays 25, 30 and 40 once, then nothing more
            var run = new RunSnapshot { RunId = "t", Wave = 41, Endless = true, Phase = SessionPhase.Defeat, ChampionId = "LanternKnight", Meta = new MetaRunState() };
            var (oil, keepsakes, lines, found, duets) = MetaProgression.Haul(fresh, run, catalog);
            log.AppendLine("HEROCHECK endless night 41 pays " + keepsakes + " keepsakes, found " + string.Join(",", found.Where(f => f.StartsWith("endless/"))));
            Require(found.Count(f => f.StartsWith("endless/")) == 3, "night 41 pays three milestones");
            foreach (string f in found) m = MetaProgression.Ensure(fresh);
            fresh.Meta.Found.AddRange(found);
            var again = MetaProgression.Haul(fresh, run, catalog);
            Require(again.found.Count(f => f.StartsWith("endless/")) == 0 && HeroUnlocks.EndgameTier(fresh) == 3, "milestones pay once; tier 3");
            Debug.Log(log.ToString());
        }
    }
}
