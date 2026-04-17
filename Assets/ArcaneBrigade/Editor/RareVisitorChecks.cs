using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class RareVisitorChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException(name); notes.Add("PASS " + name); }
        private static RunSnapshot Copy(RunSnapshot s) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s));
        private static RunSession Fresh(RunCatalog c, int wave = 12)
        {
            var s = new RunSession(c); s.Start("LanternKnight", 221026);
            s.State.Phase = SessionPhase.Shop; s.State.Wave = wave; s.State.Gold = 2000; s.State.Roster.Clear();
            foreach (var u in c.Companions.Where(u => !RunSession.StorySpecies(u.Id) && u.ReferenceCompanion != "Evil").Take(3))
                s.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = u.Weapon.Id, Tier = 1 });
            return s;
        }
        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            var odds = new Dictionary<string, int>();
            for (int i = 0; i < RareVisitorRules.RollSize; i++)
            {
                string id = RareVisitorRules.Select(i) ?? "none"; odds[id] = odds.TryGetValue(id, out int n) ? n + 1 : 1;
            }
            Check(odds["StarMender"] == 400 && odds["ThirdBellKeeper"] == 75 && odds["UnclaimedGiftKeeper"] == 225 && odds["none"] == 9300, "Exclusive chances are exactly 4%, 0.75%, 2.25%, with 93% no rare guest");
            foreach (string id in new[] { "StarMender", "ThirdBellKeeper" })
            using (var run = Fresh(c))
            {
                var held = run.State.Roster[0]; run.State.Roster.Add(new RosterEntry { UnitId = held.UnitId, WeaponId = held.WeaponId });
                run.ForceVisitor(id); RunSnapshot before = Copy(run.State);
                string unchanged = JsonUtility.ToJson(run.State); uint rng = run.State.VisitorRandom;
                run.VisitorOptions(); run.VisitorOptions();
                Check(JsonUtility.ToJson(run.State) == unchanged && rng == run.State.VisitorRandom, id + " preview neither rolls nor mutates the gift");
                using var restored = new RunSession(c); restored.Restore(before);
                Check(RunTransaction.TryCommit(run, c, "Rare visitor gift", () => run.VisitorChoose(0), () => true, out string error), id + " commits through the real transaction: " + error);
                Check(restored.VisitorChoose(0), id + " can accept from the same saved state");
                int target = id == "StarMender" ? 2 : 3;
                Check(run.State.Roster.Count(r => r.Tier == target) == 1 && run.State.Roster.Count == before.Roster.Count, id + " upgrades exactly one existing friend without spending materials");
                Check(run.State.Roster.Select(r => r.WeaponId).SequenceEqual(before.Roster.Select(r => r.WeaponId)), id + " preserves every weapon");
                Check(run.State.Roster.Select(r => r.Tier).SequenceEqual(restored.State.Roster.Select(r => r.Tier)), id + " reload produces the same random recipient");
                Check(run.State.RegularNewSpeciesPurchased == before.RegularNewSpeciesPurchased && run.State.NewRecruitsThisWave == before.NewRecruitsThisWave && run.State.Gold == before.Gold && run.State.Eggs == before.Eggs, id + " is free and consumes no recruitment allowance");
                Check(!run.VisitorChoose(0) && run.Visitor == null, id + " can only be claimed once");
                Check(run.PrepareCombat().StartingCompanions.Length == run.ActiveRoster.Count, id + " compiles one fighter per species");
            }
            using (var run = Fresh(c))
            {
                foreach (var r in run.State.Roster) r.Tier = 3;
                run.ForceVisitor("StarMender"); Check(!run.VisitorOptions()[0].Enabled && !run.VisitorChoose(0), "No eligible friend disables the two-star gift");
                run.ForceVisitor("ThirdBellKeeper"); Check(!run.VisitorOptions()[0].Enabled && !run.VisitorChoose(0), "Already-maxed brigade disables the three-star gift");
            }
            var rarities = new HashSet<ItemRarity>();
            using (var run = Fresh(c, 20))
            {
                RunSnapshot baseline = Copy(run.State);
                for (int seed = 1; seed <= 200; seed++)
                {
                    run.Restore(baseline); run.State.VisitorRandom = unchecked((uint)seed * 2654435761u) | 1;
                    run.ForceVisitor("UnclaimedGiftKeeper");
                    Check(run.VisitorChoose(0), "Relic gift is claimable, seed " + seed);
                    ItemStack gift = run.State.Items.Single(); var item = c.Item(gift.Id); rarities.Add(item.Rarity);
                    Check(item.Rarity <= ItemRarity.Mythic && item.UnlockWave <= 20 && gift.Count <= item.StackLimit, "Relic respects rarity, unlock and stack limits, seed " + seed);
                    Check(run.State.Gold == baseline.Gold && run.State.RegularNewSpeciesPurchased == baseline.RegularNewSpeciesPurchased && run.State.NewRecruitsThisWave == baseline.NewRecruitsThisWave, "Relic gift is free without consuming recruitment quota, seed " + seed);
                    run.Validate(run.State);
                }
            }
            Check(rarities.SetEquals(new[] { ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Mythic }), "Reward sampling reaches all four allowed rarities and none above Mythic");
            var paint = RunSession.VisitorPainted; RunSession.VisitorPainted = id => true;
            var roll = typeof(RunSession).GetMethod("VisitorNewShop", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                int seen = 0;
                for (int seed = 1; seed <= 500; seed++)
                using (var run = Fresh(c))
                {
                    run.State.VisitorRandom = unchecked((uint)seed * 2654435761u) | 1;
                    roll.Invoke(run, null);
                    if (!RareVisitorRules.IsRare(run.State.Visitor) || run.State.VisitorWave != run.State.Wave) continue;
                    seen++;
                    string state = JsonUtility.ToJson(run.State);
                    roll.Invoke(run, null); Check(JsonUtility.ToJson(run.State) == state, "Re-entering the same shop never rerolls a rare visitor, seed " + seed);
                    using var loaded = new RunSession(c); loaded.Restore(Copy(run.State));
                    state = JsonUtility.ToJson(loaded.State); roll.Invoke(loaded, null);
                    Check(JsonUtility.ToJson(loaded.State) == state, "Reload preserves the visitor and its random stream, seed " + seed);
                    run.State.Wave++; roll.Invoke(run, null);
                    Check(run.State.VisitorWave != run.State.Wave, "Another rare guest cannot follow in the very next shop, seed " + seed);
                }
                Check(seen >= 20 && seen <= 55, "Seed sweep sees a rare, non-guaranteed occurrence: " + seen + "/500");
                for (int wave = 3; wave < 9; wave++)
                using (var run = Fresh(c, wave))
                {
                    // Seed range is swept on the actual roll, not the selector alone.
                    RunSnapshot baseline = Copy(run.State);
                    for (int seed = 1; seed <= 100; seed++)
                    {
                        run.Restore(baseline); run.State.VisitorRandom = unchecked((uint)seed * 2654435761u) | 1;
                        roll.Invoke(run, null);
                        Check(run.State.Visitor != "ThirdBellKeeper", "Three-star visitor never appears before shop 9: " + wave + "/" + seed);
                    }
                }
            }
            finally { RunSession.VisitorPainted = paint; }
            var art = ExpansionArtwork.Current;
            foreach (string id in new[] { "StarMender", "ThirdBellKeeper", "UnclaimedGiftKeeper" })
            {
                Sprite waiting = RareVisitorArtwork.Portrait(id, 0), offering = RareVisitorArtwork.Portrait(id, 1);
                Check(waiting != null && offering != null && waiting != offering, id + " loads its own two painted poses");
                Check(art?.Visitor(id) == waiting && art.Visitor(id, 1) == offering, id + " world station and card use the commissioned artwork");
                Check(waiting.rect.size == offering.rect.size && waiting.pivot == offering.pivot, id + " poses share scale and foot pivot");
            }
            Directory.CreateDirectory("PreviewExports/RareVisitors-20261022");
            File.WriteAllLines("PreviewExports/RareVisitors-20261022/Checks.txt", notes);
            Debug.Log("RARE_VISITOR_CHECKS_PASSED " + notes.Count);
            VisitorChecks.Run();
        }
    }
}
