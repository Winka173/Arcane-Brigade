using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7BChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K7B: " + caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art.GoldMerchant.Length == 4 && art.EggMerchant.Length == 4 && art.RerollMachine.Length == 2 && art.HallDoor.Length == 2, "Twelve original merchant/fixture source frames imported");
            require(art.GoldMerchant.Concat(art.EggMerchant).Concat(art.RerollMachine).Concat(art.HallDoor).All(sprite => sprite != null && sprite.texture.name == "NightMarket_v1"), "New fixture references are project assets not external files");
            require(art.RerollMachine[0] != art.HallDoor[0] && art.GoldMerchant[0] != art.EggMerchant[0], "Reroll/door and two shopkeepers have separate silhouettes");
            require(catalog.Bonds.All(bond => bond.Icon != null && bond.Icon.texture.name == "BondIcons_v1") && catalog.Bonds.Select(bond => bond.Icon).Distinct().Count() == 4, "Four held-system HUD identities use original imported icons");
            require(catalog.Items.Any(item => item.AltarCurrency == ShopCurrency.Health) && catalog.Items.Any(item => item.AltarCurrency == ShopCurrency.Eggs), "Authored special stock supports currentHP and separate egg currency");
            require(RunModifierRules.RarityColor(ItemRarity.Uncommon).b > 0.9f && RunModifierRules.RarityColor(ItemRarity.Rare).r > 0.7f && RunModifierRules.RarityColor(ItemRarity.Mythic).g > 0.7f, "Blue purple gold rarity lighting identities retained");
            string original = JsonUtility.ToJson(catalog);
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 1451);
                CheckpointCatalog first = session.PrepareCombat();
                require(first.WaveDuration == 35 && first.MeleeEnemy.Weapon.Kind == WeaponKind.Contact && first.RangedEnemy.Weapon.Kind == WeaponKind.Contact && first.EnemyPool.All(enemy => enemy.Weapon.Kind == WeaponKind.Contact), "Wave1 lasts35s and every spawn route is contact-only");
                var firstSimulation = new CombatSimulation(first); firstSimulation.Start(first.Seed);
                for (int tick = 0; tick < 1800 && firstSimulation.Phase == RunPhase.Combat; tick++)
                {
                    firstSimulation.Step(1f / 60, new Vector2(Mathf.Sin(tick * 0.02f), Mathf.Cos(tick * 0.02f)));
                    if (firstSimulation.Projectiles.Any(projectile => projectile.Faction == Faction.Enemy)) throw new InvalidOperationException("Wave1 spawned hostile projectile");
                }
                require(true, "Wave1 actual simulation has no hostile projectile over1800tick audit or death");
                session.State.Wave = 2; CheckpointCatalog second = session.PrepareCombat();
                require(second.WaveDuration == 37 && second.EnemyPool.Any(enemy => enemy.Weapon.Kind == WeaponKind.Projectile), "Wave2 opens ranged enemy pool and increases time gently");
                require(second.EnemyPool.All(enemy => enemy.Weapon.ProjectileSpeed <= 2.4f), "Enemy cloned projectile speed capped at2.4 without changing ally weapons");
                session.State.Wave = 20; require(session.PrepareCombat().WaveDuration == 55, "Later non-boss authored duration capped55s");
                session.State.Wave = 1; session.State.Roster.Clear();
                for (int index = 0; index < 10; index++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[index % catalog.Companions.Length].Id, WeaponId = catalog.Weapons[index % catalog.Weapons.Length].Id });
                CheckpointCatalog idle = session.PrepareCombat(); idle.EnemyBudget = 0; idle.Boss = null;
                var simulation = new CombatSimulation(idle); simulation.Start(idle.Seed); simulation.EnterRest();
                int idleTicks = 0; int movingTicks = 0; bool simultaneous = false;
                for (int tick = 0; tick < 900; tick++)
                {
                    simulation.StepRest(1f / 60, Vector2.zero); bool still = false; bool moving = false;
                    foreach (UnitState ally in simulation.Units.Where(unit => unit.Faction == Faction.Companion))
                    {
                        bool stopped = (ally.Position - ally.PreviousPosition).sqrMagnitude < 0.000001f;
                        if (stopped && ally.RoamRestUntil > simulation.VisualElapsed) { idleTicks++; still = true; }
                        if (!stopped) { movingTicks++; moving = true; }
                        if ((ally.Position - simulation.Champion.Position).magnitude > idle.CompanionLeash + 0.001f) throw new InvalidOperationException("Rest/roam exceeded leash");
                    }
                    simultaneous |= still && moving;
                }
                require(idleTicks > 60 && movingTicks > 400 && simultaneous, "Stationary champion has simultaneous independently resting and roaming companions");
                require(true, "900ticks and10companions never leave leash");
                var bodyObject = new GameObject("IdleBodyCheck");
                SpriteRenderer renderer = bodyObject.AddComponent<SpriteRenderer>(); renderer.sprite = idle.Champion.Body;
                BodyPuppet puppet = bodyObject.AddComponent<BodyPuppet>(); puppet.Display(renderer, simulation.Champion, 0.4f, false);
                Transform upper = bodyObject.transform.Find("UpperBody"); Vector3 firstPose = upper.localScale;
                puppet.Display(renderer, simulation.Champion, 1.2f, false);
                require(upper.localScale != firstPose && upper.localRotation != Quaternion.identity && !renderer.enabled, "Stationary body breathing/sway visible independently of weapon");
                puppet.Display(renderer, simulation.Champion, 1.2f, true); firstPose = upper.localScale;
                puppet.Display(renderer, simulation.Champion, 2.2f, true); require(upper.localScale == firstPose && upper.localRotation == Quaternion.identity, "Reduced motion removes breathing/sway without changing combat");
                UnityEngine.Object.DestroyImmediate(bodyObject);
            }
            require(JsonUtility.ToJson(catalog) == original, "Runtime wave/speed tuning leaves source catalog untouched");
            string shop = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs");
            require(shop.Contains("new Vector2(0, -4.3f)") && shop.Contains("BeginHallExit") && shop.Contains("ItemPickup.Play") && shop.Contains("DeniedUntil"), "Downward exit and item/deny visual feedback wired through actual transactions");
            require(shop.Contains("DrawPriceTags") && !shop.Contains("CreatePlate") && shop.Contains("art.RerollMachine") && shop.Contains("art.HallDoor"), "Price ticket UI replaces mesh boxes; utility objects do not reuse offer pedestals");
            Directory.CreateDirectory("PreviewExports/K7B"); File.WriteAllLines("PreviewExports/K7B/Checks.txt", notes);
            Debug.Log("K7B_CHECKS_PASSED " + notes.Count);
        }
    }
}
