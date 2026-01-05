using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class CheckpointChecks
    {
        public static void Run(CheckpointCatalog source)
        {
            var results = new List<string>();
            CheckpointCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.EnemyBudget = 0;
            catalog.StartingCompanions = null;
            var simulation = new CombatSimulation(catalog);
            simulation.Start(42);
            Check(simulation.Units.Count == 2 && simulation.Phase == RunPhase.Combat, "Initial champion and companion", results);
            simulation.Step(1f / 60f, Vector2.one);
            Check(Mathf.Abs(simulation.Champion.Position.magnitude - source.Champion.Speed / 60f) < 0.001f, "Normalized diagonal movement", results);
            Check(simulation.ClampPosition(new Vector2(999f, 999f), 0.25f) == source.ArenaSize * 0.5f - Vector2.one * 0.25f, "Arena bounds", results);
            simulation.Pause(true);
            float elapsed = simulation.Elapsed;
            simulation.Step(0.1f, Vector2.right);
            Check(simulation.Elapsed == elapsed, "Pause freezes simulation", results);
            simulation.Pause(false);
            UnitState companion = simulation.Units[1];
            simulation.DamageFixture(companion, 10000f);
            Check(companion.Alive && float.IsPositiveInfinity(companion.Health), "Immortal companion without finite HP", results);
            simulation.DamageFixture(simulation.Champion, 4f);
            simulation.DamageFixture(simulation.Champion, 5f);
            Check(simulation.Champion.Health == 91f, "Concurrent damage without global invulnerability", results);
            simulation.Start(42);
            companion = simulation.Units[1];
            companion.Position = new Vector2(-0.5f, 0f);
            simulation.ProjectileFixture(new Vector2(-2f, 0f), new Vector2(40f, 0f), 5f, Faction.Enemy);
            simulation.Step(0.1f, Vector2.zero);
            Check(simulation.Champion.Health == 95f && companion.Alive && simulation.Projectiles.Count == 0, "Swept projectile passes companion and hits champion", results);
            simulation.Start(42);
            UnitState contact = simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, new Vector2(0.45f, 0f));
            simulation.Step(1f / 60f, Vector2.zero);
            float afterContact = simulation.Champion.Health;
            simulation.Step(1f / 60f, Vector2.zero);
            Check(afterContact == 96f && simulation.Champion.Health == afterContact, "Per-source contact cooldown", results);
            simulation.DamageFixture(contact, 100f);
            for (int tick = 0; tick < 28; tick++) simulation.Step(1f / 60f, Vector2.zero);
            Check(simulation.Kills == 1 && simulation.Gold == catalog.GoldPerKill, "Kill drop and proximity pickup", results);
            simulation.Start(42);
            simulation.Units[1].NextAttack = float.PositiveInfinity;
            simulation.AddFixture(catalog.MeleeEnemy, Faction.Enemy, new Vector2(1.2f, 0f));
            for (int tick = 0; tick < 16; tick++) simulation.Step(1f / 60f, Vector2.zero);
            UnitState attacked = null;
            foreach (UnitState unit in simulation.Units) if (unit.Faction == Faction.Enemy) attacked = unit;
            Check(attacked != null && attacked.Health == catalog.MeleeEnemy.Health - catalog.Champion.Weapon.Damage, "One melee impact per attack windup", results);
            simulation.Start(42);
            for (int tick = 0; tick < 650; tick++) simulation.Step(1f / 60f, Vector2.right);
            Check((simulation.Units[1].Position - simulation.Champion.Position).magnitude <= catalog.CompanionLeash + 0.001f, "Companion stays within leash", results);
            simulation.DamageFixture(simulation.Champion, 200f);
            Check(simulation.Phase == RunPhase.Defeat, "Defeat transition", results);
            simulation.Start(42);
            Check(simulation.Generation == 6 && simulation.Gold == 0 && simulation.Champion.Health == 100f && simulation.Projectiles.Count == 0, "Restart clears run state", results);
            catalog.WaveDuration = 0.1f;
            simulation.Step(0.1f, Vector2.zero);
            Check(simulation.Phase == RunPhase.Victory, "Wave timer victory", results);
            Transform grip = source.UnitPrefab.transform.Find("GripRoot");
            Check(grip != null && grip.Find("WeaponVisual") != null && grip.Find("HandVisual") != null && grip.Find("MuzzleMarker") != null, "Hand and weapon share grip transform", results);
            foreach (Transform child in source.UnitPrefab.GetComponentsInChildren<Transform>(true))
                if (child.name.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0) throw new InvalidOperationException("Unexpected arm bone: " + child.name);
            Check(source.Hand != null && source.Coin != null && source.Projectile != null && source.Champion.Body != null, "Atlas assets and no arm bones", results);
            Check(source.UnitPrefab.transform.Find("ShadowVisual").GetComponent<SpriteRenderer>().sprite != null, "Persistent shadow sprite", results);
            UnityEngine.Object.DestroyImmediate(catalog);
            File.WriteAllText("PreviewExports/Checkpoint01_Checks.txt", "Editor deterministic smoke checks\n" + string.Join("\n", results) + "\nTotal: " + results.Count + " passed\nNot a device or performance certification.\n");
            Debug.Log("ARCANE_CHECKPOINT_CHECKS_PASSED " + results.Count);
        }

        private static void Check(bool condition, string name, List<string> results)
        {
            if (!condition) throw new InvalidOperationException("Checkpoint check failed: " + name);
            results.Add("PASS " + name);
        }
    }
}
