using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8PresentationChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K8 presentation: " + caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            require(catalog.Bosses.Select(boss => boss.Body).Distinct().Count() == 5, "Five bosses have distinct original silhouettes");
            require(catalog.Bosses.All(boss => boss.AnimationFrames.Length == 4 && boss.Body.texture.name == "BossesAndProps_v1" && !catalog.Enemies.Any(enemy => enemy.Body == boss.Body)), "Bosses no longer recycle ordinary enemy sprite frames");
            require(catalog.Champions.All(unit => unit.Body.texture.name == "HeroIdentity_v1") && catalog.Champions.Select(unit => unit.Body).Distinct().Count() == 3, "Three protagonists have unique identifiable original animated bodies");
            require(catalog.Companions.Select(unit => unit.Body).Distinct().Count() == catalog.Companions.Length, "Every companion has its own base form");
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art.EnvironmentProps.Length == 4 && art.EnvironmentProps.All(sprite => sprite != null), "Four painted environment landmarks imported");
            foreach (string name in new[] { "BossesAndProps_v1", "HeroIdentity_v1" })
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArcaneBrigade/Art/AnimationHD/" + name + ".png");
                require(texture.GetPixel(0, 0).a < 0.01f, name + " has true transparent alpha");
                require(((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture))).maxTextureSize == 4096, name + " retains source fidelity without claiming native4K");
            }
            Vector2 safe = WorldShop.AvoidObstacle(Vector2.zero, Vector2.zero, 1, 1);
            require(Mathf.Abs(safe.magnitude - 1) < 0.001f && WorldShop.AvoidObstacle(Vector2.right * 3, Vector2.zero, 1, 1) == Vector2.right * 3, "Merchant avoidance projects only overlapping companion positions");
            string worldShop = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs");
            require(worldShop.Contains("CreateMarketFloor") && worldShop.Contains("EnvironmentProps[3]") && worldShop.Contains("station.Actor.Arrive"), "Shop has grounded cloth staging, sign instead of duplicate tiny vendor and stock entrance choreography");
            require(worldShop.Contains("RunModifierRules.Describe(item, 1)") && worldShop.Contains("Vũ khí ngẫu nhiên") && worldShop.Contains("item.EffectiveStacks(held + 1)"), "Purchase consequences preserve random weapon rule, explain stat scope and post-purchase stack count");
            Directory.CreateDirectory("PreviewExports/K8"); File.WriteAllLines("PreviewExports/K8/Presentation_Checks.txt", notes);
            Debug.Log("K8_PRESENTATION_CHECKS_PASSED " + notes.Count);
        }
    }
}
