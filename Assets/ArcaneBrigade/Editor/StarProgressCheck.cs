using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Half stars: three copies make a star, two of three show half a star more and hit a little harder (batch:
    /// -executeMethod ArcaneBrigade.Editor.StarProgressCheck.Run).</summary>
    public static class StarProgressCheck
    {
        private static void Require(bool ok, string what) { if (!ok) throw new System.InvalidOperationException("Stars: " + what); }

        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using var run = new RunSession(catalog);
            run.Start("LanternKnight", 2710);
            UnitDefinition friend = catalog.Companions.First(u => !u.SpecialRecruit && u.ReferenceCompanion != "Evil");
            string weapon = catalog.Weapons.First().Id;
            run.State.Roster.Clear();
            Require(CompanionDeployment.MergeCopies == 3, "three copies merge");
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 1 });
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 1f) && !run.HalfStar(friend.Id, 1), "one copy: 1 star");
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 1 });
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 1.5f) && run.HalfStar(friend.Id, 1), "two copies: 1.5 stars");
            Require(RunSession.StarText(1.5f) == "1.5" && RunSession.StarText(2f) == "2", "star text");
            // two copies of a 2-star friend: 2.5 stars; a 3-star friend never shows a half
            run.State.Roster.Clear();
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 2 });
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 2 });
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 2.5f), "two 2-star copies: 2.5 stars");
            run.State.Roster.Clear();
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 3 });
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 3 });
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 3.5f) && run.HalfStar(friend.Id, 3), "two 3-star copies: 3.5 stars");
            // three 3-star copies make 4 stars (purple); 4 stars is where the shop stops
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 3 });
            run.Rescan();
            Require(run.State.Roster.Count == 1 && run.State.Roster[0].Tier == 4, "three 3-star copies merge into 4 stars");
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 4f), "4 stars shows no half star");
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 4 });
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 4 });
            run.Rescan();
            Require(run.State.Roster.TrueForAll(e => e.Tier == 4), "the shop never makes a 5th star");
            // a Rainbow Thread weaves a 4-star friend to 5 (a thread with no 4-star friend waits)
            run.State.Roster.Clear(); run.State.RainbowThreads = 0;
            run.State.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = weapon, Tier = 3 });
            run.GiveRainbowThread();
            Require(run.State.RainbowThreads == 1 && run.State.Roster[0].Tier == 3, "a thread waits for a 4-star friend");
            run.State.Roster[0].Tier = 4; run.Rescan();
            Require(run.State.RainbowThreads == 0 && run.State.Roster[0].Tier == 5, "the waiting thread weaves the 4-star friend to 5");
            Require(RunSession.StarSurplus(5) > RunSession.StarSurplus(4) && RunSession.StarSurplus(4) > 1f, "each star past 3 hits harder");
            Debug.Log("STARCHECK ok: 1 -> 1.5 -> merge at three; 2 -> 2.5; 3 -> 3.5 -> 4 (purple); 5 only by a Rainbow Thread");
        }
    }
}
