using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FriendlyReachChecks
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        private static int checks;
        private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
        public static void Run()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (var weapon in catalog.Weapons)
                foreach (int wave in new[] { 1, 10, 20 })
                    foreach (int tier in new[] { 1, 2, 3 })
                    {
                        using var run = new RunSession(catalog); run.Start("LanternKnight", 71); run.State.Wave = wave;
                        run.State.Roster.Clear(); run.State.Roster.Add(new RosterEntry { UnitId = "JellySwordsman", WeaponId = weapon.Id, Tier = tier });
                        var w = run.PrepareCombat().StartingCompanions.First(u => u.Id == "JellySwordsman").Weapon;
                        Require(w.Range > 0 && w.Range <= (w.Kind == WeaponKind.Melee ? FriendlyReach.MeleeMaximum : FriendlyReach.RangedMaximum) + .001f, "Reach: " + weapon.Id);
                    }
            // Actual accelerated flight must stop at its distance budget, not its much longer lifetime.
            foreach (ShotPattern pattern in Enum.GetValues(typeof(ShotPattern)))
            {
                using var run = new RunSession(catalog); run.Start("LanternKnight", 71); run.State.Roster.Clear();
                var arena = run.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.StoryCompanions = Array.Empty<UnitDefinition>();
                var sim = new CombatSimulation(arena); sim.Start(71);
                var shot = sim.K8CombatProjectileFixture(sim.Champion, Vector2.zero, Vector2.right * 7, 1);
                shot.TravelLeft = 2; shot.Remaining = 20; shot.Pattern = pattern;
                float length = 0; Vector2 previous = shot.Position;
                for (int tick = 0; tick < 200 && sim.Projectiles.Contains(shot); tick++)
                {
                    typeof(CombatSimulation).GetMethod("MoveProjectiles", Hidden).Invoke(sim, new object[] { .05f });
                    length += Vector2.Distance(previous, shot.Position); previous = shot.Position;
                }
                Require(length <= 2.001f && !sim.Projectiles.Contains(shot), "Shot exceeded travel: " + pattern + " " + length);
            }
            VerifyMerchant(catalog);
            VerifyStagedShots(catalog);
            VerifyCriticalEncore(catalog);
            Directory.CreateDirectory("PreviewExports/HeroDpsRange");
            File.WriteAllText("PreviewExports/HeroDpsRange/Checks.txt", checks + " passed: all catalog weapon ranges at waves 1/10/20 and stars 1/2/3; actual flight for every shot pattern; shop merchant fade/restoration with flat and split portraits; wares stay opaque; drill/buoy end-of-flight and returning Arsenal stages; Billy uses equipped attacks only after hero crits; queued responses clear at shop entry.\n");
            Debug.Log("FRIENDLY_REACH_CHECKS_COMPLETE " + checks);
        }
        private static void VerifyCriticalEncore(RunCatalog catalog)
        {
            foreach (bool crit in new[] { false, true })
            {
                using var run = new RunSession(catalog); run.Start("TinVirtuoso", 71); run.State.Wave = 10; run.State.Roster.Clear();
                foreach (string id in new[] { "JellySwordsman", "TeapotPilgrim" })
                    run.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = catalog.Unit(id).Weapon.Id, Tier = 2 });
                var arena = run.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                arena.StoryCompanions = Array.Empty<UnitDefinition>(); arena.EnableHeroIdentities = false;
                arena.Champion.CriticalChance = crit ? 1 : 0;
                var sim = new CombatSimulation(arena); sim.Start(71);
                var dummy = UnityEngine.Object.Instantiate(arena.MeleeEnemy);
                try
                {
                    dummy.Id = "EncoreTarget"; dummy.Health = 100000; dummy.Speed = 0; dummy.KnockbackResistance = 1;
                    dummy.ContactDamage = dummy.CollisionDamage = 0;
                    var target = sim.AddFixture(dummy, Faction.Enemy, Vector2.right);
                    target.NextAttack = target.NextContact = 1000;
                    int melee = 0;
                    for (int i = 0; i < 400; i++)
                    {
                        sim.Step(.05f, Vector2.zero);
                        melee += sim.Events.Count(e => e.Kind == CombatEventKind.Fire && e.Faction == Faction.Companion && e.Weapon.Kind == WeaponKind.Melee);
                    }
                    float friendDamage = sim.Contributions.Where(c => c.UnitId == "JellySwordsman" || c.UnitId == "TeapotPilgrim").Sum(c => c.Damage);
                    Require(crit ? friendDamage > 0 && melee > 0 : friendDamage == 0 && melee == 0,
                        "Critical encore must use the equipped attack and stay blocked without hero crits: " + crit + " " + friendDamage);
                    foreach (var friend in sim.Units.Where(u => u.Faction == Faction.Companion)) friend.CriticalEncore = true;
                    sim.EnterRest();
                    Require(sim.Units.All(u => !u.CriticalEncore && !u.PendingAttack), "Queued encore survived shop entry");
                }
                finally { UnityEngine.Object.DestroyImmediate(dummy); }
            }
        }
        private static void VerifyStagedShots(RunCatalog catalog)
        {
            foreach (string id in new[] { "CandyDrill", "SonarHarpoon", "YoYo", "TwinMoonStaff", "PlushShark" })
            {
                using var run = new RunSession(catalog); run.Start("LanternKnight", 71); run.State.Roster.Clear();
                var arena = run.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                arena.StoryCompanions = Array.Empty<UnitDefinition>(); arena.EnableHeroMechanics = arena.EnableHeroIdentities = false;
                var w = UnityEngine.Object.Instantiate(catalog.Weapon(id));
                try
                {
                    arena.Champion.Weapon = w;
                    var sim = new CombatSimulation(arena); sim.Start(71); sim.Champion.NextAttack = 1000;
                    var shot = sim.K8CombatProjectileFixture(sim.Champion, Vector2.right * .2f, Vector2.right * 7, 1);
                    shot.Arsenal = w.Arsenal; shot.Remaining = .4f;
                    shot.TravelLeft = id == "CandyDrill" || id == "SonarHarpoon" ? .9f : -1;
                    bool drill = false, buoy = false, returned = false;
                    for (int tick = 0; tick < 120; tick++)
                    {
                        sim.Step(.05f, Vector2.zero);
                        drill |= sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Drill);
                        buoy |= sim.ArsenalProps.Any(p => p.Kind == ArsenalPropKind.Buoy);
                        returned |= shot.ArsenalStage > 0 && Vector2.Distance(shot.Position, sim.Champion.Position) < .5f;
                    }
                    Require(id == "CandyDrill" ? drill : id == "SonarHarpoon" ? buoy : returned, "Lost end-of-flight behaviour: " + id);
                }
                finally { UnityEngine.Object.DestroyImmediate(w); }
            }
        }
        private static void VerifyMerchant(RunCatalog catalog)
        {
            using var run = new RunSession(catalog); run.Start("LanternKnight", 71); run.State.Roster.Clear();
            var arena = run.PrepareCombat(); arena.Boss = null; arena.StoryCompanions = Array.Empty<UnitDefinition>();
            var sim = new CombatSimulation(arena); sim.Start(71); sim.EnterRest(); run.State.Phase = SessionPhase.Shop;
            var root = new GameObject("Merchant occlusion check");
            bool reduced = PresentationSettings.ReducedMotion, highlight = PresentationSettings.HighlightHero;
            var settings = typeof(PresentationSettings);
            void Setting(string name, bool value) => settings.GetProperty(name).GetSetMethod(true).Invoke(null, new object[] { value });
            try
            {
                var owner = root.AddComponent<ArenaController>(); owner.ConfigureRun(catalog, null);
                typeof(ArenaController).GetField("session", Hidden).SetValue(owner, run);
                typeof(ArenaController).GetField("simulation", Hidden).SetValue(owner, sim);
                var heroGo = new GameObject("hero"); heroGo.transform.SetParent(root.transform);
                var view = heroGo.AddComponent<UnitVisual>();
                var body = heroGo.AddComponent<SpriteRenderer>(); body.sprite = arena.Champion.Body; body.sortingOrder = 1000;
                typeof(UnitVisual).GetField("body", Hidden).SetValue(view, body);
                typeof(UnitVisual).GetField("state", Hidden).SetValue(view, sim.Champion);
                ((Dictionary<int, UnitVisual>)typeof(ArenaController).GetField("units", Hidden).GetValue(owner))[sim.Champion.Id] = view;
                var merchantGo = new GameObject("merchant"); merchantGo.transform.SetParent(root.transform);
                var actor = merchantGo.AddComponent<ShopDisplayActor>(); actor.SetMerchantMood(false);
                var merchant = merchantGo.AddComponent<SpriteRenderer>(); merchant.sprite = body.sprite; merchant.sortingOrder = 1010;
                var portrait = merchantGo.AddComponent<MerchantPortraitRig>();
                var wareGo = new GameObject("wares"); wareGo.transform.SetParent(root.transform);
                wareGo.AddComponent<ShopDisplayActor>(); var ware = wareGo.AddComponent<SpriteRenderer>(); ware.sprite = body.sprite; ware.sortingOrder = 1020;
                var focus = root.AddComponent<HeroFocus>(); focus.Configure(owner); Setting("HighlightHero", true);
                foreach (bool split in new[] { false, true })
                {
                    Setting("ReducedMotion", !split);
                    portrait.Display(merchant, 1, false, 10, 10, false, 0, !split);
                    typeof(HeroFocus).GetField("nextScan", Hidden).SetValue(focus, -1f);
                    typeof(HeroFocus).GetMethod("LateUpdate", Hidden).Invoke(focus, null);
                    typeof(HeroFocus).GetMethod("ApplyFades", Hidden).Invoke(focus, new object[] { 1f });
                    foreach (var piece in merchantGo.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled))
                        Require(Mathf.Abs(piece.color.a - HeroFocus.PropAlpha) < .01f, "Merchant failed to fade: " + piece.name);
                    Require(Mathf.Approximately(ware.color.a, 1), "Shop wares faded");
                    merchantGo.transform.position = Vector3.right * 20;
                    typeof(HeroFocus).GetMethod("LateUpdate", Hidden).Invoke(focus, null);
                    typeof(HeroFocus).GetMethod("ApplyFades", Hidden).Invoke(focus, new object[] { 1f });
                    foreach (var piece in merchantGo.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled))
                        Require(Mathf.Abs(piece.color.a - 1) < .01f, "Merchant opacity did not restore");
                    merchantGo.transform.position = Vector3.zero;
                }
                // Dispose is owned by this fixture's using declaration, not the fake controller.
                typeof(ArenaController).GetField("session", Hidden).SetValue(owner, null);
            }
            finally { Setting("ReducedMotion", reduced); Setting("HighlightHero", highlight); UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
