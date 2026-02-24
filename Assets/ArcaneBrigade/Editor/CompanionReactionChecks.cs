using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class CompanionReactionChecks
    {
        private static readonly List<string> checks = new List<string>();
        private const string Root = "PreviewExports/CompanionReactions";
        private static void Check(bool pass, string label)
        {
            checks.Add((pass ? "PASS " : "FAIL ") + label); Directory.CreateDirectory(Root); File.WriteAllLines(Root + "/Checks.txt", checks);
            if (!pass) throw new InvalidOperationException(label);
        }
        public static void Run()
        {
            checks.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            var art = AssetDatabase.LoadAssetAtPath<PaintedStatIcons>("Assets/ArcaneBrigade/Resources/PaintedStatIcons.asset");
            Check(art != null && art.Entries.Length == Enum.GetValues(typeof(ItemEffect)).Length, "Painted art covers every stat");
            foreach (ItemEffect e in Enum.GetValues(typeof(ItemEffect)))
            {
                var row = art.Entries.Single(r => r.Effect == e);
                Check(row.Picture != null && row.Ink.width > 8 && row.Ink.height > 8 && !string.IsNullOrEmpty(row.Source), "Traceable nonempty painted stat icon: " + e);
                Check(row.Badge == null || row.BadgeInk.width > 0 && row.BadgeInk.height > 0, "Scope/correction badge is complete: " + e);
            }
            var copy = new List<string> { "Species\tSituation\tLine" };
            Check(CompanionReactions.SpeciesCount == c.Companions.Length, "Personal reaction bank covers the entire companion catalog");
            foreach (var u in c.Companions)
            {
                foreach (CompanionReactionKind kind in Enum.GetValues(typeof(CompanionReactionKind)))
                {
                    string line = CompanionReactions.Line(u.Id, kind);
                    Check(!string.IsNullOrEmpty(line) && line.Length <= 110 && line != CompanionBanter.Line(u.Id, 0, 1), "Authored short personal reaction: " + u.Id + " / " + kind);
                    copy.Add(u.Id + "\t" + kind + "\t" + line);
                }
                Check(SpeechBubbleCast.StarLine(u.Id, 2, 5) == CompanionReactions.Line(u.Id, CompanionReactionKind.Merge), "Merge uses species identity: " + u.Id);
            }
            foreach (CompanionReactionKind kind in Enum.GetValues(typeof(CompanionReactionKind)))
                Check(c.Companions.Select(u => CompanionReactions.Line(u.Id, kind)).Distinct().Count() == c.Companions.Length, "No shared generic line in " + kind);
            File.WriteAllLines(Root + "/Reaction_Lines.tsv", copy);
            using var run = new RunSession(c); run.Start("LanternKnight", 171017); run.State.Wave = 6; run.State.Phase = SessionPhase.Shop; run.State.Gold = 4000; run.State.Roster.Clear(); run.State.SpecialOffers.Clear();
            var drinker = c.Companions.First(u => u.ReferenceCompanion == "Alien");
            run.State.Roster.Add(new RosterEntry { UnitId = drinker.Id, WeaponId = "CrescentBow" });
            run.Restore(run.State);
            var drink = c.Items.First(i => i.Category == RelicCategory.Drink);
            var offer = new ShopOffer { Token = ++run.State.NextOffer, Id = drink.Id, Kind = OfferKind.Item, Price = 2 }; run.State.Offers.Clear(); run.State.Offers.Add(offer);
            var before = MergeReveal.Capture(run.State);
            Check(CompanionReactions.AfterPurchase(c, before, run, offer).Count == 0, "Inspecting an unsold offer cannot create a purchase line");
            Check(!RunTransaction.TryCommit(run, c, "Failed save", () => run.Buy(offer.Token), () => false, out _), "Failed-save transaction rolls back");
            Check(CompanionReactions.AfterPurchase(c, before, run, offer).Count == 0, "Failed persistence produces no companion reaction");
            offer = run.State.Offers[0];
            Check(RunTransaction.TryCommit(run, c, "Drink", () => run.Buy(offer.Token), () => true, out _), "A drink purchase commits");
            string stable = JsonUtility.ToJson(run.State);
            var response = CompanionReactions.AfterPurchase(c, before, run, offer);
            Check(response.Count == 1 && response[0].UnitId == drinker.Id && response[0].Kind == CompanionReactionKind.Purchase, "The drink-reactive companion answers its actual purchase");
            Check(stable == JsonUtility.ToJson(run.State), "Planning a response changes no saved counters or RNG");
            Check(CompanionReactions.AfterPurchase(c, MergeReveal.Capture(run.State), run, offer).Count == 0, "Reloading or observing an old receipt cannot replay it");
            var unit = c.Companions.First(u => u.ReferenceCompanion == "Cheer Bunny");
            var melee = c.Weapons.First(w => w.Kind == WeaponKind.Melee); var ranged = c.Weapons.First(w => w.Kind == WeaponKind.Projectile);
            var flat = new ItemDefinition { Modifiers = new[] { new StatModifier { Effect = ItemEffect.RangedDamage, Scope = ModifierScope.Team, Value = 2 } } };
            Check(CompanionReactions.Affinity(unit, melee, flat) == 0 && CompanionReactions.Affinity(unit, ranged, flat) > 0, "Flat ranged benefits choose a ranged user, not a melee user");
            flat.Modifiers[0].Scope = ModifierScope.Champion;
            Check(CompanionReactions.Affinity(unit, ranged, flat) == 0, "Hero-only damage does not pretend to benefit a companion");
            flat.Modifiers[0].Scope = ModifierScope.Team; flat.Modifiers[0].Value = -2;
            Check(CompanionReactions.Affinity(unit, ranged, flat) == 0, "A drawback alone produces no benefit reaction");
            run.State.Roster.Clear(); run.State.Offers.Clear(); run.State.Gold = 4000; run.State.NewRecruitsThisWave = 0; run.State.NewSpeciesRecruitsThisWave = 0;
            run.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = melee.Id }); run.Restore(run.State);
            var recruit = new ShopOffer { Token = ++run.State.NextOffer, Id = unit.Id, Kind = OfferKind.Companion, Price = 2 }; run.State.Offers.Add(recruit); before = MergeReveal.Capture(run.State);
            Check(RunTransaction.TryCommit(run, c, "Bond recruit", () => run.Buy(recruit.Token), () => true, out _), "A second matching bond body is bought");
            response = CompanionReactions.AfterPurchase(c, before, run, recruit);
            Check(response.Count == 1 && response[0].Kind == CompanionReactionKind.Bond && response[0].UnitId == unit.Id && !string.IsNullOrEmpty(response[0].BondId), "A real threshold crossing selects a member of that bond; at most one response");
            before = MergeReveal.Capture(run.State); run.State.Phase = SessionPhase.Combat;
            Check(CompanionReactions.AfterPurchase(c, before, run, recruit).Count == 0, "Combat is outside purchase chatter");
            Debug.Log("COMPANION_REACTIONS_PASS " + checks.Count);
        }
        public static void VerifyAndBuild()
        {
            PaintedStatIconBuilder.Import(); Run();
            string path = "Builds/CompanionReactions/ArcaneBrigade.exe";
            var args = Environment.GetCommandLineArgs(); int output = Array.IndexOf(args, "-reactionBuildPath");
            if (output >= 0 && output + 1 < args.Length) path = Path.GetFullPath(args[output + 1]);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "RunBoot", "RunFrontend", "RunArena", "WaitingRoom" }.Select(s => "Assets/ArcaneBrigade/Scenes/" + s + ".unity").ToArray(), locationPathName = path, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            File.WriteAllText(Root + "/Build.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nPlayer: " + path + "\n");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Reaction player build failed");
        }
    }
}
