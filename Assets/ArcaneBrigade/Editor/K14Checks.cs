using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K14Checks
    {
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K14 " + caption); notes.Add("PASS " + caption); };
            WeaponDefinition[] weapons = catalog.Weapons.Where(weapon => weapon.Mechanic > CarnivalWeapon.None && weapon.Mechanic <= CarnivalWeapon.RibbonWheel).ToArray();
            require(weapons.Length == 6 && weapons.Count(weapon => weapon.Kind == WeaponKind.Melee) == 3, "Six distinct weapons balanced three melee / three ranged");
            foreach (WeaponDefinition weapon in weapons)
            {
                require(weapon.Sprite != null && weapon.Windup > 0 && weapon.Recovery > 0, "Authored art and motion " + weapon.Id);
                using (var sample = new K14BalanceDiagnostics.Sample(catalog, weapon, 1, 8))
                {
                    require(sample.Run(5) > 0, "Actual combat damage " + weapon.Id);
                    require(sample.Simulation.Champion.Alive, "No champion damage in isolated weapon fixture " + weapon.Id);
                }
                K8Motion.Pose normal = K8Motion.Attack(weapon, weapon.Windup * 0.8f, weapon.Windup, false);
                K8Motion.Pose reduced = K8Motion.Attack(weapon, weapon.Windup * 0.8f, weapon.Windup, true);
                require(normal.Angle != 0 || normal.Recoil != 0, "Weapon has distinct windup " + weapon.Id);
                require(reduced.Angle == 0 && reduced.Recoil == 0, "Reduced motion " + weapon.Id);
            }
            using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon("SpringKnocker"), 1, 1))
            {
                UnitState target = sample.Simulation.Units.First(unit => unit.Faction == Faction.Enemy);
                Vector2 before = target.Position;
                sample.Run(0.8f);
                require(target.Position.x > before.x + 0.2f, "Spring hammer impact applies weapon knockback");
            }
            using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon("SpringKnocker"), 1, 1))
            {
                UnitState target = sample.Simulation.Units.First(unit => unit.Faction == Faction.Enemy);
                target.Definition.KnockbackResistance = 1;
                Vector2 before = target.Position;
                sample.Run(0.8f);
                require(target.Position == before, "Spring hammer respects full knockback resistance");
            }
            foreach (string id in new[] { "CoilKettle", "ThimbleMoth", "ReelOwl" })
            {
                UnitDefinition unit = catalog.Unit(id);
                require(unit != null && unit.AnimationFrames.Length == 4 && unit.AnimationFrames.Distinct().Count() == 4, "Four painted poses " + id);
                require(unit.CompanionModifiers.Any(modifier => modifier.Value > 0) && unit.CompanionModifiers.Any(modifier => modifier.Value < 0), "Recruit has bonus and tradeoff " + id);
                require(catalog.Champions.Any(champion => K11HeroRules.Eligible(champion, unit)), "Recruit matches champion theme " + id);
                require(unit.RecruitThemeLocked, "New recruit keeps its theme in the dream pool " + id);
            }
            using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon("TwinSabres"), 1, 8))
            {
                sample.Ally.Definition.Speed = 5;
                UnitState second = sample.Simulation.AddCompanion(sample.Ally.Definition);
                second.Position = second.PreviousPosition = sample.Ally.Position;
                sample.Run(1);
                require(second.TacticalTargetId != sample.Ally.TacticalTargetId, "Companions distribute enemy claims");
                require(Vector2.Distance(second.Position, sample.Ally.Position) > 0.15f, "Companions separate instead of stacking");
                require(Vector2.Distance(second.Position, sample.Simulation.Champion.Position) <= CompanionMotion.Leash(second, sample.Session.CombatCatalog) + 0.01f, "Pursuit stays within leash");
                Vector2 before = second.Position;
                sample.Simulation.Pause(true); sample.Simulation.Step(0.1f, Vector2.one);
                require(second.Position == before, "Pause freezes tactics");
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 14000);
                session.State.Wave = 10; session.State.Roster.Clear(); // Act I boss waves are 5, 10, 15, 20
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.WaveDuration = 1000; combat.StoryCompanions = new UnitDefinition[0];
                combat.Champion.Health = 10000; combat.Boss.Speed = 0;
                combat.EnableArenaToys = combat.EnableBossArenaPhase = false;
                combat.EnableHeroIdentities = false; // the boss's recovery is measured without the hero's kit
                var simulation = new CombatSimulation(combat); simulation.Start(14000);
                simulation.Champion.NextAttack = float.PositiveInfinity;
                for (int tick = 0; tick < 270; tick++) simulation.Step(0.05f, Vector2.zero);
                require(simulation.Boss.BossVolley >= 3, "Boss completes a readable three-volley sequence");
                require(simulation.Boss.BossRecoveryUntil > simulation.Elapsed, "Third volley opens a longer recovery window");
                float previous = simulation.Boss.Health;
                simulation.WeaponDamageFixture(simulation.Boss, 100, DamageElement.Physical);
                require(Mathf.Abs(previous - simulation.Boss.Health - 115) < 0.1f, "Recovery rewards counterattack by 15 percent");
            }
            float previousHealth = 0;
            for (int difficulty = 0; difficulty < 5; difficulty++)
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 14001, difficulty); session.State.Wave = 10;
                    float health = session.PrepareCombat().Boss.Health;
                    require(health > previousHealth, "Difficulty increases boss durability " + difficulty); previousHealth = health;
                }
            Directory.CreateDirectory("PreviewExports/K14"); File.WriteAllLines("PreviewExports/K14/Checks.txt", notes);
            Debug.Log("K14_CHECKS_PASSED " + notes.Count);
        }
    }
}
