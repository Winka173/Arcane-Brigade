using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class SquadCheckpointChecks
    {
        public static void Run(CheckpointCatalog source)
        {
            var results = new List<string>();
            CheckpointCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.EnemyBudget = 0;
            var simulation = new CombatSimulation(catalog);
            simulation.Start(42);
            Check(simulation.CompanionCount == 3 && simulation.Units.Count == 4, "Three-body squad with sword, staff and crossbow fixtures", results);
            var distanceTravelled = new float[3];
            var firstOffsets = new Vector2[3];
            for (int tick = 0; tick < 600; tick++)
            {
                simulation.Step(1f / 60f, Vector2.zero);
                for (int index = 0; index < 3; index++)
                {
                    UnitState ally = simulation.Units[index + 1];
                    if (tick == 0) firstOffsets[index] = ally.RoamOffset;
                    distanceTravelled[index] += (ally.Position - ally.PreviousPosition).magnitude;
                    RequireInside(ally, simulation.Champion, catalog);
                }
            }
            Check(simulation.Champion.Position == Vector2.zero, "Idle champion remains stationary", results);
            bool roaming = true;
            for (int index = 0; index < 3; index++) roaming &= distanceTravelled[index] > 2f && simulation.Units[index + 1].RoamOffset != firstOffsets[index];
            Check(roaming, "Every ally patrols and retargets while champion is idle", results);
            Check(simulation.Units[1].RoamOffset != simulation.Units[2].RoamOffset && simulation.Units[2].RoamOffset != simulation.Units[3].RoamOffset, "Independent patrol destinations", results);
            bool personalSpace = true;
            for (int index = 1; index < simulation.Units.Count; index++) personalSpace &= (simulation.Units[index].Position - simulation.Champion.Position).magnitude > 0.5f;
            Check(personalSpace, "Idle squad does not settle on champion", results);
            UnitState companion = simulation.Units[1];
            Vector2 pausedPosition = companion.Position;
            Vector2 pausedOffset = companion.RoamOffset;
            float pausedRetarget = companion.NextRoamTime;
            simulation.Pause(true);
            simulation.Step(0.1f, Vector2.right);
            Check(companion.Position == pausedPosition && companion.RoamOffset == pausedOffset && companion.NextRoamTime == pausedRetarget, "Pause freezes patrol and timers", results);
            simulation.Pause(false);
            for (int tick = 0; tick < 1000; tick++)
            {
                Vector2 intent = tick < 250 ? Vector2.right : tick < 500 ? Vector2.up : tick < 750 ? Vector2.left : Vector2.down;
                simulation.Step(1f / 60f, intent);
                for (int index = 1; index < simulation.Units.Count; index++) RequireInside(simulation.Units[index], simulation.Champion, catalog);
            }
            Check(true, "Continuous leash and arena bounds during movement, reversal and corners", results);
            for (int index = 1; index < simulation.Units.Count; index++) simulation.DamageFixture(simulation.Units[index], 10000f);
            bool fallen = true;
            for (int index = 1; index < simulation.Units.Count; index++) fallen &= !simulation.Units[index].Alive && simulation.Units[index].Health == 0 && simulation.Mortality.IsFallen(simulation.Units[index]);
            Check(fallen && simulation.Phase == RunPhase.Combat, "All companions can fall while the standing hero keeps the wave alive", results);
            for (int tick = 0; tick < 151; tick++) simulation.Step(.1f, Vector2.zero);
            bool revived = true;
            for (int index = 1; index < simulation.Units.Count; index++) revived &= simulation.Units[index].Alive && Mathf.Abs(simulation.Units[index].Health - simulation.Units[index].Definition.Health * MortalityTuning.RiseHealthFraction) < .001f;
            Check(revived, "All fallen companions rise at 40 percent health after 15 seconds", results);
            simulation.Start(123);
            var second = new CombatSimulation(catalog);
            second.Start(123);
            for (int tick = 0; tick < 180; tick++)
            {
                Vector2 intent = new Vector2(Mathf.Sin(tick * 0.03f), Mathf.Cos(tick * 0.02f));
                simulation.Step(1f / 60f, intent);
                second.Step(1f / 60f, intent);
            }
            bool reproducible = true;
            for (int index = 1; index < simulation.Units.Count; index++) reproducible &= simulation.Units[index].Position == second.Units[index].Position && simulation.Units[index].RoamOffset == second.Units[index].RoamOffset;
            Check(reproducible, "Same seed and input reproduce patrol in this Editor", results);
            simulation.Start(42);
            UnitDefinition target = UnityEngine.Object.Instantiate(catalog.MeleeEnemy);
            target.Health = 10000f;
            target.Speed = 0f;
            simulation.AddFixture(target, Faction.Enemy, new Vector2(2.8f, 0f));
            int allyFireCount = 0;
            for (int tick = 0; tick < 180; tick++)
            {
                simulation.Step(1f / 60f, Vector2.zero);
                foreach (CombatEvent combatEvent in simulation.Events) if (combatEvent.Kind == CombatEventKind.Fire && combatEvent.Faction == Faction.Companion) allyFireCount++;
            }
            Check(allyFireCount >= 3, "Squad keeps auto-attacking while roaming", results);
            simulation.Start(42);
            for (int index = simulation.CompanionCount; index < RulesV3Tuning.CompanionLimit; index++) simulation.AddCompanion(catalog.Companion);
            bool rejected = false;
            try { simulation.AddCompanion(catalog.Companion); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected && simulation.CompanionCount == RulesV3Tuning.CompanionLimit, "Recruit API admits ten companions and rejects the eleventh", results);
            simulation.Start(42);
            Check(simulation.CompanionCount == 3 && simulation.Units[1].Velocity == Vector2.zero && simulation.Units[1].NextRoamTime == 0f, "Restart clears squad additions and patrol state", results);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(catalog);
            File.WriteAllText("PreviewExports/Checkpoint02_Checks.txt", "Squad deterministic Editor checks\n" + string.Join("\n", results) + "\nTotal: " + results.Count + " passed\nNot a mobile or large-army performance certification.\n");
            Debug.Log("ARCANE_SQUAD_CHECKS_PASSED " + results.Count);
        }

        private static void RequireInside(UnitState ally, UnitState champion, CheckpointCatalog catalog)
        {
            if ((ally.Position - champion.Position).magnitude > CompanionMotion.Leash(ally, catalog) + 0.001f || Mathf.Abs(ally.Position.x) > catalog.ArenaSize.x * 0.5f - ally.Definition.Radius + 0.001f || Mathf.Abs(ally.Position.y) > catalog.ArenaSize.y * 0.5f - ally.Definition.Radius + 0.001f)
                throw new InvalidOperationException("Ally left leash or arena: " + ally.Id + " dist " + (ally.Position - champion.Position).magnitude + " leash " + catalog.CompanionLeash + " pos " + ally.Position + " arena " + catalog.ArenaSize + " melee " + (ally.Definition.Weapon.Kind == WeaponKind.Melee));
        }

        private static void Check(bool condition, string name, List<string> results)
        {
            if (!condition) throw new InvalidOperationException("Squad check failed: " + name);
            results.Add("PASS " + name);
        }
    }
}
