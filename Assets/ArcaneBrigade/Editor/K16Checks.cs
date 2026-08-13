using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K16Checks
    {
        public static void Run()
        {
            var notes = new List<string>(); Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K16 " + caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            foreach (string id in K16Builder.Units)
            {
                UnitDefinition definition = AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/ArcaneBrigade/Data/Run/" + id + ".asset"); AuthoredMotionSet art = definition.AuthoredMotion;
                require(art != null, "Authored motion " + id);
                Sprite[] all = art.Idle.Concat(art.Walk).Concat(art.Windup).Concat(art.Release).Concat(art.Recovery).ToArray();
                bool redo = art.name.EndsWith("RedoMotion", StringComparison.Ordinal);
                if (redo) require(art.Idle.Length == 4 && art.Walk.Length == 4 && art.Windup.Length == 3 && art.Release.Length == 2 && art.Recovery.Length == 3, "Exact RedoMotion pose layout " + id);
                int expected = redo ? 15 : (definition.IsBoss && AssetDatabase.GetAssetPath(definition.Body).Contains("/K21/") || !AssetDatabase.GetAssetPath(art.Idle[0]).Contains("/K16/") ? 8 : 12); // RedoMotion's final recovery reuses idle; Act I actors carry 8 poses
                require(all.Distinct().Count() == expected, expected + " separate drawings " + id + " (has " + all.Distinct().Count() + ")");
                require(all.All(frame => frame != null && frame.pivot.y == 0), "Ground-anchored pose pivots " + id);
                require(art.ReferenceHeight > 0 && definition.Body == art.Idle[0], "Stable idle height and portrait " + id);
                if (!AssetDatabase.GetAssetPath(art.Idle[0]).Contains("/K16/")) continue; // an Act I repainted canon actor keeps its own pose layout
                var unit = new UnitState { Definition = definition, Id = 2, Position = Vector2.zero, PreviousPosition = Vector2.zero, AttackStarted = -100, LastHit = -100 };
                require(art.Select(unit, 0.5f, true) == art.Idle[0], "Reduced motion steady portrait " + id);
                unit.PendingAttack = true; unit.AttackStarted = 0; unit.AttackWindup = 0.4f;
                require(art.Select(unit, 0.1f, false) == art.Windup[0] && art.Select(unit, 0.3f, false) == art.Windup[1], "Both anticipation poses " + id);
                unit.PendingAttack = false;
                require(art.Select(unit, 0.41f, false) == art.Release[0], "True release pose " + id);
                require(art.Select(unit, 0.4f + definition.Weapon.Recovery * 0.9f, false) == art.Recovery[1], "True recovery pose " + id);
            }
            using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon("TwinSabres"), 1, 2))
            {
                UnitState enemy = sample.Simulation.Units.First(unit => unit.Faction == Faction.Enemy);
                sample.Ally.Position = Vector2.zero; enemy.Position = new Vector2(2, 0);
                Vector2 point = MeleeTactics.Engage(sample.Ally, enemy);
                require(point.x < enemy.Position.x, "Engagement chooses near side, not far-side orbit");
                sample.Ally.Position = point;
                require(Vector2.Distance(MeleeTactics.Engage(sample.Ally, enemy), point) < 0.001f, "Settled attack slot does not keep rotating around its target");
                sample.Ally.Position = Vector2.zero;
                sample.Ally.PendingAttack = true; sample.Ally.AttackDirection = Vector2.right; sample.Ally.AttackStarted = 0; sample.Ally.ImpactTime = 0.3f;
                enemy.Position = new Vector2(1, 1); MeleeTactics.Aim(sample.Ally, enemy, 0.02f);
                require(Vector2.Angle(Vector2.right, sample.Ally.AttackDirection) > 0 && Vector2.Angle(Vector2.right, sample.Ally.AttackDirection) <= 6.01f, "Windup aim follows target with a bounded turn");
                sample.Ally.PendingAttack = false; sample.Ally.Definition.Speed = 5;
                enemy.Position = enemy.PreviousPosition = new Vector2(1, 0); sample.Run(0.05f);
                int locked = sample.Ally.AttackTargetId; float impact = sample.Ally.ImpactTime;
                require(sample.Ally.PendingAttack, "Actual melee windup starts");
                UnitState defeated = sample.Simulation.Units.First(unit => unit.Id == locked);
                sample.Simulation.WeaponDamageFixture(defeated, defeated.Health + 100, DamageElement.Physical);
                sample.Simulation.Step((impact - sample.Simulation.Elapsed) * 0.5f, Vector2.zero);
                require(sample.Ally.AttackTargetId != locked && sample.Ally.ImpactTime == impact, "Dead target reacquired without restarting windup");
            }
            foreach (string id in new[] { "ClockworkYoYo", "NeedleSinger", "DreamProjector", "SpringKnocker", "RibbonWheel", "ScrapMagnet" })
            for (int tier = 1; tier <= 3; tier++)
                using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon(id), tier, 8))
                {
                    bool linked = false, controlled = false, primaryHammer = false;
                    for (int tick = 0; tick < 100; tick++)
                    {
                        sample.Simulation.Step(0.05f, Vector2.zero);
                        linked |= sample.Simulation.ExpandedFields.Any(field => field.Kind == ExpandedFieldKind.SewingLink);
                        controlled |= sample.Simulation.Units.Any(unit => unit.Faction == Faction.Enemy && unit.ChillUntil > sample.Simulation.Elapsed);
                        foreach (CombatEvent value in sample.Simulation.Events)
                        {
                            if (value.Kind == CombatEventKind.Hit && value.Cause.ItemId == "sewing-stitch") require(value.Cause.Depth > 0, "Stitch is secondary/nonrecursive");
                            if (value.Kind == CombatEventKind.Hit && id == "SpringKnocker") primaryHammer |= value.Cause.Depth == 0;
                        }
                    }
                    require(id != "NeedleSinger" || linked == (tier >= 2), "Sewing tier-gated thread " + id + " " + tier);
                    require(id != "ClockworkYoYo" || controlled, "Yo-yo control " + id + " " + tier);
                    require(id != "DreamProjector" || controlled == (tier >= 2), "Projector tier-gated convergence " + id + " " + tier);
                    require(id != "SpringKnocker" || primaryHammer, "Hammer impact retains primary attribution " + id + " " + tier);
                    require(!string.IsNullOrEmpty(ExpandedDescription.Describe(catalog.Weapon(id), tier)), "Short actual mechanic description " + id + " " + tier);
                }
            using (var sample = new K14BalanceDiagnostics.Sample(catalog, catalog.Weapon("SpringKnocker"), 1, 1))
            {
                sample.Ally.Definition.CriticalChance = 1;
                bool critical = false;
                for (int tick = 0; tick < 30; tick++) { sample.Simulation.Step(0.05f, Vector2.zero); critical |= sample.Simulation.Events.Any(value => value.Kind == CombatEventKind.Hit && value.Faction == Faction.Enemy && value.Critical); }
                require(critical, "Heavy splash carries its critical flag");
            }
            Directory.CreateDirectory("PreviewExports/K16"); File.WriteAllLines("PreviewExports/K16/Checks.txt", notes); Debug.Log("K16_CHECKS_PASSED " + notes.Count);
        }
    }
}
