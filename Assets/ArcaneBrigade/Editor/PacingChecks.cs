using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The pacing and economy pass: merges with the story party, coins, wave lengths, surges, swarm roles,
    /// overkill, the Lantern Flare, the bell and the end-of-wave sweep.</summary>
    public static class PacingChecks
    {
        private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

        private static CombatSimulation Fight(RunSession session, int wave, int seed)
        {
            session.State.Wave = wave; session.State.Phase = SessionPhase.Combat;
            CheckpointCatalog arena = session.PrepareCombat();
            arena.StartingCompanions = Array.Empty<UnitDefinition>(); arena.StoryCompanions = Array.Empty<UnitDefinition>(); arena.ExplicitStartingRoster = true;
            var simulation = new CombatSimulation(arena); simulation.Start(seed);
            simulation.Champion.NextAttack = float.PositiveInfinity; // the brigade stays out of the fixtures
            simulation.Champion.Definition.Health = simulation.Champion.Health = 1e7f; // and the traveller outlasts any crowd
            return simulation;
        }
        private static void Steps(CombatSimulation simulation, float seconds, bool immortal = true)
        {
            for (int tick = 0; tick < Mathf.CeilToInt(seconds * 60) && simulation.Phase == RunPhase.Combat; tick++)
            {
                if (immortal) simulation.Champion.Health = simulation.Champion.Definition.Health;
                simulation.Step(1f / 60, Vector2.zero);
            }
        }
        private static void Clear(CombatSimulation simulation)
        {
            foreach (UnitState unit in simulation.Units) if (unit.Faction == Faction.Enemy && !unit.Definition.IsBoss) unit.Position = new Vector2(1000, 1000);
        }

        /// <summary>Batch: -executeMethod ArcaneBrigade.Editor.PacingChecks.RunBatch</summary>
        public static void RunBatch()
        {
            int code = 0;
            try { Run(); Debug.Log("PACING_CHECKS PASS"); } catch (Exception error) { Debug.Log("PACING_CHECKS FAIL " + error.Message + " | " + error.StackTrace); code = 1; }
            UnityEditor.EditorApplication.Exit(code);
        }

        public static void Run()
        {
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");

            // a run that beat the wave-5 Keeper (a story boss outside the roster) still saves, loads and trades
            using (var s = new RunSession(c))
            {
                s.Start("DrowsySeer", 5005);
                Check(!c.Bosses.Any(boss => boss.Id == "OutsideStageKeeper") && RunSession.IsStoryBossId("OutsideStageKeeper"), "The Keeper is a story boss, not a roster boss");
                s.State.DefeatedBosses.Add("OutsideStageKeeper");
                s.Validate(s.State); // threw "Invalid boss discovery" and rolled back every purchase and the way out
                Check(!s.State.DefeatedBosses.Contains("OutsideStageKeeper"), "A story boss recorded by mistake is dropped from the discoveries");
                var copy = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)); copy.DefeatedBosses.Add("NotABoss");
                bool rejected = false; try { s.Validate(copy); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "Unknown boss ids are still rejected");
            }

            // merging beside the story party
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5001);
                Check(!s.State.Roster.Any(entry => RunSession.StorySpecies(entry.UnitId)), "The starting brigade never doubles Hob, Latch, Postscript or Tally");
                s.State.Phase = SessionPhase.Shop;
                for (int roll = 0; roll < 40; roll++)
                {
                    s.State.Gold = 1000; s.Reroll();
                    Check(!s.State.Offers.Any(offer => offer.Kind == OfferKind.Companion && RunSession.StorySpecies(offer.Id)), "The four's kinds are never sold");
                }
            }
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5002); s.State.Phase = SessionPhase.Shop; s.State.Wave = 6;
                UnitDefinition species = c.Companions.First(unit => !unit.SpecialRecruit && !unit.GiftOnly && !RunSession.StorySpecies(unit.Id) && K11HeroRules.Eligible(c.Unit("LanternKnight"), unit)
                    && c.Weapons.Count(weapon => RecruitEligible(s, unit, weapon)) >= 3);
                s.State.Roster.RemoveAll(entry => entry.UnitId == species.Id);
                var weapons = c.Weapons.Where(weapon => RecruitEligible(s, species, weapon)).Take(2).ToArray();
                foreach (WeaponDefinition weapon in weapons) s.State.Roster.Add(new RosterEntry { UnitId = species.Id, WeaponId = weapon.Id, Tier = 1 });
                s.State.Offers.Clear();
                s.State.Offers.Add(new ShopOffer { Token = ++s.State.NextOffer, Kind = OfferKind.Companion, Id = species.Id, Tier = 1, Price = 1 });
                s.State.Gold = 100;
                Check(s.Buy(s.State.NextOffer), "A third copy can be bought");
                Check(s.State.Roster.Count(entry => entry.UnitId == species.Id && entry.Tier == 2) == 1 && !s.State.Roster.Any(entry => entry.UnitId == species.Id && entry.Tier == 1),
                    "Three one-star copies become exactly one two-star, with nothing left behind");
            }

            // economy and wave lengths
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5003);
                s.State.Wave = 1; CheckpointCatalog first = s.PrepareCombat();
                Check(Mathf.Approximately(first.WaveDuration, 30) && first.GoldPerKill == 1 && first.MinionCoin == 1 && first.BossSwarmBudget == 0, "Wave 1 is a 30 s night; every swarm body drops one coin");
                s.State.Wave = 13; CheckpointCatalog late = s.PrepareCombat();
                Check(Mathf.Approximately(late.WaveDuration, 60) && late.GoldPerKill == 3, "From wave 13 a night lasts a minute; a full enemy's coin is 3");
                s.State.Wave = 10; CheckpointCatalog boss = s.PrepareCombat();
                Check(boss.Boss != null && boss.BossSwarmBudget > 0 && Mathf.Approximately(boss.WaveDuration, OctoberBalance.WaveSeconds), "A boss wave keeps a light swarm while the boss stands");
                Check(Mathf.Approximately(WaveCadence.PriceRamp(1), 1) && Mathf.Approximately(WaveCadence.PriceRamp(30), 1.6f), "Prices climb at most 60% with the night");
                for (int wave = 1; wave < 20; wave++) Check(SwarmPacingRules.SurgeEvery(wave) >= 20, "Surges never come more often than every 20 s");
                s.State.Wave = 20; CheckpointCatalog duel = s.PrepareCombat();
                Check(duel.FinalDuel && duel.BossSwarmBudget > 0 && duel.EnemyBudget == 0, "The Last Customer's duel keeps a swarm and its walls");
                var walls = new CombatSimulation(duel); walls.Start(5010);
                for (int tick = 0; tick < 600; tick++) { walls.Champion.Health = walls.Champion.Definition.Health; walls.Step(1f / 60, Vector2.zero); }
                Check(walls.Units.Count(u => u.Alive && u.Faction == Faction.Enemy) > 1, "The duel's swarm is really there");
            }

            // surges: rare, and they really come
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5004);
                CombatSimulation sim = Fight(s, 8, 5004);
                var times = new List<float>(); int fired = 0;
                for (int tick = 0; tick < 50 * 60 && sim.Phase == RunPhase.Combat; tick++)
                {
                    sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(1f / 60, Vector2.zero);
                    if (sim.SurgesFired > fired) { fired = sim.SurgesFired; times.Add(sim.Elapsed); }
                }
                Check(fired >= 1, "A wave of 47 s has at least one surge");
                for (int i = 1; i < times.Count; i++) Check(times[i] - times[i - 1] >= 20, "Surges are at least 20 s apart");
            }

            // every swarm body drops a coin
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5005);
                CombatSimulation sim = Fight(s, 6, 5005); Steps(sim, 1); Clear(sim);
                int coinsBefore = sim.Pickups.Count(p => p.Kind == LootKind.Coin);
                var minions = new List<UnitState>();
                for (int i = 0; i < 6; i++) { UnitState unit = sim.PacingFixtureRole(SwarmRole.None, sim.Champion.Position + new Vector2(4 + i, 3), 1); unit.SwarmMinion = true; minions.Add(unit); }
                foreach (UnitState unit in minions) sim.PacingFixtureKill(unit);
                Check(sim.Pickups.Count(p => p.Kind == LootKind.Coin) - coinsBefore >= 6, "Six swarm bodies leave six coins");
            }

            // ink bloaters: friendly bursts that chain, three links at most
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5006);
                CombatSimulation sim = Fight(s, 6, 5006); Steps(sim, 0.5f); Clear(sim);
                var chain = new List<UnitState>();
                for (int i = 0; i < 10; i++) chain.Add(sim.PacingFixtureRole(SwarmRole.Bloater, sim.Champion.Position + new Vector2(-6 + i * 1.2f, 4), 5));
                foreach (UnitState unit in chain) unit.Definition.Speed = 0; // a still line, so each burst reaches only the next one
                float health = sim.Champion.Health;
                sim.PacingFixtureKill(chain[0]);
                int blasts = 0;
                for (int tick = 0; tick < 90; tick++) { sim.Step(1f / 60, Vector2.zero); blasts += sim.Events.Count(e => e.Kind == CombatEventKind.Detonation); }
                int dead = chain.Count(unit => !unit.Alive);
                Check(dead == 2 + SwarmPacingRules.ChainDepth && blasts == 1 + SwarmPacingRules.ChainDepth, "A popped bloater sets off its neighbours, three links at most (" + dead + " of 10 dead, " + blasts + " blasts)");
                Check(sim.Champion.Health >= health - 0.01f, "Bloater ink never hurts the traveller");
            }

            // fuse runners: rush, light, blow on the traveller; popped early, they burst on their own kind
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5007);
                CombatSimulation sim = Fight(s, 6, 5007); Steps(sim, 0.5f); Clear(sim);
                UnitState fuse = sim.PacingFixtureRole(SwarmRole.Fuse, sim.Champion.Position + new Vector2(5, 0), 1e6f);
                float lit = -1;
                for (int tick = 0; tick < 6 * 60 && fuse.Alive; tick++) { sim.Step(1f / 60, Vector2.zero); if (lit < 0 && fuse.FuseAt > 0) lit = sim.Elapsed; sim.Champion.Health = Mathf.Max(sim.Champion.Health, 1); }
                Check(lit > 0 && !fuse.Alive, "A fuse runner reaches the traveller, lights and is gone");
            }

            // overkill spills into the nearest body
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5008);
                CombatSimulation sim = Fight(s, 6, 5008); Steps(sim, 0.5f); Clear(sim);
                UnitState victim = sim.PacingFixtureRole(SwarmRole.None, sim.Champion.Position + new Vector2(4, 0), 2);
                UnitState neighbour = sim.PacingFixtureRole(SwarmRole.None, sim.Champion.Position + new Vector2(4.5f, 0), 500);
                victim.SwarmMinion = neighbour.SwarmMinion = false;
                sim.PacingFixtureKill(victim);  // 3 damage into 2 health: too little to spill
                Steps(sim, 0.1f);
                float before = neighbour.Health;
                UnitState big = sim.PacingFixtureRole(SwarmRole.None, sim.Champion.Position + new Vector2(4, 0.2f), 2);
                big.SwarmMinion = false;
                // a heavy blow: 200 damage into 2 health
                sim.WeaponDamageFixture(big, 200f, DamageElement.Physical);
                Steps(sim, 0.1f);
                Check(neighbour.Health < before - 50, "A killing blow with damage to spare spills into the next body");
            }

            // the Lantern Flare, the bell, and the sweep
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 5009);
                CombatSimulation sim = Fight(s, 8, 5009); Steps(sim, 12);
                int near = sim.Units.Count(u => u.Alive && u.Faction == Faction.Enemy && u.SwarmMinion && Mathf.Abs(u.Position.x - sim.Champion.Position.x) < 9 && Mathf.Abs(u.Position.y - sim.Champion.Position.y) < 5);
                sim.PacingFixtureLoot(LootKind.Flare, sim.Champion.Position, 1);
                bool flared = false;
                for (int tick = 0; tick < 120 && !flared; tick++) { sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(1f / 60, Vector2.zero); flared = sim.Events.Any(e => e.Kind == CombatEventKind.Flare); }
                int left = sim.Units.Count(u => u.Alive && u.Faction == Faction.Enemy && u.SwarmMinion && Mathf.Abs(u.Position.x - sim.Champion.Position.x) < 9 && Mathf.Abs(u.Position.y - sim.Champion.Position.y) < 5);
                Check(flared && (near == 0 || left < near), "The Lantern Flare burns the swarm on screen away (" + near + " -> " + left + ")");

                for (int i = 0; i < 30; i++) sim.PacingFixtureLoot(LootKind.Coin, sim.Champion.Position + new Vector2(Mathf.Cos(i), Mathf.Sin(i)) * 9, 1);
                Steps(sim, 0.4f);
                int gold = sim.Gold;
                sim.PacingFixtureLoot(LootKind.Bell, sim.Champion.Position, 1);
                Steps(sim, 2.5f);
                Check(sim.Gold - gold >= 30, "The bell calls every coin home (" + (sim.Gold - gold) + ")");

                for (int i = 0; i < 20; i++) sim.PacingFixtureLoot(LootKind.Coin, sim.Champion.Position + new Vector2(Mathf.Cos(i), Mathf.Sin(i)) * 8, 2);
                Steps(sim, 0.4f);
                int wallet = s.State.Gold, earned = sim.Gold, floor = sim.Pickups.Where(p => p.Kind == LootKind.Coin).Sum(p => p.Value);
                sim.PacingFixtureWin();
                s.Settle(sim);
                Check(s.State.Phase == SessionPhase.Shop && s.State.Gold >= wallet + earned + floor && sim.SweptGold >= floor && floor >= 40, "Winning the wave sweeps the floor's coins into the wallet");
            }
        }

        private static bool RecruitEligible(RunSession session, UnitDefinition unit, WeaponDefinition weapon)
        {
            var method = typeof(RunSession).GetMethod("RecruitWeaponEligible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            return method != null && (bool)method.Invoke(session, new object[] { unit.Id, weapon });
        }
    }
}
