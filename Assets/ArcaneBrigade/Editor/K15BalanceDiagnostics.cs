using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K15BalanceDiagnostics
    {
        public static void Run()
        {
            RunInto("PreviewExports/K15");
        }
        public static void RunInto(string directory)
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var rows = new List<string> { "wave,roster,composition,boss,phase,seconds,kills,damage_taken,ally_damage,first_ally_hit,mean_melee_distance,mean_ranged_distance" };
            foreach (int wave in new[] { 2, 8, 16 })
            foreach (int count in new[] { 2, 12, 20 })
            foreach (string kind in new[] { "melee", "ranged", "mixed" }) Measure(catalog, rows, wave, count, kind, false);
            foreach (int count in new[] { 2, 12, 20 })
            foreach (string kind in new[] { "melee", "ranged" }) Measure(catalog, rows, 8, count, kind, true);
            Directory.CreateDirectory(directory); File.WriteAllLines(Path.Combine(directory, "Encounter_Balance.csv"), rows);
            Debug.Log("K15_ENCOUNTER_MEASUREMENTS " + (rows.Count - 1) + " authored-roster encounters; not natural shop builds or human approval");
        }
        private static void Measure(RunCatalog catalog, List<string> rows, int wave, int count, string kind, bool boss)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 151077); session.State.Wave = wave; session.State.Roster.Clear();
                WeaponDefinition[] pool = catalog.Weapons.Where(weapon => !weapon.SpecialOnly && (kind == "mixed" || (weapon.Kind == WeaponKind.Melee) == (kind == "melee"))).ToArray();
                for (int index = 0; index < count; index++) session.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = pool[index % pool.Length].Id, Tier = wave < 4 ? 1 : wave < 12 ? 2 : 3 });
                CheckpointCatalog combat = session.PrepareCombat(); if (!boss) combat.Boss = null;
                var simulation = new CombatSimulation(combat); simulation.Start(151077);
                float taken = 0, dealt = 0, first = -1, melee = 0, ranged = 0; int meleeSamples = 0, rangedSamples = 0;
                for (int tick = 0; tick < 1080 && simulation.Phase == RunPhase.Combat; tick++)
                {
                    simulation.Step(1f / 60, K13MovementPolicy.Steer(simulation));
                    foreach (CombatEvent value in simulation.Events)
                    {
                        if (value.Kind != CombatEventKind.Hit) continue;
                        if (value.Faction == Faction.Champion) taken += value.Value;
                        if (value.Faction == Faction.Enemy && simulation.Units.Any(unit => unit.Faction == Faction.Companion && unit.Id == value.Cause.SourceUnitId)) { dealt += value.Value; if (first < 0) first = simulation.Elapsed; }
                    }
                    if (tick % 6 != 0) continue;
                    foreach (UnitState ally in simulation.Units)
                    {
                        if (ally.Faction != Faction.Companion) continue;
                        UnitState nearest = null; float distance = float.PositiveInfinity;
                        foreach (UnitState enemy in simulation.Units)
                            if (enemy.Alive && enemy.Faction == Faction.Enemy && Vector2.Distance(ally.Position, enemy.Position) < distance) { nearest = enemy; distance = Vector2.Distance(ally.Position, enemy.Position); }
                        if (nearest == null) continue;
                        if (ally.Definition.Weapon.Kind == WeaponKind.Melee) { melee += distance; meleeSamples++; } else { ranged += distance; rangedSamples++; }
                    }
                }
                rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5:0.00},{6},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00}", wave, count, kind, boss, simulation.Phase, simulation.Elapsed, simulation.Kills, taken, dealt, first, melee / Mathf.Max(1, meleeSamples), ranged / Mathf.Max(1, rangedSamples)));
            }
        }
    }
}
