using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K13Checks
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var notes = new List<string>();
            void Require(bool condition, string label) { if (!condition) throw new InvalidOperationException(label); notes.Add("PASS " + label); }
            string[] arms = { "DreamOrbStaff", "FireExtinguisher", "PlasmaTrident", "MoonFeatherFan", "TwinSabres", "ClockCrescent", "ParasolNeedle", "StampHammer" };
            Material floor = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/ArcaneBrigade/Data/QualityHD/FloorHD.mat");
            Require(floor.mainTexture.name == "CarnivalStone_v1" && floor.GetTexture("_PatinaTex").name == "CarnivalPatina_v1", "New stone and independent patina textures are connected");
            Require(floor.GetFloat("_PatinaStrength") > 0 && floor.GetFloat("_MicroStrength") > 0 && floor.GetFloat("_Detail") > 0.5f, "Three material scales retain visible painted detail");
            Require(!File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/K8ArenaScenery.cs").Contains("DreamLandmark_"), "Raised decorative map props removed");
            Require(arms.Count(id => catalog.Weapon(id).Kind == WeaponKind.Melee) == 4 && arms.Count(id => catalog.Weapon(id).Kind == WeaponKind.Projectile) == 4, "New arsenal four melee/four ranged");
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses))
            {
                Require(unit.Body != null && unit.AnimationFrames.Length >= 4 && unit.AnimationFrames.All(sprite => sprite != null), "Four authored frames and body " + unit.Id);
                var profile = new SpeciesMotionProfile(unit);
                Require(profile.GaitAmplitude > 0 && profile.Recoil > 0 && profile.IdleGesture > 0, "Idle/travel/recoil motion profile " + unit.Id);
            }
            foreach (UnitDefinition hero in catalog.Champions)
            {
                if (hero.HeroMechanic >= HeroMechanic.RootedOracle) continue;
                string weaponId = HeroLoadout.Weapon(catalog, hero.Id);
                Require(hero.Speed >= 2.9f && hero.Speed <= 3.7f && hero.Weapon.Id == weaponId, "Slower authored speed and fixed weapon " + hero.Id);
                using var session = new RunSession(catalog); session.Start(hero.Id, 13077);
                session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                string id = session.State.Roster[0].UnitId;
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = id == "TeapotPilgrim" ? "InkPistol" : "SpoonSabre", Tier = 1 });
                int mergeOffers = 0;
                for (int roll = 0; roll < 100; roll++)
                {
                    session.State.Gold = 100000; session.Reroll();
                    mergeOffers += session.State.Offers.Count(offer => offer.Kind == OfferKind.Companion && offer.Id == id);
                }
                Require(mergeOffers >= 75, "Merge-ready recruitment appears often " + hero.Id + " count=" + mergeOffers);
                var offer = new ShopOffer { Token = ++session.State.NextOffer, Id = id, Kind = OfferKind.Companion, Price = 1 };
                session.State.Offers.Add(offer);
                uint random = session.State.WeaponRandom;
                Require(ShopMergePreview.Ready(session, offer) && session.State.WeaponRandom == random, "Merge preview does not consume weapon randomness " + hero.Id);
                Require(session.Buy(offer.Token) && session.State.Roster.Any(entry => entry.UnitId == id && entry.Tier == 2) && session.State.Roster.Count == 1, "Purchase actually creates two-star ally " + hero.Id);
                Require(!ShopMergePreview.Ready(session, offer), "Sold offer clears preview " + hero.Id);
                session.State.Roster.Clear();
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = "SpoonSabre", Tier = 1 });
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = "InkPistol", Tier = 1 });
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = "ToastCleaver", Tier = 2 });
                session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = "ShellCannon", Tier = 2 });
                var cascade = new ShopOffer { Token = ++session.State.NextOffer, Id = id, Kind = OfferKind.Companion, Price = 1 }; session.State.Offers.Add(cascade);
                Require(ShopMergePreview.ProjectedTier(session, cascade) == 3, "Preview predicts cascade to three stars " + hero.Id);
                Require(session.Buy(cascade.Token) && session.State.Roster.Count == 1 && session.State.Roster[0].Tier == 3, "Actual cascade matches three-star preview " + hero.Id);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 13077);
                UnitDefinition ordinary = catalog.Companions.First(unit => !unit.SpecialRecruit);
                UnitDefinition elite = catalog.Companions.First(unit => unit.SpecialRecruit);
                Require(!session.RecruitWeaponEligible(ordinary.Id, catalog.Weapon("DreamOrbStaff")) && session.RecruitWeaponEligible(elite.Id, catalog.Weapon("DreamOrbStaff")), "Dream orb restricted to special recruits");
                session.State.Phase = SessionPhase.Shop; session.State.Eggs = 100;
                var offer = new ShopOffer { Token = ++session.State.NextOffer, Id = elite.Id, Kind = OfferKind.Companion, Price = 1, Currency = ShopCurrency.Eggs };
                session.State.SpecialOffers.Add(offer);
                Require(session.Buy(offer.Token) && catalog.Weapon(session.State.Roster.Last().WeaponId).SpecialOnly, "Actual dream recruit receives an exclusive weapon");
            }
            foreach (string id in arms) CheckAttack(catalog, id, Require);
            Require(catalog.Weapon("DreamOrbStaff").ProjectilePierce == 3 && catalog.Weapon("DreamOrbStaff").ProjectileVisualWidth >= 0.9f, "Giant orb has large collision/visual profile and pierce");
            Require(catalog.Weapon("FireExtinguisher").ProjectileCount == 6 && catalog.Weapon("FireExtinguisher").ProjectileLifetime < 1, "Foam has six short-lived pellets");
            Require(catalog.Weapon("MoonFeatherFan").ProjectileCount == 4 && catalog.Weapon("PlasmaTrident").ProjectileCount == 3, "Distinct fan/plasma default volley profiles");
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect))) Require(GlossaryUI.Find(ShortOfferCopy.StatName(effect) + ".")?.Effect == effect, "Compact stat alias remains clickable " + effect);
            foreach (Vector2 surface in new[] { new Vector2(1280, 720), new Vector2(1920, 1080), new Vector2(2560, 1440), new Vector2(3840, 2160), new Vector2(2560, 1080), new Vector2(1600, 1200) })
            {
                Matrix4x4 matrix = ArcadeTypography.FitViewport(new Rect(0, 0, surface.x, surface.y), new Rect(0, 0, surface.x, surface.y), surface.y, 1920, 1080);
                Vector3 left = matrix.MultiplyPoint3x4(Vector3.zero); Vector3 right = matrix.MultiplyPoint3x4(new Vector3(1920, 1080, 0));
                Require(left.x >= -0.01f && left.y >= -0.01f && right.x <= surface.x + 0.01f && right.y <= surface.y + 0.01f, "Entire HUD fits desktop surface " + surface);
            }
            Directory.CreateDirectory("PreviewExports/K13"); File.WriteAllLines("PreviewExports/K13/Focused_Checks.txt", notes); Debug.Log("K13_CHECKS_PASSED " + notes.Count);
        }
        private static void CheckAttack(RunCatalog catalog, string id, Action<bool, string> require)
        {
            using var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id, 13077); session.State.Roster.Clear();
            CheckpointCatalog arena = session.PrepareCombat(); arena.EnemyBudget = 0; arena.Boss = null; arena.EnableArenaToys = false; arena.EnableHeroMechanics = false;
            WeaponDefinition weapon = UnityEngine.Object.Instantiate(catalog.Weapon(id)); arena.Champion.Weapon = weapon;
            try
            {
                var simulation = new CombatSimulation(arena); simulation.Start(13077);
                UnitState target = simulation.AddFixture(arena.MeleeEnemy, Faction.Enemy, new Vector2(Mathf.Min(weapon.Range * 0.7f, 2), 0));
                target.Definition.Speed = 0; target.Health = 100000; target.NextAttack = target.NextContact = 100; simulation.Champion.NextAttack = 0;
                int peak = 0; bool returning = false; bool hit = false;
                for (int tick = 0; tick < 180; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    peak = Mathf.Max(peak, simulation.Projectiles.Count(projectile => projectile.Faction == Faction.Champion));
                    returning |= simulation.Projectiles.Any(projectile => projectile.Returning);
                    hit |= target.Health < 100000;
                }
                require(hit, "Real attack delivers damage " + id);
                if (weapon.Kind == WeaponKind.Projectile) require(peak >= weapon.ProjectileCount, "Real initial volley count " + id + " peak=" + peak);
                if (id == "ClockCrescent") require(returning && weapon.HomingOutbound, "Clock boomerang actually returns and has outbound homing");
            }
            finally { UnityEngine.Object.DestroyImmediate(weapon); }
        }
    }
}
