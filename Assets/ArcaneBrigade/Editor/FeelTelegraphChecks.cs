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
                    Require(telegraph.Windups == 0 && telegraph.Rims == 0 && telegraph.Lanes == 0, "No ink while the boss is not winding up");
                    copy.BossPattern = 5; boss.ActiveBossSignature = -1; boss.TelegraphDirection = Vector2.down; boss.TelegraphUntil = sim.Elapsed + 0.4f;
                    telegraph.Display(sim);
                    Require(telegraph.Windups == 1 && telegraph.Lanes == 1 && telegraph.Marks == 0 && telegraph.Rims == 0, "An aimed volley paints its warning fan and arrowhead");
                    copy.BossPattern = 4; telegraph.Display(sim);
                    Require(telegraph.Windups == 1 && telegraph.Lanes == 0 && telegraph.Rims == 1, "A ring volley paints the danger area around the body");
                    boss.ActiveBossSignature = 0; boss.AbilityTarget = sim.Champion.Position; telegraph.Display(sim);
                    Require(telegraph.Marks == 1 && telegraph.Lanes == 0, "A signature move paints the danger area where it lands");
                    boss.TelegraphUntil = 0; telegraph.Display(sim);
                    Require(telegraph.Windups == 0 && telegraph.Marks == 0 && telegraph.Rims == 0, "The ink clears when the wind-up ends");
                }
                string source = System.IO.File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/BossTelegraph.cs");
                Require(!source.Contains("LineRenderer") && !source.Contains("DangerLane"), "No straight danger lanes from the boss");
                // The 24/25 October warning readability change replaced the earlier outline-only contract.
                Require(source.Contains("ink.FloorZone(") && source.Contains("ink.FloorLane(") && source.Contains("ink.Cone(") && !source.Contains(".Vignette("),
                    "Boss warnings fill the circle, lane and fan danger areas without a screen vignette");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
            // Section 8 replaces the three-tier awakening gauge with each hero's own rule.
            using (var run = new RunSession(catalog))
            {
                run.Start("DrowsySeer", 18019);
                var sim = new CombatSimulation(run.PrepareCombat()); sim.Start(18019);
                Require(sim.HeroRulesV3 != null && sim.HeroRulesV3.HeroId == "DrowsySeer", "Dream Breath uses the section 8 hero runtime");
                Require(!sim.HeroIdentityEnabled && !sim.HeroAwakenEnabled, "Replaced awakening gauge stays disabled during normal fights");
            }
            System.IO.Directory.CreateDirectory("PreviewExports/FeelTelegraph");
            System.IO.File.WriteAllLines("PreviewExports/FeelTelegraph/Checks.txt", notes);
            Debug.Log("FEEL_TELEGRAPH_CHECKS_PASSED " + notes.Count);
        }
    }
}
