using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11ThemeChecks
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K11 theme: " + caption); notes.Add("PASS " + caption); };
            require(catalog.HeroRevision > 0, "Themed runtime is enabled");
            var pools = new List<string>(); var coverage = new HashSet<string>();
            foreach (UnitDefinition hero in catalog.Champions)
            {
                UnitDefinition[] pool = catalog.Companions.Where(unit => K11HeroRules.Eligible(hero, unit)).ToArray();
                require(pool.Length >= 15 && pool.Length < catalog.Companions.Length, "Distinct broad themed pool " + hero.Id + " = " + pool.Length);
                pools.Add(string.Join(",", pool.Select(unit => unit.Id))); foreach (UnitDefinition unit in pool) coverage.Add(unit.Id);
                using (var session = new RunSession(catalog))
                {
                    session.Start(hero.Id, 77107); require(K11HeroRules.Eligible(hero, catalog.Unit(session.State.Roster[0].UnitId)), "Starter belongs to selected theme " + hero.Id);
                    session.State.Phase = SessionPhase.Shop; session.State.Wave = 10;
                    for (int roll = 0; roll < 24; roll++)
                    {
                        session.State.Gold = int.MaxValue; require(session.Reroll(), "Seeded themed reroll " + hero.Id + "/" + roll);
                        require(session.State.Offers.Where(offer => offer.Kind == OfferKind.Companion).All(offer => K11HeroRules.Eligible(hero, catalog.Unit(offer.Id))), "Shop recruits stay within theme " + hero.Id + "/" + roll);
                    }
                    using (var restored = new RunSession(catalog))
                    {
                        restored.Restore(session.State); session.State.Gold = restored.State.Gold = int.MaxValue;
                        require(session.Reroll() && restored.Reroll() && JsonUtility.ToJson(session.State.Offers[0]) == JsonUtility.ToJson(restored.State.Offers[0]) && session.State.ShopRandom == restored.State.ShopRandom, "Theme/RNG survive save restore " + hero.Id);
                    }
                }
                using (var session = new RunSession(catalog))
                {
                    session.Start(hero.Id, 1107); session.State.Wave = 2; CheckpointCatalog combat = session.PrepareCombat();
                    combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 60; combat.PassiveHeal = 0;
                    var simulation = new CombatSimulation(combat); simulation.Start(1107);
                    foreach (UnitState unit in simulation.Units) unit.NextAttack = 100;
                    UnitState enemy = simulation.AddFixture(combat.MeleeEnemy, Faction.Enemy, new Vector2(2, 0));
                    enemy.Definition.Speed = 0; enemy.NextAttack = enemy.NextContact = 100; enemy.Health = 10000;
                    if (hero.HeroMechanic == HeroMechanic.LanternStride)
                    {
                        float before = enemy.Health;
                        for (int tick = 0; tick < 240; tick++) simulation.Step(1f / 60, tick / 30 % 2 == 0 ? Vector2.right : Vector2.left);
                        require(simulation.HeroActivations > 0, "Actual movement triggers Lantern stride");
                        require(simulation.Contributions.Any(entry => entry.Key.Contains("hero-LanternStride") && entry.Damage > 0) || enemy.Health < before, "Lantern pulse deals actual damage, not only VFX");
                    }
                    else if (hero.HeroMechanic == HeroMechanic.ParcelExpress)
                    {
                        Vector2 direction = simulation.Champion.AttackDirection;
                        simulation.LootFixture(LootKind.Coin, simulation.Champion.Position, 6);
                        for (int tick = 0; tick < 90 && simulation.HeroActivations == 0; tick++) simulation.Step(1f / 60, Vector2.zero);
                        require(simulation.HeroActivations == 1 && simulation.Projectiles.Count > 0, "Collected gold triggers actual express projectiles");
                        require(simulation.Champion.AttackDirection == direction, "Express volley does not retarget a pending base attack");
                    }
                    else
                    {
                        for (int tick = 0; tick < 115; tick++) simulation.Step(1f / 60, Vector2.zero);
                        for (int projectile = 0; projectile < 5; projectile++) simulation.ProjectileFixture(new Vector2(1, 1), Vector2.zero, 1, Faction.Enemy);
                        for (int tick = 0; tick < 10; tick++) simulation.Step(1f / 60, Vector2.zero);
                        require(simulation.HeroActivations == 1 && enemy.ChillUntil > simulation.Elapsed, "Stationary dream breath chills actual enemy");
                        require(simulation.Projectiles.Count(projectile => projectile.Faction == Faction.Enemy) == 2, "Dream breath removes exactly three of five unexpired hostile projectiles");
                    }
                    int activations = simulation.HeroActivations; float charge = simulation.HeroCharge;
                    simulation.Pause(true); simulation.Step(1f / 60, Vector2.right);
                    require(simulation.HeroActivations == activations && simulation.HeroCharge == charge, "Pause does not charge or trigger hero mechanic " + hero.Id);
                    simulation.Pause(false); simulation.EnterRest(); simulation.StepRest(1f / 60, Vector2.right);
                    require(simulation.HeroActivations == activations && simulation.HeroCharge == charge, "Shop/rest does not trigger hero mechanic " + hero.Id);
                }
            }
            require(pools.Distinct().Count() == 3 && coverage.Count == catalog.Companions.Length, "All three pools differ and cover every species across protagonists");
            Directory.CreateDirectory("PreviewExports/K11"); File.WriteAllLines("PreviewExports/K11/Theme_Checks.txt", notes);
            Debug.Log("K11_THEME_CHECKS_PASSED " + notes.Count);
        }
    }
}
