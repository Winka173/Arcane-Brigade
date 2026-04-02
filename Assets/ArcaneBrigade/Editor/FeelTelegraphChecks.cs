using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Crits and big kills hold the screen for a blink (rarely enough not to stutter); bosses paint their wind-up in
    /// ink on the floor: a spreading pool, a tongue along an aimed fan, a blot where a signature move lands (18/10/2026).</summary>
    public static class FeelTelegraphChecks
    {
        [MenuItem("Arcane Brigade/Checks/Hit-stop and boss ink telegraphs")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("FeelTelegraph " + caption); notes.Add("PASS " + caption); }
            PresentationSettings.Apply(new PlayerProfile());
            var views = new Dictionary<int, UnitVisual>();
            var bigDefinition = ScriptableObject.CreateInstance<UnitDefinition>(); bigDefinition.Radius = 0.55f; bigDefinition.Health = 240;
            var smallDefinition = ScriptableObject.CreateInstance<UnitDefinition>(); smallDefinition.Radius = 0.25f; smallDefinition.Health = 400;
            try
            {
                var big = new UnitState { Id = 40, Definition = bigDefinition, Faction = Faction.Enemy };
                var small = new UnitState { Id = 41, Definition = smallDefinition, Faction = Faction.Enemy };
                CombatEvent HeroCrit() => new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 5, Vector2.right, critical: true, subjectId: small.Id, subject: small, cause: new CombatCause(1, 7));
                // no hit stop anywhere (play-test 20/10/2026): crits, big kills and boss falls never hold the screen
                var feel = new CombatFeel();
                feel.Play(new[] { HeroCrit() }, views, 1, 7);
                Require(!feel.Frozen && feel.PresentationSpeed == 1, "The hero's critical hit never holds the screen");
                var kill = new CombatFeel();
                kill.Play(new[] { new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Enemy, 0, Vector2.right, subjectId: big.Id, subject: big) }, views, 1, 7);
                Require(!kill.Frozen && kill.Trauma > 0.1f, "A big enemy's fall shakes the screen without holding it");

                var bossDefinition = ScriptableObject.CreateInstance<UnitDefinition>(); bossDefinition.IsBoss = true; bossDefinition.Health = 900;
                try
                {
                    var boss = new UnitState { Id = 42, Definition = bossDefinition, Faction = Faction.Enemy };
                    CombatEvent Fall() => new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Enemy, 0, Vector2.right, subjectId: boss.Id, subject: boss);
                    var midBoss = new CombatFeel(); midBoss.Play(new[] { Fall() }, views, 1, 7, false);
                    Require(!midBoss.Frozen && !midBoss.SlowMotion, "A wave-5/10/15 boss falls at full speed");
                    var finale = new CombatFeel(); finale.Play(new[] { Fall() }, views, 1, 7, true);
                    Require(!finale.Frozen && finale.SlowMotion, "The final boss's fall plays in slow motion");
                    finale.Advance(CombatFeel.FinalBossSlowMotion + 0.01f);
                    Require(!finale.SlowMotion, "The slow motion lasts half a second");
                    PresentationSettings.Apply(new PlayerProfile { ReducedMotion = true });
                    var reduced = new CombatFeel(); reduced.Play(new[] { HeroCrit(), Fall() }, views, 1, 7, true);
                    Require(!reduced.Frozen && !reduced.SlowMotion, "Reduced motion: no slow motion either");
                }
                finally { UnityEngine.Object.DestroyImmediate(bossDefinition); }
            }
            finally
            {
                PresentationSettings.Apply(new PlayerProfile());
                UnityEngine.Object.DestroyImmediate(bigDefinition); UnityEngine.Object.DestroyImmediate(smallDefinition);
            }

            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var host = new GameObject("FeelTelegraphFixture");
            UnitDefinition copy = null;
            try
            {
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 18018); run.State.Wave = 5;
                    var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18018);
                    UnitState boss = sim.Boss;
                    Require(boss != null, "Wave 5 has a boss to telegraph");
                    copy = UnityEngine.Object.Instantiate(boss.Definition); boss.Definition = copy;
                    BossTelegraph telegraph = host.AddComponent<BossTelegraph>();
                    telegraph.Display(sim);
                    Require(telegraph.Pools == 0, "No ink while the boss is not winding up");
                    copy.BossPattern = 5; boss.ActiveBossSignature = -1; boss.TelegraphDirection = Vector2.down; boss.TelegraphUntil = sim.Elapsed + 0.4f;
                    telegraph.Display(sim);
                    Require(telegraph.Pools == 1 && telegraph.Tongues == 1 && telegraph.Blots == 0, "An aimed volley: the wind-up droplets and a tongue of ink along the aim (no pool since 19/10/2026)");
                    copy.BossPattern = 4; telegraph.Display(sim);
                    Require(telegraph.Pools == 1 && telegraph.Tongues == 0, "A ring volley: the wind-up droplets alone, no lanes radiating from the boss");
                    boss.ActiveBossSignature = 0; boss.AbilityTarget = sim.Champion.Position; telegraph.Display(sim);
                    Require(telegraph.Blots == 1 && telegraph.Tongues == 0, "A signature move: a blot where it lands");
                    boss.TelegraphUntil = 0; telegraph.Display(sim);
                    Require(telegraph.Pools == 0 && telegraph.Blots == 0, "The ink clears when the wind-up ends");
                }
                string source = System.IO.File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/BossTelegraph.cs");
                Require(!source.Contains("LineRenderer") && !source.Contains("DangerLane"), "No straight danger lanes from the boss");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
            // the hero's gauge: an upright vial left of the hero, no taller than the hero, following them
            var gaugeHost = new GameObject("HeroGaugeFixture");
            try
            {
                using (var run = new RunSession(catalog))
                {
                    run.Start("DrowsySeer", 18019); run.State.Wave = 3;
                    var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18019);
                    Require(sim.HeroIdentityEnabled, "Hero identities run in a normal fight");
                    HeroIdentityVisual visual = gaugeHost.AddComponent<HeroIdentityVisual>();
                    visual.Display(sim);
                    UnitState hero = sim.Champion;
                    Vector2 bottom = visual.GaugeBottom, top = visual.GaugeTop;
                    Require(top.y > bottom.y && top.y - bottom.y <= hero.Definition.BodyHeight + 0.001f, "The gauge stands upright and is no taller than the hero");
                    Require(Mathf.Abs(top.x - bottom.x) < 0.001f && top.x < hero.Position.x - hero.Definition.BodyHeight * 0.3f, "The gauge stands to the hero's left");
                    hero.Position += new Vector2(2, 1); visual.Display(sim);
                    Require(Vector2.Distance(visual.GaugeBottom - bottom, new Vector2(2, 1)) < 0.001f, "The gauge follows the hero");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(gaugeHost); }
            System.IO.Directory.CreateDirectory("PreviewExports/FeelTelegraph");
            System.IO.File.WriteAllLines("PreviewExports/FeelTelegraph/Checks.txt", notes);
            Debug.Log("FEEL_TELEGRAPH_CHECKS_PASSED " + notes.Count);
        }
    }
}
