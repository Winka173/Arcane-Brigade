using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Measures every weapon in the catalog with the build-balance weapon fixture (BuildBalanceDiagnostics.Fight: one
    /// companion carrying it, no hero damage, no story party, 25 s of a real wave) over three seeds, tiers 1-3 and waves 5/15,
    /// after ArsenalBuilder has written the current ArsenalSpecs numbers. For each tier/wave/seed an Arsenal weapon's damage
    /// is divided by the median of the older weapons of its kind; its ratio is the geometric mean of those (one seed alone
    /// swings with how many enemies die). Output: PreviewExports/BuildBalance/Arsenal{label}/.</summary>
    public static class ArsenalBalanceProbe
    {
        private static readonly int[] Seeds = { 77138, 161026, 33071 };

        public static void Run()
        {
            string label = "Arsenal" + (Environment.GetCommandLineArgs().SkipWhile(a => a != "-probe-label").Skip(1).FirstOrDefault() ?? "Probe");
            ArsenalBuilder.Apply();
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            string root = "PreviewExports/BuildBalance/" + label; Directory.CreateDirectory(root);
            MethodInfo fight = typeof(BuildBalanceDiagnostics).GetMethod("Fight", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("BuildBalanceDiagnostics.Fight not found");
            var rows = new List<string> { "weapon,kind,tier,wave,seed,damage,kills,seconds" };
            var damage = new Dictionary<(string, int, int, int), float>();
            foreach (WeaponDefinition weapon in catalog.Weapons)
                for (int tier = 1; tier <= 3; tier++)
                    foreach (int wave in new[] { 5, 15 })
                        foreach (int seed in Seeds)
                        {
                            using var run = new RunSession(catalog); run.Start("LanternKnight", seed);
                            run.State.Wave = wave; run.State.Roster.Clear();
                            run.State.Roster.Add(new RosterEntry { UnitId = "JellySwordsman", WeaponId = weapon.Id, Tier = tier });
                            var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                            arena.Champion.Weapon.Damage = 0; arena.Champion.CollisionDamage = 0; arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                            var sim = (CombatSimulation)fight.Invoke(null, new object[] { arena, seed, 25f, false });
                            float dealt = sim.Contributions.Where(c => c.UnitId == "JellySwordsman").Sum(c => c.Damage);
                            damage[(weapon.Id, tier, wave, seed)] = dealt;
                            rows.Add(string.Join(",", weapon.Id, weapon.Kind, tier, wave, seed, dealt.ToString("0.0", CultureInfo.InvariantCulture), sim.Kills, sim.Elapsed.ToString("0.0", CultureInfo.InvariantCulture)));
                        }
            File.WriteAllLines(root + "/Weapons_Seeds.csv", rows);
            var arsenal = new HashSet<string>(ArsenalSpecs.All.Select(s => s.Id));
            var lines = new List<string> { "weapon\tkind\tratio\tlow\thigh" };
            foreach (WeaponDefinition weapon in catalog.Weapons.Where(w => arsenal.Contains(w.Id)).OrderBy(w => w.Id))
            {
                var logs = new List<float>();
                for (int tier = 1; tier <= 3; tier++)
                    foreach (int wave in new[] { 5, 15 })
                        foreach (int seed in Seeds)
                        {
                            var legacy = catalog.Weapons.Where(w => !arsenal.Contains(w.Id) && w.Kind == weapon.Kind).Select(w => damage[(w.Id, tier, wave, seed)]).OrderBy(v => v).ToList();
                            float median = legacy[legacy.Count / 2];
                            logs.Add(Mathf.Log(Mathf.Max(1, damage[(weapon.Id, tier, wave, seed)]) / Mathf.Max(1, median)));
                        }
                logs.Sort();
                lines.Add(string.Join("\t", weapon.Id, weapon.Kind, Mathf.Exp(logs.Average()).ToString("0.00", CultureInfo.InvariantCulture),
                    Mathf.Exp(logs[logs.Count / 4]).ToString("0.00", CultureInfo.InvariantCulture), Mathf.Exp(logs[logs.Count * 3 / 4]).ToString("0.00", CultureInfo.InvariantCulture)));
            }
            File.WriteAllLines(root + "/Arsenal_Ratios.tsv", lines);
            Debug.Log("ARSENAL_BALANCE " + label + " " + (lines.Count - 1));
        }
    }
}
