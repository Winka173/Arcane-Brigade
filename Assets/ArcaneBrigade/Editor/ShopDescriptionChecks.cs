using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ShopDescriptionChecks
    {
        private static readonly List<string> notes = new List<string>();
        private const string Root = "PreviewExports/ShopDescriptions";
        private static void Check(bool pass, string message)
        {
            notes.Add((pass ? "PASS " : "FAIL ") + message); Directory.CreateDirectory(Root); File.WriteAllLines(Root + "/Checks.txt", notes);
            if (!pass) throw new InvalidOperationException(message);
        }
        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            Check(c != null, "Live catalog loads");
            using var run = new RunSession(c); run.Start("LanternKnight", 171017);
            run.State.Wave = 12; run.State.Gold = 5000; run.State.Eggs = 300;
            var card = new ShopDescriptionCard(); var copy = new System.Text.StringBuilder();
            foreach (var u in c.Companions)
            foreach (int tier in new[] { 1, 2, 3 })
            {
                string before = JsonUtility.ToJson(run.State);
                var offer = new ShopOffer { Kind = OfferKind.Companion, Id = u.Id, Tier = tier };
                card.Refresh(run, c, offer);
                Check(JsonUtility.ToJson(run.State) == before, "Inspection preserves RNG, currency, roster and receipts: " + u.Id + " / " + tier);
                Check(card.Portrait != null && card.Attack.Choices > 0 && card.Attack.Icon != null && !float.IsInfinity(card.Attack.MinDamage) && card.Attack.MaxDamage >= card.Attack.MinDamage && card.Attack.MinCycle >= .28f, "Valid weapon icon and compiled random-weapon bounds: " + u.Id + " / " + tier);
                Check(card.Bonds.Count == (u.Tags ?? Array.Empty<string>()).Length && card.Bonds.All(b => b.Bond.Icon != null && b.Next == b.Bond.NextThreshold(b.Count)), "Every bond has its own icon and real threshold: " + u.Id + " / " + tier);
                foreach (var m in u.CompanionModifiers ?? Array.Empty<StatModifier>())
                    if (m.Value != 0) Check(card.Summary.Contains(GlossaryUI.Modifier(m.Effect, m.Scope, m.Value * UnitDefinition.ModifierScale(tier))), "All support values and scopes included: " + u.Id + " / " + tier + " / " + m.Effect);
                Check(card.Summary.Contains(CompanionDescriptions.ShortTalent(u)) && card.Details.Contains(CompanionDescriptions.Talent(u)) && card.Details.Contains(CreatureLives.Origin(u.Id)), "Short talent on the card, the full talent and lore under INFO: " + u.Id + " / " + tier);
                Check(card.Summary.Split('\n').Count(line => !line.Contains("{bonus:")) <= 6, "The companion card stays short: role line + up to 5 more (stat lines are one icon each): " + u.Id + " / " + tier + " :: " + card.Summary.Replace("\n", " | "));
                copy.AppendLine("## " + u.DisplayName + " / " + tier + " star\n" + card.Details + "\n");
            }
            foreach (var item in c.Items)
            {
                string before = JsonUtility.ToJson(run.State);
                card.Refresh(run, c, new ShopOffer { Kind = OfferKind.Item, Id = item.Id });
                Check(card.Portrait != null && card.Attack == null && card.Identity.Contains(RelicDescriptions.Category(item)), "Item icon, category and rarity: " + item.Id);
                foreach (var m in item.EffectiveModifiers) if (m.Value != 0)
                    Check(card.Summary.Contains(GlossaryUI.Modifier(m.Effect, m.Scope, m.Value)), "Every item modifier includes icon, scope and exact value: " + item.Id + " / " + m.Effect);
                string mechanic = RelicDescriptions.Mechanic(item.Mechanic), signature = RelicDescriptions.Signature(item.SignatureRole);
                Check(card.Summary.Contains(RelicDescriptions.ShortMechanic(item.Mechanic)) && card.Summary.Contains(RelicDescriptions.ShortSignature(item.SignatureRole))
                    && card.Details.Contains(mechanic) && card.Details.Contains(signature) && JsonUtility.ToJson(run.State) == before, "Short effect lines on the card, the full rules under INFO, RNG preserved: " + item.Id);
                Check(card.Summary.Split('\n').Count(line => !line.Contains("{bonus:")) <= 5, "The item card stays short (stat lines are one icon each): " + item.Id);
                copy.AppendLine("## " + item.Name + "\n" + card.Details + "\n");
            }
            var selected = c.Companions.First(u => u.ReferenceCompanion != "Evil" && u.CompanionModifiers.Length > 0);
            run.State.Roster.Clear();
            foreach (var w in c.Weapons.Where(w => run.RecruitWeaponEligible(selected.Id, w)).Take(2)) run.State.Roster.Add(new RosterEntry { UnitId = selected.Id, WeaponId = w.Id, Tier = 2 });
            card.Refresh(run, c, new ShopOffer { Kind = OfferKind.Companion, Id = selected.Id, Tier = 2 });
            Check(card.Summary.Contains("Owned 2/3 at 2-star · buying merges them into 3-star"), "Tier-two offers count tier-two ingredients");
            var high = new ShopOffer { Kind = OfferKind.Companion, Id = selected.Id, Tier = 3 };
            card.Refresh(run, c, high); Check(card.Summary.Contains("max stars"), "Maximum-tier offer does not promise a fourth star");
            Check(ShortOfferCopy.PurchaseDelta(run, c, high).StartsWith("Recruit 3-star"), "Buying a high-star body is not falsely labeled a merge");
            run.State.NewRecruitsThisWave = 3; run.State.Gold = 0;
            card.Refresh(run, c, high); Check(card.Attack.Choices > 0 && card.Details.Length > 100, "Quota or insufficient currency never hides inspection");
            Check(ShopDescriptionCard.BenefitColor(ItemEffect.EnemyHealth, -.1f) == ArcadeSkin.Better && ShopDescriptionCard.BenefitColor(ItemEffect.Armor, -.1f) == ArcadeSkin.Worse, "Enemy reductions are beneficial; ally stat penalties are drawbacks");
            File.WriteAllText(Root + "/Catalog_Descriptions.md", copy.ToString());
            Debug.Log("SHOP_DESCRIPTION_PASS " + notes.Count);
        }
        public static void VerifyAndBuild()
        {
            Run();
            const string output = "Builds/ShopDescriptions/ArcaneBrigade.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "RunBoot", "RunFrontend", "RunArena", "WaitingRoom" }.Select(s => "Assets/ArcaneBrigade/Scenes/" + s + ".unity").ToArray(), locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            File.WriteAllText(Root + "/Build.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nPlayer: " + output);
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Description player build failed");
        }
    }
}
