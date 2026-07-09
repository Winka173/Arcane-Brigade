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
            Require(Mathf.Approximately(run.StarProgress(friend.Id), 3f), "a 3-star friend shows no half star");
            Debug.Log("STARCHECK ok: 1 -> 1.5 -> merge at three; 2 -> 2.5; 3 stays 3");
        }
    }
}
