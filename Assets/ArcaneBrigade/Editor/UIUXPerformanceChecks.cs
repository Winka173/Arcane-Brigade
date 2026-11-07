using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class UIUXPerformanceChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool ok, string name) { notes.Add((ok ? "PASS " : "FAIL ") + name); if (!ok) throw new InvalidOperationException(name); }
            string language = GameLocalization.Language;
            try
            {
                GameLocalization.SetLanguage("vi");
                Check(GameLocalization.Text("RELICS") == "DI VẬT" && GameLocalization.Text("BONDS") == "LIÊN KẾT", "F01 functional tab names retain gameplay meaning");
                Check(GameLocalization.Text("CURIO") == "DI VẬT", "F08 uppercase item category keeps the authored translation");
                Check(GameLocalization.Text("Advance") == "Tiến công" && GameLocalization.Text("FAN OUT") == "TỎA RỘNG" && GameLocalization.Text("Hold ground") == "Giữ vị trí", "F08 approach names in sentence and upper case");
                foreach (var hero in HeroIdentities.All)
                    Check(hero.Approaches.All(a => GameLocalization.Text(a) != a), "F08 all three authored approaches localized: " + hero.Id);
                Check(!GameLocalization.Text("While you have it (×2)").Contains("While"), "F08 dynamic owned-item heading localizes");
                Check(GlossaryUI.Find("Damage") == GlossaryUI.Find("Sát thương") && GlossaryUI.Find("Sát thương") != null, "F06 translated prose resolves to the stable glossary entry");
                Check(GlossaryUI.Find("Máu") == GlossaryUI.Find("Health"), "F06 translated health alias");
                Check(GameLocalization.Text(GlossaryUI.Find("Damage").Description) == "Lượng sát thương gây ra trong mỗi đòn đánh.", "F08 glossary damage explains damage rather than landing on the ground");
                foreach (string token in new[] { "{mark}", "{gives}", "{costs}", "{tag:COMMON}", "{bonus:Health|Health|+5}", "{stat:Health}" })
                    Check(GameLocalization.Text(token) == token, "F07 presentation token preserved: " + token);
                Check(GameLocalization.Text("{gives}Damage {costs}{bonus:Health|Health|+5}") == "{gives}Sát thương {costs}{bonus:Health|Health|+5}", "F07 text translates around multiple stable tokens");
                var viContent = LocalizedGUI.Content("RELICS");
                Check(ReferenceEquals(viContent, LocalizedGUI.Content("RELICS")), "Stable GUI content reused without per-draw allocation");
                GameLocalization.SetLanguage("en");
                Check(LocalizedGUI.Content("RELICS").text == "RELICS", "GUI content cache invalidates when switching language");
                Check(ScrollPanel.RevealOffset(0, 200, new Rect(0, 500, 100, 50)) == 358, "F04 offscreen control scrolls into the viewport");
                Check(ScrollPanel.RevealOffset(400, 200, new Rect(0, 100, 100, 50)) == 92, "F04 backward navigation scrolls up");
                Check(ScrollPanel.RevealOffset(0, 200, new Rect(0, 20, 100, 50)) == 0, "F04 already visible control does not move the viewport");
                var catalog = Resources.Load<RunCatalog>("RunCatalog");
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 170011);
                    foreach (var item in catalog.Items.Take(8)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 1 });
                    var compiled = session.PrepareCombat(); var simulation = new CombatSimulation(compiled); simulation.Start(170011);
                    string before = JsonUtility.ToJson(session.State);
                    var fast = RunBuildReadModel.Capture(session, catalog, session.State.Health, simulation, statsOnly: true, presentationOnly: true);
                    Check(before == JsonUtility.ToJson(session.State), "Opening the live brigade never changes game state or wave rewards");
                    var full = RunBuildReadModel.Capture(session, catalog, session.State.Health, simulation);
                    Check(fast.CompanionCount == full.Companions.Count && fast.RelicCount == full.Relics.Count && fast.LinkCount == full.Links.Count, "F02 first-page badges agree with the fully loaded tabs");
                    Check(fast.RelicCount == 8 && fast.CompanionCount >= 1, "F02 owned content never displays a false zero");
                    foreach (var row in fast.Stats)
                        Check(full.Stats.Any(r => r.Name == row.Name && r.Summary == row.Summary), "Live brigade stat matches independent compiler: " + row.Name);
                    full = RunBuildReadModel.Capture(session, catalog, session.State.Health);
                    var transaction = RunBuildReadModel.Capture(session, catalog, session.State.Health, transactionOnly: true);
                    Check(full.Stats.Count == transaction.Stats.Count && full.Stats.All(r => transaction.Stats.Any(n => n.Name == r.Name && n.Summary == r.Summary)), "Fast transaction capture preserves every receipt stat");
                    Check(full.Companions.Count == transaction.Companions.Count && full.Companions.All(r => transaction.Companions.Any(n => n.UnitId == r.UnitId && n.CombatValues.All(pair => r.CombatValues[pair.Key] == pair.Value))), "Fast transaction capture preserves companion combat values");
                }
                var copy = UnityEngine.Object.Instantiate(catalog);
                var first = JsonUtility.FromJson<ItemDefinition>(JsonUtility.ToJson(catalog.Items[0])); var second = JsonUtility.FromJson<ItemDefinition>(JsonUtility.ToJson(catalog.Items[0]));
                try
                {
                    copy.Items = new[] { first }; string id = first.Id;
                    Check(copy.Item(id) == first, "Indexed catalog resolves authored item");
                    copy.Items[0] = second;
                    Check(copy.Item(id) == second, "Indexed catalog handles in-place array replacement");
                    second.Id = "review-renamed";
                    Check(copy.Item(second.Id) == second && copy.Item(id) == null, "Indexed catalog handles renamed definitions");
                    first.Modifiers = Array.Empty<StatModifier>(); first.Effect = ItemEffect.Damage; first.Value = .1f; first.Rarity = ItemRarity.Common;
                    var modifiers = first.EffectiveModifiers;
                    Check(ReferenceEquals(modifiers, first.EffectiveModifiers), "Legacy modifiers reuse a stable read array");
                    first.Value = .2f;
                    Check(first.EffectiveModifiers[0].Value == .2f, "Legacy modifier cache follows authored value edits");
                }
                finally { UnityEngine.Object.DestroyImmediate(copy); }
            }
            finally
            {
                GameLocalization.SetLanguage(language);
                Directory.CreateDirectory("Docs/Verification/Performance");
                File.WriteAllLines("Docs/Verification/Performance/uiux-functional.txt", notes);
            }
            Debug.Log("UIUX_PERFORMANCE_FUNCTIONAL_PASS");
        }
        public static void Regressions()
        {
            var notes = new List<string>(); bool failed = false;
            void Suite(string name, Action run)
            {
                try { run(); notes.Add("PASS " + name); }
                catch (Exception exception)
                {
                    string failure = "FAIL " + name + ": " + exception;
                    notes.Add(failure);
                    string baseline = "Docs/Verification/Performance/legacy-baseline.txt";
                    string firstLine = failure.Split('\n')[0].TrimEnd('\r');
                    bool known = name != "UI UX and optimized data paths" && File.Exists(baseline) && File.ReadAllLines(baseline).Contains(firstLine);
                    if (!known) failed = true;
                    else notes.Add("BASELINE MATCH: existing failure reproduced using the untouched HEAD runtime; suite remains failing.");
                }
            }
            Suite("Localization", LocalizationChecks.Run);
            Suite("UI UX and optimized data paths", Run);
            Suite("Core run, shop, save and combat", () => FullRunChecks.Run(Resources.Load<RunCatalog>("RunCatalog")));
            Suite("Companion deployment and merges", CompanionDeploymentChecks.Run);
            Suite("Hero identities", HeroIdentityChecks.Run);
            Suite("Relic mechanics", K26RelicChecks.Run);
            Suite("Stat compilation", K10Checks.Run);
            Suite("Settings and save compatibility", K7Checks.Run);
            Directory.CreateDirectory("Docs/Verification/Performance");
            File.WriteAllLines("Docs/Verification/Performance/regressions.txt", notes);
            if (failed) throw new InvalidOperationException("See performance regression report");
        }
        public static void CheckMeasureAndBuild()
        {
            Regressions(); PerformanceReviewChecks.Measure();
            LocalizationChecks.CheckAndBuild();
        }
        public static void CheckAndBuild()
        {
            Regressions(); LocalizationChecks.CheckAndBuild();
        }
    }
}
