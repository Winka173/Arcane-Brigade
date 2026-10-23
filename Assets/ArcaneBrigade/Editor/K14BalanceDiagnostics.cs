using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K14BalanceDiagnostics
    {
        public sealed class Sample : IDisposable
        {
            public readonly RunSession Session;
            public readonly CombatSimulation Simulation;
            public readonly UnitState Ally;
            private readonly List<UnitDefinition> enemies = new List<UnitDefinition>();
            public Sample(RunCatalog catalog, WeaponDefinition weapon, int tier, int count)
            {
                Session = new RunSession(catalog); Session.Start("LanternKnight", 141077);
                Session.State.Wave = 2; Session.State.Roster.Clear();
                string species = weapon.SpecialOnly ? "ReelOwl" : "TeapotPilgrim";
                Session.State.Roster.Add(new RosterEntry { UnitId = species, WeaponId = weapon.Id, Tier = tier });
                CheckpointCatalog combat = Session.PrepareCombat();
                combat.EnemyBudget = 0; combat.WaveDuration = 1000; combat.Boss = null;
                combat.EnableArenaToys = combat.EnableBossArenaPhase = combat.EnableHeroMechanics = false;
                combat.Procs = new ActiveProc[0]; combat.PassiveHeal = 0;
                Simulation = new CombatSimulation(combat); Simulation.Start(141077);
                Simulation.Champion.NextAttack = float.PositiveInfinity;
                Ally = Simulation.Units.First(unit => unit.Faction == Faction.Companion);
                Ally.Position = Ally.PreviousPosition = Vector2.zero; Ally.Definition.Speed = 0;
                Ally.Definition.Passive = PassiveKind.None;
                for (int index = 0; index < count; index++)
                {
                    UnitDefinition enemy = UnityEngine.Object.Instantiate(combat.MeleeEnemy);
                    enemies.Add(enemy); enemy.Health = 1000000; enemy.Speed = 0; enemy.Passive = PassiveKind.None; enemy.Behavior = EnemyBehavior.Standard; enemy.KnockbackResistance = 0;
                    Vector2 position = index == 0 ? new Vector2(1.2f * MeleeBalance.Range / 1.35f, 0) /* within the shortened melee reach (19/10/2026) */ : new Vector2(1.4f + index % 3 * 0.35f, (index / 3 - 1) * 0.55f);
                    UnitState target = Simulation.AddFixture(enemy, Faction.Enemy, position);
                    target.NextAttack = target.NextContact = float.PositiveInfinity;
                }
            }
            public float Run(float duration, bool moving = false)
            {
                float damage = 0;
                for (int tick = 0; tick < Mathf.RoundToInt(duration * 20); tick++)
                {
                    if (moving)
                    {
                        foreach (UnitState enemy in Simulation.Units)
                            if (enemy.Faction == Faction.Enemy) enemy.Position += Vector2.up * Mathf.Sin(tick * 0.1f) * 0.025f;
                    }
                    Simulation.Step(0.05f, Vector2.zero);
                    foreach (CombatEvent value in Simulation.Events)
                        if (value.Kind == CombatEventKind.Hit && value.Faction == Faction.Enemy && value.Cause.SourceUnitId == Ally.Id) damage += value.Value;
                }
                return damage / duration;
            }
            public void Dispose()
            {
                Session.Dispose();
                foreach (UnitDefinition enemy in enemies) UnityEngine.Object.DestroyImmediate(enemy);
            }
        }
        public static void Measure()
        {
            MeasureInto("PreviewExports/K14");
        }
        public static void MeasureInto(string directory)
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var rows = new List<string> { "weapon,kind,tier,targets,moving,dps,range,cooldown,mechanic" };
            foreach (WeaponDefinition weapon in catalog.Weapons)
            for (int tier = 1; tier <= 3; tier++)
            foreach (int count in new[] { 1, 8 })
            foreach (bool moving in new[] { false, true })
                using (var sample = new Sample(catalog, weapon, tier, count))
                {
                    float dps = sample.Run(12, moving);
                    if (!float.IsFinite(dps)) throw new InvalidOperationException("Invalid damage for " + weapon.Id);
                    rows.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5:0.000},{6:0.00},{7:0.00},{8}", weapon.Id, weapon.Kind, tier, count, moving, dps, sample.Ally.Definition.Weapon.Range, sample.Ally.Definition.Weapon.Cooldown, weapon.Mechanic));
                }
            Directory.CreateDirectory(directory); File.WriteAllLines(Path.Combine(directory, "Weapon_Balance.csv"), rows);
            var recruits = new List<string> { "ally,tier,price,currency,special,theme_locked,passive,passive_strength,scoped_modifiers" };
            foreach (UnitDefinition unit in catalog.Companions)
            for (int tier = 1; tier <= 3; tier++)
            {
                string modifiers = string.Join(";", unit.CompanionModifiers.Select(modifier => modifier.Scope + ":" + modifier.Effect + "=" + (modifier.Value * UnitDefinition.ModifierScale(tier)).ToString("0.####", CultureInfo.InvariantCulture)));
                recruits.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7:0.####},{8}", unit.Id, tier, unit.SpecialRecruit ? 4 : unit.Price, unit.SpecialRecruit ? "Eggs" : "Gold", unit.SpecialRecruit, unit.RecruitThemeLocked, unit.Passive, unit.PassiveStrength * Mathf.Pow(catalog.TierSupportMultiplier, tier - 1), modifiers));
            }
            File.WriteAllLines(Path.Combine(directory, "Recruit_Balance.csv"), recruits);
            Debug.Log("K14_WEAPON_MEASUREMENTS " + (rows.Count - 1) + " controlled immortal-target samples; not human balance approval");
        }
    }
}
