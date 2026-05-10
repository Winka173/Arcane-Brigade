using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Repeatable single/crowd benchmark using the production simulation, including kits and awakenings.
    /// Targets respawn at fixed points, so kills can trigger mechanics without exhausting the test population.</summary>
    public static class HeroDpsRangeAudit
    {
        private static string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
        public static void Run() => Measure(false);
        public static void Final()
        {
            Measure(false);
            Measure(true);
            FriendlyReachChecks.Run();
        }
        private static void Measure(bool invested)
        {
            string label = Environment.GetCommandLineArgs().SkipWhile(x => x != "-audit-label").Skip(1).FirstOrDefault() ?? "Before";
            string root = "PreviewExports/HeroDpsRange/" + label + (invested ? "Invested" : ""); Directory.CreateDirectory(root);
            string heroFilter = Environment.GetCommandLineArgs().SkipWhile(x => x != "-dps-hero").Skip(1).FirstOrDefault();
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "hero\tapproach\twave\ttargets\tteam\tmode\theroDps\tteamDps\tteamBonusDps\thealing\tgold\tawakeningSeconds\tseconds\trange\thit\tcooldown" };
            foreach (var hero in catalog.Champions.Where(h => heroFilter == null || h.Id == heroFilter))
                foreach (int approach in new[] { 0, 1, 2 })
                    foreach (int wave in new[] { 10, 20 })
                        foreach (int count in new[] { 1, 8 })
                            foreach (bool team in new[] { false, true })
                                foreach (string mode in new[] { "stationary", "moving", "contact" })
                                {
                                    using var run = new RunSession(catalog); run.Start(hero.Id, 77138);
                                    run.State.Wave = wave; run.State.Gold = 60; run.State.HeroIdentity.Approach = approach;
                                    if (invested)
                                    {
                                        // Same stat investment for every hero, especially important for critical-hit and
                                        // negative-damage kits. These are fixture bonuses, not saved purchases or free live stats.
                                        run.State.OctoberGains.Add(new StatModifier { Effect = ItemEffect.Damage, Scope = ModifierScope.Team, Value = .5f });
                                        run.State.OctoberGains.Add(new StatModifier { Effect = ItemEffect.Crit, Scope = ModifierScope.Team, Value = .35f });
                                        run.State.OctoberGains.Add(new StatModifier { Effect = ItemEffect.AttackSpeed, Scope = ModifierScope.Team, Value = .25f });
                                    }
                                    run.State.Roster.Clear();
                                    if (team && hero.HeroMechanic != HeroMechanic.SoloMaw)
                                        foreach (string id in new[] { "JellySwordsman", "TeapotPilgrim", "BrassAcorn", "CandleSnail" })
                                        {
                                            var ally = catalog.Companions.FirstOrDefault(u => u.Id == id);
                                            if (ally == null) throw new Exception("Missing benchmark ally: " + id);
                                            run.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = ally.Weapon.Id, Tier = 2 });
                                        }
                                    if (hero.Id == "LoneJelly")
                                        foreach (var ally in catalog.Companions.Where(u => !u.GiftOnly && !u.SpecialRecruit && K11HeroRules.Eligible(hero, u)).Take(3)) run.State.AbsorbedSkills.Add(ally.Id);
                                    var arena = run.PrepareCombat();
                                    arena.Boss = null; arena.EnemyBudget = 0; arena.EliteCount = 0;
                                    arena.StoryCompanions = Array.Empty<UnitDefinition>(); arena.WaveDuration = 65;
                                    arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.EnableSpectacles = false;
                                    arena.ChestInterval = 10000; arena.GoldPerKill = 0; arena.EggDropChance = arena.HeartDropChance = 0;
                                    var dummy = ScriptableObject.CreateInstance<UnitDefinition>();
                                    var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
                                    dummy.Id = "DpsTarget"; dummy.DisplayName = "DPS target"; dummy.Weapon = weapon;
                                    dummy.Health = 1200; dummy.Radius = .25f; dummy.Speed = 0;
                                    dummy.ContactDamage = dummy.CollisionDamage = 0; dummy.KnockbackResistance = 1;
                                    weapon.Kind = WeaponKind.Melee; weapon.Damage = 0; weapon.Range = .1f; weapon.Cooldown = 1000;
                                    var sim = new CombatSimulation(arena); sim.Start(77138);
                                    var targets = new UnitState[count]; var positions = new Vector2[count];
                                    for (int i = 0; i < count; i++)
                                    {
                                        float angle = count == 1 ? 0 : (i - 3.5f) * 13 * Mathf.Deg2Rad;
                                        positions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (mode == "contact" ? .5f : i % 2 == 0 ? 1.5f : 2.4f);
                                        targets[i] = sim.AddFixture(dummy, Faction.Enemy, positions[i]);
                                    }
                                    float awake = 0;
                                    for (int tick = 0; tick < 1200; tick++)
                                    {
                                        if (sim.Phase != RunPhase.Combat) throw new Exception("Benchmark stopped: " + hero.Id + " " + sim.Phase);
                                        for (int i = 0; i < count; i++)
                                        {
                                            if (!targets[i].Alive) targets[i] = sim.AddFixture(dummy, Faction.Enemy, positions[i]);
                                            targets[i].Position = positions[i];
                                        }
                                        // Small real steps, then rest: tests movement and stillness mechanics without leaving melee reach.
                                        float phase = Mathf.Repeat(sim.Elapsed, 6);
                                        Vector2 move = mode == "moving" && phase < 4 ? Vector2.up * (Mathf.Repeat(phase, .4f) < .2f ? .5f : -.5f) : Vector2.zero;
                                        if (mode == "contact" && tick % 80 == 40)
                                        {
                                            // Equal, survivable pressure enables hurt/block/counter kits. This is an offence test, not a survival test.
                                            sim.Champion.Health = Mathf.Max(sim.Champion.Health, sim.Champion.Definition.Health * .75f);
                                            sim.DamageFixture(sim.Champion, Mathf.Max(1, sim.Champion.Definition.Health * .02f));
                                            sim.ProjectileFixture(sim.Champion.Position + Vector2.right * 3, Vector2.left * 2, 1, Faction.Enemy);
                                        }
                                        sim.Step(.05f, move);
                                        if (sim.HeroAwakening) awake += .05f;
                                    }
                                    float own = sim.Contributions.Where(c => c.UnitId == hero.Id).Sum(c => c.Damage);
                                    float all = sim.Contributions.Sum(c => c.Damage);
                                    if (!float.IsFinite(own) || !float.IsFinite(all)) throw new Exception("Non-finite damage: " + hero.Id);
                                    rows.Add(string.Join("\t", hero.Id, approach, wave, count, team, mode, F(own / sim.Elapsed), F(all / sim.Elapsed), F((sim.HeroKitTeamBonusDamage + sim.HeroIdentityBonusDamage) / sim.Elapsed), F(sim.Contributions.Sum(c => c.Healing)), F(sim.HeroIdentityGold), F(awake), F(sim.Elapsed), F(arena.Champion.Weapon.Range), F(arena.Champion.Weapon.Damage), F(arena.Champion.Weapon.Cooldown)));
                                    if ((rows.Count - 1) % 72 == 0) { File.WriteAllLines(root + "/Heroes.tsv", rows); Debug.Log("DPS progress " + hero.Id); }
                                    UnityEngine.Object.DestroyImmediate(dummy); UnityEngine.Object.DestroyImmediate(weapon);
                                }
            File.WriteAllLines(root + "/Heroes.tsv", rows);
            Ranges(catalog, root);
            Debug.Log("HERO_DPS_RANGE_COMPLETE " + label + " samples=" + (rows.Count - 1));
        }
        private static void Ranges(RunCatalog catalog, string root)
        {
            var rows = new List<string> { "role\tid\tweapon\tkind\twave\ttier\trange\tflightDistance\tmuzzle" };
            foreach (int wave in new[] { 1, 10, 20 })
            {
                foreach (var hero in catalog.Champions)
                {
                    using var run = new RunSession(catalog); run.Start(hero.Id, 1); run.State.Wave = wave;
                    var u = run.PrepareCombat().Champion; var w = u.Weapon;
                    rows.Add(string.Join("\t", "hero", hero.Id, w.Id, w.Kind, wave, 1, F(w.Range), F(w.ProjectileSpeed * w.ProjectileLifetime), F(w.MuzzleDistance)));
                }
                foreach (var weapon in catalog.Weapons)
                    foreach (int tier in new[] { 1, 2, 3 })
                    {
                        using var run = new RunSession(catalog); run.Start("LanternKnight", 1); run.State.Wave = wave; run.State.Roster.Clear();
                        run.State.Roster.Add(new RosterEntry { UnitId = "JellySwordsman", WeaponId = weapon.Id, Tier = tier });
                        var w = run.PrepareCombat().StartingCompanions.First(u => u.Id == "JellySwordsman").Weapon;
                        rows.Add(string.Join("\t", "companion", "JellySwordsman", w.Id, w.Kind, wave, tier, F(w.Range), F(w.ProjectileSpeed * w.ProjectileLifetime), F(w.MuzzleDistance)));
                    }
            }
            File.WriteAllLines(root + "/Ranges.tsv", rows);
        }
    }
}
