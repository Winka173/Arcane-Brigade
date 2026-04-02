using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8UIChecks
    {
        [MenuItem("Arcane Brigade/Checks/K8 UI")]
        public static void Run()
        {
            int passed = 0;
            Action<bool, string> require = (condition, caption) =>
            {
                if (!condition) throw new InvalidOperationException("K8 UI: " + caption);
                passed++; Debug.Log("K8_UI_PASS " + caption);
            };
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            require(catalog != null && catalog.Champions.Length >= 3, "Actual roster exists");
            string originalCatalog = JsonUtility.ToJson(catalog);
            require(catalog.Champions.All(unit => unit.Body != null), "Portraits use existing sprites");
            foreach (UnitDefinition champion in catalog.Champions.Where(unit => unit.AnimationFrames != null && unit.AnimationFrames.Length >= 4))
            {
                require(RunMenu.RosterPortrait(champion, 0, 0, false, false) == (champion.AnimationFrames[1] ?? champion.Body), "Roster blink uses authored frame " + champion.Id);
                require(RunMenu.RosterPortrait(champion, 6.5f, 0, true, false) == (champion.AnimationFrames[UnitVisual.AnimationFrame(6.5f, 0, true, false)] ?? champion.Body), "Focused portrait steps briefly " + champion.Id);
                require(RunMenu.RosterPortrait(champion, 6.5f, 0, true, true) == (champion.AnimationFrames[0] ?? champion.Body), "Reduced motion freezes portrait " + champion.Id);
            }
            require(catalog.Champions.Select(RunUICopy.ChampionDescription).Distinct().Count() == catalog.Champions.Length, "Each champion has a distinct description");
            foreach (UnitDefinition champion in catalog.Champions)
            {
                require(RunUICopy.UnlockCondition(champion, champion.UnlockWave).Contains("Đã mở"), "Unlock boundary " + champion.Id);
                if (champion.UnlockWave > 0) require(RunUICopy.UnlockCondition(champion, champion.UnlockWave - 1).Contains("Reach wave " + champion.UnlockWave), "Locked condition " + champion.Id);
            }
            require(RunMenu.RosterCardBounds(0).y == RunMenu.RosterCardBounds(2).y && RunMenu.RosterCardBounds(3).y > RunMenu.RosterCardBounds(2).y, "Three-column roster wraps");
            require(!RunMenu.RosterCardBounds(0).Overlaps(RunMenu.RosterCardBounds(1)), "Portrait cards do not overlap");
            require(RunHud.CombatTimerCaption(false, 0.1f) == "1" && RunHud.CombatTimerCaption(false, -1) == "0" && RunHud.CombatTimerCaption(true, -25) == "BOSSES", "Boss overtime differs from ordinary zero timer");
            foreach (float height in new[] { 0f, 100f, 600f })
                require(RunHud.ShopTooltipBounds(height).yMax <= 1030 && RunHud.ShopTooltipBounds(height).y >= 845, "Shop tooltip stays below prices with bottom safe padding " + height);
            var bond = new BondDefinition { Id = "fixture", Name = "Hệ thử", Threshold = 3, Value = 0.1f, Effect = ItemEffect.Damage,
                ExtraBonuses = new[] { new BondThresholdBonus { Threshold = 4, Value = 0.2f }, new BondThresholdBonus { Threshold = 6, Value = 0.3f } } };
            require(RunUICopy.BondDescription(bond, 3).Contains("3/4") && RunUICopy.BondDescription(bond, 4).Contains(bond.Reward(4).ToString("+0.###;-0.###;0")), "Tooltip uses actual next threshold and reward including extra bonuses");
            var feedback = new RunBondFeedback();
            var counts = new Dictionary<string, int> { { bond.Id, 2 } };
            feedback.Observe("first", new[] { bond }, counts, 10);
            require(!feedback.Flashing(bond.Id, 10), "Loading does not fake activation");
            counts[bond.Id] = 3; feedback.Observe("first", new[] { bond }, counts, 11);
            require(feedback.Flashing(bond.Id, 11) && !feedback.Flashing(bond.Id, 13), "Activation feedback expires without persistent text");
            counts[bond.Id] = 4; feedback.Observe("first", new[] { bond }, counts, 14);
            require(feedback.Flashing(bond.Id, 14), "Extra milestone triggers feedback even without base level change");
            counts[bond.Id] = 2; feedback.Observe("first", new[] { bond }, counts, 14.5f);
            require(!feedback.Flashing(bond.Id, 14.5f), "Losing threshold clears obsolete activation feedback");
            feedback.Observe("second", new[] { bond }, counts, 15);
            require(!feedback.Flashing(bond.Id, 15), "New run resets activation feedback");
            require(RunUIHints.Details(RunUIDevice.KeyboardMouse).Contains("I") && RunUIHints.Details(RunUIDevice.Controller).Contains("Y") && RunUIHints.Details(RunUIDevice.Touch).Contains("Chạm"), "Device-specific details hints");
            require(RunUIHints.Pause(RunUIDevice.Controller).Contains("Start") && RunUIHints.Interact(RunUIDevice.Controller).Contains("A"), "Pause and interaction hints match existing bindings");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 8801);
                string originalState = JsonUtility.ToJson(session.State);
                RunBuildReadModel model = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                require(model.Links.Count(row => row.Name.StartsWith("Bond ", StringComparison.Ordinal)) == catalog.Bonds.Length, "Bonds live in existing Details links tab");
                require(model.Stats.Any(row => row.Name == "Champion health" && row.Explanation.Contains("NGUỒN")), "Localized stats retain source explanations");
                string invoice = RunInvoice.Describe(session.State, catalog);
                require(invoice.Contains(RunUICopy.WeaponName(catalog.Champions[0].Weapon.Id)) && invoice.Contains("WAVE ") && !invoice.Contains("overkill") && !invoice.Contains("MEASURED"), "Invoice uses player-facing Vietnamese");
                require(originalState == JsonUtility.ToJson(session.State), "Details and invoice do not mutate run state");
                session.State.Roster[0].Tier = 2;
                RunBuildReadModel upgraded = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                CheckpointCatalog combat = session.PrepareCombat();
                require(upgraded.Companions[0].CombatValues["Cycle"] == combat.StartingCompanions[0].Weapon.Cooldown.ToString("0.###") + " s" && upgraded.Companions[0].CombatValues["Projectile"] == combat.StartingCompanions[0].Weapon.ProjectileCount.ToString(), "Tier-upgraded details agree with real attack cycle and projectile count");
            }
            string hud = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunHud.cs");
            require(hud.Contains("shop.DescribeOffer(station.Offer)") && hud.Contains("shop.DisplayName(station)"), "HUD consumes WorldShop copy API");
            require(!hud.Contains("\"II\", button") && !hud.Contains("\"I\", button"), "Battle controls use drawn icons instead of ambiguous letters");
            string details = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunDetails.cs");
            require(!details.Contains("explanation += \"\\n\\nBONDS"), "Details does not append duplicate bond blocks to every selection");
            require(originalCatalog == JsonUtility.ToJson(catalog), "UI audit does not modify authored content");
            Debug.Log("K8_UI_CHECKS_PASSED " + passed);
        }
    }
}
