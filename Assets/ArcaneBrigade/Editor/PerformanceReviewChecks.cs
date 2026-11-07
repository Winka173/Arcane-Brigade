using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class PerformanceReviewChecks
    {
        [Serializable] private sealed class Sample { public string name; public double milliseconds; public long allocatedBytes; }
        [Serializable] private sealed class Report { public string tag; public int items, units, weapons; public List<Sample> samples = new List<Sample>(); }
        public static void Measure()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-performance-tag");
            var report = new Report { tag = at >= 0 && at + 1 < args.Length ? args[at + 1] : "baseline" };
            void Time(string name, Action operation)
            {
                long bytes = GC.GetAllocatedBytesForCurrentThread(); var clock = Stopwatch.StartNew();
                operation(); clock.Stop();
                report.samples.Add(new Sample { name = name, milliseconds = clock.Elapsed.TotalMilliseconds, allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - bytes });
            }
            RunCatalog catalog = null;
            Time("catalog-load", () => catalog = Resources.Load<RunCatalog>("RunCatalog"));
            report.items = catalog.Items.Length; report.units = catalog.Companions.Length; report.weapons = catalog.Weapons.Length;
            foreach (bool expanded in new[] { false, true })
            {
                string prefix = expanded ? "expanded" : "starter";
                RunSession session = null;
                Time(prefix + "/session-constructor", () => session = new RunSession(catalog));
                using (session)
                {
                    Time(prefix + "/start", () => session.Start(catalog.Champions[0].Id, 170011));
                    if (expanded)
                    {
                        session.State.Wave = 15;
                        for (int i = 1; i < 10; i++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[i].Id, WeaponId = catalog.Companions[i].Weapon.Id, Tier = 2 });
                        foreach (var item in catalog.Items.Take(24)) session.State.Items.Add(new ItemStack { Id = item.Id, Count = 2 });
                    }
                    for (int repeat = 0; repeat < 3; repeat++)
                    {
                        Time(prefix + "/book-stats-" + repeat, () => RunBuildReadModel.Capture(session, catalog, session.State.Health, statsOnly: true));
                        Time(prefix + "/book-full-" + repeat, () => RunBuildReadModel.Capture(session, catalog, session.State.Health));
                    }
                    RunSession copy = null;
                    Time(prefix + "/read-session-constructor", () => copy = new RunSession(catalog));
                    using (copy)
                    {
                        Time(prefix + "/restore", () => copy.Restore(session.State));
                        CheckpointCatalog combat = null;
                        Time(prefix + "/prepare-combat", () => combat = copy.PrepareCombat());
                        Time(prefix + "/shop-stat-capture", () => ShopStatSnapshot.Capture(copy, catalog, combat));
                        var simulation = new CombatSimulation(combat);
                        Time(prefix + "/simulation-start", () => simulation.Start(170011));
                        Time(prefix + "/simulation-600-steps", () => { for (int step = 0; step < 600; step++) simulation.Step(1f / 60, new Vector2(Mathf.Sin(step * .02f), Mathf.Cos(step * .02f))); });
                    }
                    Time(prefix + "/catalog-lookups-10000", () => { for (int i = 0; i < 10000; i++) { catalog.Item(catalog.Items[i % catalog.Items.Length].Id); catalog.Unit(catalog.Companions[i % catalog.Companions.Length].Id); } });
                    var live = session.PrepareCombat(); var liveSimulation = new CombatSimulation(live); liveSimulation.Start(170011);
                    for (int repeat = 0; repeat < 3; repeat++)
                        Time(prefix + "/book-live-stats-" + repeat, () => RunBuildReadModel.Capture(session, catalog, session.State.Health, liveSimulation, statsOnly: true, presentationOnly: true));
                }
            }
            Directory.CreateDirectory("Docs/Verification/Performance");
            File.WriteAllText("Docs/Verification/Performance/" + report.tag + ".json", JsonUtility.ToJson(report, true));
            UnityEngine.Debug.Log("PERFORMANCE_REVIEW_MEASURED " + report.tag);
        }
        public static void MeasureLocalization()
        {
            long before = GC.GetTotalMemory(true);
            GameLocalization.SetLanguage("vi"); var clock = Stopwatch.StartNew();
            GameLocalization.Text("Settings"); clock.Stop();
            double cold = clock.Elapsed.TotalMilliseconds; long retained = GC.GetTotalMemory(true) - before;
            clock.Restart(); for (int i = 0; i < 10000; i++) GameLocalization.Text("Wave " + i); clock.Stop();
            Directory.CreateDirectory("Docs/Verification/Performance");
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args,"-performance-tag"); string tag = at >= 0 && at + 1 < args.Length ? args[at+1] : "baseline";
            File.WriteAllText("Docs/Verification/Performance/localization-" + tag + ".txt", "Cold ms: " + cold + "\nRetained managed bytes: " + retained + "\n10000 dynamic labels ms: " + clock.Elapsed.TotalMilliseconds + "\n");
        }
    }
}
