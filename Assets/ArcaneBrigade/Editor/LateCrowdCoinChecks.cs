using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class LateCrowdCoinChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException("Late crowd/coins: " + message);
            notes.Add("PASS " + message);
        }
        private static CheckpointCatalog Prepare(RunSession run, int wave)
        {
            run.Start("LanternKnight", 221026); run.State.Wave = wave;
            CheckpointCatalog c = run.PrepareCombat();
            c.Boss = null; c.EnemyBudget = 0; c.EliteCount = 0; c.WaveDuration = 1000;
            c.MapEnvironment = -1; c.ExtendedLoot = c.EnableArenaToys = c.EnableSpectacles = false;
            c.StartingCompanions = Array.Empty<UnitDefinition>(); c.ExplicitStartingRoster = true;
            c.GoldPerKill = c.MinionCoin = 1;
            return c;
        }
        private static CombatSimulation Simulation(CheckpointCatalog c)
        {
            var sim = new CombatSimulation(c); sim.Start(221026);
            sim.Champion.NextAttack = float.PositiveInfinity;
            return sim;
        }
        private static void Kill(CombatSimulation sim, CheckpointCatalog c, int count)
        {
            for (int i = 0; i < count; i++)
            {
                UnitState u = sim.AddFixture(c.MeleeEnemy, Faction.Enemy, new Vector2(8, 0));
                u.SwarmMinion = true; sim.DamageFixture(u, u.Health + 1000);
            }
        }
        private static void Win(CombatSimulation sim)
            => typeof(CombatSimulation).GetProperty(nameof(CombatSimulation.Phase)).GetSetMethod(true).Invoke(sim, new object[] { RunPhase.Victory });
        private static void Collect(CombatSimulation sim)
        {
            typeof(CombatSimulation).GetProperty(nameof(CombatSimulation.Elapsed)).GetSetMethod(true).Invoke(sim, new object[] { 5f });
            foreach (PickupState p in sim.Pickups) { p.Position = sim.Champion.Position; p.Attracting = true; }
            typeof(CombatSimulation).GetMethod("CollectPickups", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sim, new object[] { .05f });
        }

        public static void Run()
        {
            notes.Clear();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (int wave in new[] { 1, 6, 9, 10, 11, 15, 20 })
            using (var run = new RunSession(catalog))
            {
                CheckpointCatalog c = Prepare(run, wave);
                c.EnemyBudget = 2000; c.SpawnSafetyRadius = 1; c.ArenaSize = new Vector2(40, 26);
                var sim = Simulation(c);
                for (int t = 0; t < 13; t++) sim.Step(.05f, Vector2.zero);
                int original = WaveCadence.SwarmBatch(wave, sim.Elapsed);
                int small = wave < 10 ? original - 1 : Mathf.CeilToInt((original - 1) * .5f);
                int leads = wave < 10 ? 1 : 2;
                var enemies = sim.Units.Where(u => u.Alive && u.Faction == Faction.Enemy).ToList();
                Check(enemies.Count(u => u.SwarmMinion) == small, "Wave " + wave + " small bodies " + small);
                Check(enemies.Count(u => !u.SwarmMinion) == leads, "Wave " + wave + " escorts " + leads);
                Check(enemies.Sum(u => u.SwarmMinion ? c.MinionCoin : c.GoldPerKill + u.PackCoinBonus) == c.GoldPerKill + (original - 1) * c.MinionCoin, "Wave " + wave + " escort rewards preserve the pack budget before the 10% cut");
                if (wave >= 10)
                    Check(enemies.Where(u => !u.SwarmMinion).All(u => u.Definition.BodyHeight > enemies.First(u => u.SwarmMinion).Definition.BodyHeight), "Wave " + wave + " escorts are larger species");
                Check(c.KillCoinBatchSize == (wave >= 15 ? 12 : wave >= 10 ? 8 : 4), "Wave " + wave + " coin batching");
            }
            foreach (int wave in new[] { 1, 10, 15, 20 })
            using (var run = new RunSession(catalog))
            {
                CheckpointCatalog c = Prepare(run, wave);
                var sim = Simulation(c);
                Kill(sim, c, 960);
                int coinCount = sim.Pickups.Count(p => p.Kind == LootKind.Coin);
                Check(coinCount <= 32, "960 kills stay within 32 coins, wave " + wave);
                Check(sim.Pickups.Where(p => p.Kind == LootKind.Coin).Sum(p => p.Value) == 864, "90% raw reward conserved, wave " + wave);
                Win(sim); sim.SweepRemaining();
                Check(sim.Gold == 648, "960 kills pay 648 instead of 720 gold, wave " + wave);
                int banked = sim.Gold; sim.SweepRemaining();
                Check(sim.Gold == banked, "Sweep cannot pay twice, wave " + wave);
                sim.Start(221026); Kill(sim, c, 5); Win(sim); sim.SweepRemaining();
                Check(sim.Gold == 3, "Reset and partial final batch pay only this wave's reward, wave " + wave);
            }
            using (var run = new RunSession(catalog))
            {
                CheckpointCatalog c = Prepare(run, 15);
                var sim = Simulation(c);
                for (int i = 0; i < 200; i++) sim.LootFixture(LootKind.Coin, new Vector2(i % 10, i / 10), 3);
                foreach (var p in sim.Pickups) p.Attracting = true;
                for (int i = 0; i < 100; i++) sim.LootFixture(LootKind.Coin, Vector2.zero, 2);
                Check(sim.Pickups.Count == 32 && sim.Pickups.Sum(p => p.Value) == 800, "All-attracting merge preserves 800 units in 32 coins");
                Collect(sim);
                int collected = sim.Gold;
                Check(collected == 600 && sim.Pickups.Count == 0, "Normal pickup applies the same 0.75 coin rate");
                sim.Start(221026);
                sim.LootFixture(LootKind.Coin, Vector2.zero, 800);
                Win(sim); sim.SweepRemaining();
                Check(sim.Gold == collected, "Victory sweep and manual collection pay identical gold");
                sim.Start(221026); Kill(sim, c, 2);
                Check(sim.Pickups.Count == 0, "Incomplete late batch does not create tiny drops");
                Win(sim); sim.SweepRemaining();
                Check(sim.Gold == 0, "Fractional partial reward rounds down below one gold");
                sim.Start(221026); c.EnemyBudget = 2000; c.SpawnSafetyRadius = 1; c.ArenaSize = new Vector2(40, 26);
                typeof(CombatSimulation).GetMethod("SpawnWall", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sim, new object[] { 0, 20 });
                var wall = sim.Units.Where(u => u.Faction == Faction.Enemy).ToList();
                Check(wall.Count(u => u.SwarmMinion) == 10 && wall.Count(u => !u.SwarmMinion) == 2, "Late surge keeps its occurrence but replaces twenty small bodies with ten and two escorts");
                Check(wall.Sum(u => u.SwarmMinion ? c.MinionCoin : c.GoldPerKill + u.PackCoinBonus) == 20, "Surge escorts carry the missing small-body rewards");
            }
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 221026);
                foreach (var ally in catalog.Companions.Take(18))
                    run.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = ally.Weapon.Id, Tier = 1 });
                string before = JsonUtility.ToJson(run.State);
                var bonds = RunInvoice.Bonds(run.State, catalog);
                Check(bonds.Count > 5, "Record exposes more than five bond rows");
                Check(RunInvoice.Brigade(run.State).Count > 5, "Record keeps all brigade members");
                string text = RunInvoice.Describe(run.State, catalog);
                Check(bonds.All(b => text.Contains("BOND  " + b.Bond.Name)), "Plain record includes every displayed bond");
                Check(before == JsonUtility.ToJson(run.State), "Reading record bonds never migrates or mutates the saved record");
            }
            Directory.CreateDirectory("PreviewExports/LateCrowdCoins-20261022");
            File.WriteAllLines("PreviewExports/LateCrowdCoins-20261022/Focused_Checks.txt", notes);
            Debug.Log("LATE_CROWD_COINS_CHECKS_PASSED " + notes.Count);
            K4SandboxChecks.Run();
            K2Checks.Run();
            K6Checks.Run();
        }
    }
}
