using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class FinalQualityChecks
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            void Require(bool value, string text) { if (!value) throw new InvalidOperationException("Final quality: " + text); notes.Add("PASS " + text); }
            var lightingFixture = new GameObject("SpriteLightingRegression");
            try
            {
                var renderer = lightingFixture.AddComponent<SpriteRenderer>(); renderer.sprite = catalog.Champions[0].Body;
                foreach (bool hostile in new[] { false, true })
                    foreach (bool prop in new[] { false, true })
                    {
                        var properties = new MaterialPropertyBlock();
                        SpriteLighting.Apply(renderer, properties, hostile, prop);
                        Rect rectangle = renderer.sprite.rect; Texture texture = renderer.sprite.texture;
                        Require(properties.GetVector("_UvRect") == new Vector4(rectangle.x / texture.width, rectangle.y / texture.height, rectangle.width / texture.width, rectangle.height / texture.height), "Lighting UV remains exact " + hostile + "/" + prop);
                        Require(Mathf.Approximately(properties.GetFloat("_NightVisibility"), 1) && Mathf.Approximately(properties.GetFloat("_Saturation"), 1.06f), "Unlit visibility and saturation preserved " + hostile + "/" + prop);
                        Require(Mathf.Approximately(properties.GetFloat("_ShadeAmount"), prop ? .045f : .085f) && Mathf.Approximately(properties.GetFloat("_RimAmount"), prop ? .06f : .12f), "Unlit shade and rim preserved " + hostile + "/" + prop);
                    }
            }
            finally { UnityEngine.Object.DestroyImmediate(lightingFixture); }
            Require(CompanionBanter.SpeciesCount == catalog.Companions.Length, "One authored dialogue set per companion species");
            Require(CompanionBanter.AllLines.Distinct().Count() == catalog.Companions.Length * 3, "All 198 species lines are unique");
            FinalQualityArtwork artwork = FinalQualityArtwork.Current;
            if (artwork != null)
            {
                Require(artwork.Props.Length == 20, "Twenty distinct redrawn environment objects");
                foreach (var prop in artwork.Props)
                {
                    Require(prop.Frames.Length == 4 && prop.Frames.Distinct().Count() == 4, "Four authored environment poses " + prop.Id);
                    Require(artwork.Frame(prop.Id, 0, true) == artwork.Frame(prop.Id, 200, true), "Stable reduced environment pose " + prop.Id);
                }
                foreach (UnitDefinition hero in catalog.Champions.Take(10))
                    Require(UnityEditor.AssetDatabase.GetAssetPath(hero.Body).Contains("/FinalQuality/") && CharacterActingLibrary.Current.Find(hero.Id).Curious.All(frame => UnityEditor.AssetDatabase.GetAssetPath(frame).Contains("/FinalQuality/")), "Redrawn body and expression actually assigned " + hero.Id);
            }
            foreach (UnitDefinition ally in catalog.Companions)
            {
                Require(CompanionBanter.Has(ally.Id), "Exact-ID dialogue " + ally.Id);
                foreach (int wave in new[] { 1, 12, 20 })
                    for (int line = 0; line < 3; line++) Require(!string.IsNullOrWhiteSpace(OctoberDialogue.AllyLine(ally, line, wave)) && OctoberDialogue.AllyLine(ally, line, wave).Length < 110, "Bounded context line " + ally.Id + "/" + wave + "/" + line);
                Require(CompanionBanter.Line(ally.Id, 0, 1) != CompanionBanter.Line(ally.Id, 0, 12), "Nightmare-specific voice " + ally.Id);
            }
            var shapes = new HashSet<string>();
            foreach (UnitDefinition original in catalog.Bosses.Concat(new[] { OutsideStageAssets.Current.Boss, NightmareAssets.Current.Customer }))
                using (var session = new RunSession(catalog))
                {
                    session.Start("LanternKnight", 161026); session.State.Wave = 12;
                    CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0;
                    combat.Boss = original;
                    var simulation = new CombatSimulation(combat); simulation.Start(161026);
                    UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.Position = new Vector2(0, 3);
                    int identity = original.Id == "OutsideStageKeeper" ? 11 : original.Id == NightmareRules.CustomerId ? 12 : original.BossPattern;
                    Require(BossIdentityEncore.Name(identity) != null, "Named individual spatial encore " + original.Id);
                    int shots = 0;
                    Action<Vector2, float, float, float, HostileShot> shot = (position, angle, delay, speed, motion) => shots++;
                    MethodInfo method = typeof(CombatSimulation).GetMethod("ResolveIdentityEncore", BindingFlags.NonPublic | BindingFlags.Instance);
                    Require((bool)method.Invoke(simulation, new object[] { boss, 0, identity, shot }), "Production choreography executes " + original.Id);
                    Require(simulation.ExpandedFields.Count > 0 && simulation.ExpandedFields.Count <= 16 && shots <= 4, "Bounded non-projectile choreography " + original.Id);
                    foreach (ExpandedField field in simulation.ExpandedFields)
                        Require(field.At >= simulation.Elapsed + .95f && float.IsFinite(field.Position.x) && float.IsFinite(field.Position.y) && field.Damage > 0, "Committed readable hazard " + original.Id);
                    string fingerprint = string.Join(";", simulation.ExpandedFields.Select(field => field.Kind + ":" + field.Position.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + ":" + field.Position.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + ":" + field.At.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
                    Require(shapes.Add(fingerprint), "Distinct spatial footprint and timing " + original.Id);
                    simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "Choreography clears at shop boundary " + original.Id);
                }
            Directory.CreateDirectory("PreviewExports/FinalQuality"); File.WriteAllLines("PreviewExports/FinalQuality/Focused_Checks.txt", notes);
            Debug.Log("FINAL_QUALITY_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate()
        {
            Run(); RecruitmentChecks.Run(); HeroIdentityChecks.Run(); OctoberContentBuilder.Regressions();
            FinalQualityArtBuilder.Import(); Run(); ExpandedPerformanceBuilder.Check(); HeroIdentityChecks.Run(); FullRunBuilder.BuildWindows();
        }
        public static void CheckAndBuild()
        {
            Run(); RosterCapAndBondChecks.Run(); RecruitmentChecks.Run(); HeroIdentityChecks.Run(); ExpandedPerformanceBuilder.Check(); OctoberContentBuilder.Regressions(); FullRunBuilder.BuildWindows();
        }
        public static void ImportAndBuild()
        {
            FinalQualityArtBuilder.Import(); CheckAndBuild();
        }
    }
}
