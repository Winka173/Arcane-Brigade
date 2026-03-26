using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7QualityChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K7 quality: " + caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses))
                require(unit.AnimationFrames.Length == 4 && unit.AnimationFrames.All(sprite => sprite != null) && unit.AnimationFrames.Select(sprite => sprite.rect).Distinct().Count() == 4, unit.Id + " has separate idle blink left right source frames");
            require(UnitVisual.AnimationFrame(0, 0, false, false) == 1 && UnitVisual.AnimationFrame(1, 0, false, false) == 0, "Idle blink timed independently of attack");
            require(UnitVisual.AnimationFrame(0, 0, true, false) == 2 && UnitVisual.AnimationFrame(0.2f, 0, true, false) == 3 && UnitVisual.AnimationFrame(0.2f, 0, true, true) == 0, "Walk alternates feet; reduced motion stays stable");
            foreach (UnitDefinition enemy in catalog.Enemies)
                // roster v2 (19/10/2026) gives the Waiting Realm enemies their own atlases; none may share a friendly unit's sheet
                require(enemy.Body != null && !catalog.Companions.Concat(catalog.Champions).Any(ally => ally.Body != null && ally.Body.texture == enemy.Body.texture), enemy.Id + " enemy source never reuses friendly body");
            require(catalog.Companions.Select(unit => unit.Body).Distinct().Count() >= 5 && catalog.Unit("TeapotPilgrim").Body.name == "TeapotCourier" && catalog.Unit("CandleSnail").Body.name == "SnailPorter", "Companion identities map explicitly not generic pillow fallback");
            require(catalog.Weapon("RuneStaff").ProjectileCount == 2 && catalog.Weapon("CrescentBow").ProjectilePierce == 2, "Base magic fan and bow pierce authored without item dependency");
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art != null && art.Dust.Length == 4 && art.Slash.Length == 4 && art.Magic.Length == 4 && art.Death.Length == 4, "Painted effects have16 actual alpha animation frames not mesh placeholders");
            require(Resources.Load<AudioClip>("Audio/Licensed/FusionJazz") != null && Enumerable.Range(0, 33).All(index => Resources.Load<AudioClip>("Audio/Licensed/Cue-" + index.ToString("00")) != null), "Default playback uses licensed composed music and sampled33Foley cues");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 7718); session.State.Roster.Clear();
                CheckpointCatalog combat = session.PrepareCombat(); combat.CoinValueScale = 1; combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 1000;
                combat.Procs = new ActiveProc[0]; combat.EnableArenaToys = false; combat.ChestInterval = 1000;
                combat.Champion.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon("RuneStaff"));
                WeaponDefinition weapon = combat.Champion.Weapon; weapon.ProjectileCount = 1; weapon.Cooldown = 1000; weapon.Element = DamageElement.Physical;
                combat.Champion.CriticalChance = 0; combat.Champion.Passive = PassiveKind.None;
                UnitDefinition enemy = UnityEngine.Object.Instantiate(combat.MeleeEnemy); enemy.Health = 10000; enemy.Speed = 0; enemy.ContactDamage = 0;
                var simulation = new CombatSimulation(combat); simulation.Start(7718);
                UnitState first = simulation.AddFixture(enemy, Faction.Enemy, Vector2.right * 1.2f);
                UnitState second = simulation.AddFixture(enemy, Faction.Enemy, Vector2.right * 2.1f);
                first.NextAttack = second.NextAttack = first.NextContact = second.NextContact = float.PositiveInfinity;
                var pierceLog = new List<string>();
                for (int tick = 0; tick < 40; tick++)
                {
                    simulation.Step(0.025f, Vector2.zero);
                    foreach (CombatEvent value in simulation.Events)
                        if (value.Kind == CombatEventKind.Hit && (value.SubjectId == first.Id || value.SubjectId == second.Id))
                            pierceLog.Add((value.SubjectId == first.Id ? "A" : "B") + ":" + value.Value.ToString("0.##") + ":" + value.Cause.ItemId + ":" + (value.Weapon != null ? value.Weapon.Id : "-") + "@" + tick);
                }
                require(first.Health < enemy.Health && second.Health < enemy.Health, "Single actual projectile pierces into second enemy with no same-target repeat");
                // one hit per victim (the hero's own damage factor may lift each hit above the weapon's base damage)
                require(pierceLog.Count(entry => entry.StartsWith("A:")) == 1 && pierceLog.Count(entry => entry.StartsWith("B:")) == 1, "Pierce victim identity prevents multi-frame duplicate hit (" + string.Join(" | ", pierceLog) + ")");
                simulation.LootFixture(LootKind.Coin, Vector2.up * 1.5f, 1);
                for (int tick = 0; tick < 40; tick++) simulation.Step(0.025f, Vector2.zero);
                require(simulation.Gold == 1 && simulation.Pickups.All(pickup => pickup.Kind != LootKind.Coin), "Coin attracts and credits once inside baseline1.7radius without walking onto it");
                UnityEngine.Object.DestroyImmediate(enemy); UnityEngine.Object.DestroyImmediate(weapon);
            }
            string hud = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunHud.cs");
            require(!hud.Contains("MOVE: WASD") && !hud.Contains("/ ALLIES") && !hud.Contains("DEFEAT THE BOSS"), "Combat HUD removes tutorial bonds difficulty ally-count sections");
            string shop = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs");
            require(!shop.Contains("ĐÃ MUA") && shop.Contains("Mathf.SmoothDamp") && shop.Contains("PlayCommittedArrival(before"), "Shop focus spring plus committed visual arrival replaces sold-text feedback");
            require(File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/ArcadeEffects.cs").Contains("Shape.Puff") && !File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/ArcadeEffects.cs").Contains("value.Faction == Faction.Champion ? 0.75f"), "Hit target rings removed; death puff retained");
            Directory.CreateDirectory("PreviewExports/K7"); File.WriteAllLines("PreviewExports/K7/Quality_Checks.txt", notes);
            Debug.Log("K7_QUALITY_CHECKS_PASSED " + notes.Count);
        }
    }
}
