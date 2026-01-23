using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K17CampaignDiagnostics
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var runs = new List<string> { "hero,seed,outcome,wave,kills,allies,gold,seconds" };
            var economy = new List<string> { "hero,seed,wave,combat_gold,wallet_before,wallet_after,mean_offer_price,ally_purchases" };
            foreach (UnitDefinition hero in catalog.Champions)
            {
                const int seed = 171077;
                using var session = new RunSession(catalog); session.Start(hero.Id, seed);
                string outcome = "TIMEOUT";
                for (int wave = 0; wave < catalog.NormalWaves; wave++)
                {
                    var simulation = new CombatSimulation(session.PrepareCombat()); simulation.Start(seed);
                    int limit = Mathf.CeilToInt((catalog.WaveSeconds + 180) * 60);
                    for (int tick = 0; tick < limit && simulation.Phase == RunPhase.Combat; tick++)
                        simulation.Step(1f / 60, Steer(simulation));
                    if (simulation.Phase == RunPhase.Combat) break;
                    session.Settle(simulation);
                    if (session.State.Phase == SessionPhase.Defeat) { outcome = "DEFEAT"; break; }
                    if (session.State.Phase == SessionPhase.Victory) { outcome = "VICTORY"; break; }
                    int wallet = session.State.Gold;
                    float price = session.State.Offers.Count == 0 ? 0 : (float)session.State.Offers.Average(offer => offer.Price);
                    K13MovementPolicy.Purchase(session, catalog);
                    if (session.State.NewRecruitsThisWave > 3) throw new InvalidOperationException("Campaign bypassed companion quota");
                    economy.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6:0.0},{7}", hero.Id, seed, session.State.Wave, simulation.Gold, wallet, session.State.Gold, price, session.State.NewRecruitsThisWave));
                    session.ContinueWave();
                }
                runs.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7:0.0}", hero.Id, seed, outcome, session.State.Wave, session.State.TotalKills, session.State.Roster.Count, session.State.Gold, session.State.CombatSeconds));
                Debug.Log("K17_ROUTE " + runs[runs.Count - 1]);
            }
            Directory.CreateDirectory("PreviewExports/K17");
            File.WriteAllLines("PreviewExports/K17/Campaign.csv", runs);
            File.WriteAllLines("PreviewExports/K17/Economy.csv", economy);
            Debug.Log("K17_CAMPAIGN_REPORT_FINISHED; outcomes are bot observations, not human balance acceptance");
        }
        private static Vector2 Steer(CombatSimulation simulation)
        {
            if (simulation.Champion.Definition.HeroMechanic == HeroMechanic.RootedOracle) return Vector2.zero;
            Vector2 movement = K13MovementPolicy.Steer(simulation);
            if (simulation.Champion.Definition.HeroMechanic != HeroMechanic.SoloMaw) return movement;
            UnitState nearest = null; float distance = float.PositiveInfinity;
            foreach (UnitState enemy in simulation.Units)
            {
                float candidate = (enemy.Position - simulation.Champion.Position).sqrMagnitude;
                if (!enemy.Alive || enemy.Faction != Faction.Enemy || candidate >= distance) continue;
                nearest = enemy; distance = candidate;
            }
            if (nearest == null) return movement;
            Vector2 direction = (nearest.Position - simulation.Champion.Position).normalized;
            float reach = simulation.Champion.Definition.Weapon.Range + nearest.Definition.Radius;
            if (distance > reach * reach * 0.7f) return direction;
            float collision = simulation.Champion.Definition.Radius + nearest.Definition.Radius + 0.3f;
            return distance < collision * collision ? -direction : new Vector2(-direction.y, direction.x);
        }
    }
}
