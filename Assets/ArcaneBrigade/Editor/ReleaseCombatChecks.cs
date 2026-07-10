using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ReleaseCombatChecks
    {
        public static void Run()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var notes = new List<string>();
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (string mode in new[] { "hostile collateral", "friendly area", "arsenal fizz", "expanded melee", "echo blade" })
            using (var run = new RunSession(catalog))
            {
                bool hostile = mode == "hostile collateral";
                run.Start("LanternKnight", 271026);
                var arena = run.PrepareCombat();
                arena.ExpandedContent = true; arena.EnemyBudget = 0; arena.Boss = null;
                var sim = new CombatSimulation(arena); sim.Start(271026);
                var definition = UnityEngine.Object.Instantiate(arena.MeleeEnemy);
                definition.Id = "ReleaseSplitter"; definition.Health = 1000;
                definition.IsBoss = false; definition.Behavior = EnemyBehavior.Splitter;
                try
                {
                    var first = sim.AddFixture(definition, Faction.Enemy, Vector2.right);
                    var second = sim.AddFixture(definition, Faction.Enemy, Vector2.right * 1.2f);
                    first.Health = second.Health = 1;
                    var field = new ExpandedField
                    {
                        Kind = hostile ? ExpandedFieldKind.FallingProp : ExpandedFieldKind.Fuse,
                        Position = Vector2.right, Radius = 3, At = 0, Until = 2, Damage = 10000,
                        Source = hostile ? null : sim.Champion,
                        Cause = new CombatCause(1, hostile ? 0 : sim.Champion.Id, "release-splitter", 0)
                    };
                    if (hostile)
                        typeof(CombatSimulation).GetMethod("ProcessHostileCollateral", flags).Invoke(sim, new object[] { field });
                    else if (mode == "expanded melee" || mode == "echo blade")
                    {
                        var weapon = sim.Champion.Definition.Weapon;
                        weapon.Family = ExpandedWeapon.Hammer;
                        weapon.Mechanic = mode == "echo blade" ? CarnivalWeapon.EchoBlade : CarnivalWeapon.None;
                        weapon.Arsenal = ArsenalWeapon.None; weapon.Range = 3; weapon.ConeDot = -1;
                        sim.Champion.AttackDirection = Vector2.right;
                        typeof(CombatSimulation).GetMethod("ResolveExpandedAttack", flags).Invoke(sim,
                            new object[] { sim.Champion, 10000f, false, field.Cause });
                    }
                    else if (mode == "arsenal fizz")
                    {
                        var props = (List<ArsenalProp>)typeof(CombatSimulation).GetField("arsenalProps", flags).GetValue(sim);
                        props.Add(new ArsenalProp { Kind = ArsenalPropKind.Fizz, Source = sim.Champion,
                            Weapon = sim.Champion.Definition.Weapon, Position = Vector2.right, Radius = 3,
                            Damage = 10000, Until = 2, Cause = field.Cause });
                        typeof(CombatSimulation).GetMethod("ProcessArsenal", flags).Invoke(sim, new object[] { .05f });
                    }
                    else
                    {
                        var fields = (List<ExpandedField>)typeof(CombatSimulation).GetField("fields", flags).GetValue(sim);
                        fields.Add(field);
                        typeof(CombatSimulation).GetMethod("ProcessExpansion", flags).Invoke(sim, new object[] { .05f });
                    }
                    var halves = sim.Units.Where(u => u.IsHalf && u.Definition.Id == definition.Id).ToArray();
                    if (first.Alive || second.Alive || halves.Length != 4 || halves.Any(u => !u.Alive || u.Health != u.Definition.Health))
                        throw new InvalidOperationException("Splitting during field damage must preserve both new halves for the next resolution: " + hostile);
                    if (hostile && (field.CollateralVictims.Count != 2 || halves.Any(u => field.CollateralVictims.Contains(u.Id))))
                        throw new InvalidOperationException("New halves must not be hit by the collateral that created them");
                    notes.Add("PASS " + mode + " kills two splitters, spawns four unharmed halves without invalidating iteration");
                    foreach (var clone in halves.Select(u => u.Definition).Distinct()) UnityEngine.Object.DestroyImmediate(clone);
                }
                finally { UnityEngine.Object.DestroyImmediate(definition); }
            }
            Directory.CreateDirectory("PreviewExports/ReleaseBalance/CombatChecks");
            File.WriteAllLines("PreviewExports/ReleaseBalance/CombatChecks/Checks.txt", notes);
            Debug.Log("RELEASE_COMBAT_CHECKS PASS " + notes.Count);
        }
    }
}
