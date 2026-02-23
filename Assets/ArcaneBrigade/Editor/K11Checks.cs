using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11Checks
    {
        // read the painted file itself: shipped textures are not import-readable (that would double their memory)
        private static float CornerAlpha(Sprite sprite)
        {
            var copy = new Texture2D(2, 2);
            try { copy.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture))); return copy.GetPixel(0, 0).a; }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException(name); notes.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 7707);
                session.State.Phase = SessionPhase.Shop; session.State.SpecialShopVisited = true;
                RunSnapshot saved = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                session.Restore(saved);
                require(session.State.SpecialShopVisited, "Special merchant visit stage survives save/restore");
                session.ContinueWave();
                require(session.State.Wave == 2 && session.State.Phase == SessionPhase.Combat, "Continue advances exactly one wave");
                CombatSimulation simulation = new CombatSimulation(session.PrepareCombat()); simulation.Start(7707);
                simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(2, -3);
                simulation.EnterRest();
                require(simulation.Champion.Position == new Vector2(2, -3), "Rest preserves player world position");
                require(simulation.Units.All(unit => unit.Faction != Faction.Enemy) && simulation.Projectiles.Count == 0, "Shop is safe with hostile units/projectiles cleared");
                simulation.StepRest(1f / 60, Vector2.right);
                require(simulation.Champion.Position.x > 2, "Player continues moving during rest");
            }
            string controller = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs");
            string lifecycle = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs");
            string hud = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunHud.cs");
            require(controller.Contains("public float HallCurtain => 0") && !hud.Contains("owner.HallCurtain >"), "No inter-wave opaque curtain/loading overlay");
            require(!controller.Substring(controller.IndexOf("public void NextWave()"), controller.IndexOf("public void ContinueEndless()") - controller.IndexOf("public void NextWave()")).Contains("LoadScene"), "Continue uses same arena scene");
            require(controller.Contains("positions.TryGetValue") && controller.Contains("championPosition.Value"), "Compiled wave refresh preserves friendly positions");
            require(controller.Contains("follow.Follow(this, !preserveEffects)"), "Camera does not snap on purchases or same-scene phase changes");
            require(controller.Contains("session.Restore(before); if (Shop != null) Shop.CancelDeparture()"), "Failed boundary save restores shop departure for retry");
            require(lifecycle.Contains("!state.SpecialShopVisited") && lifecycle.Contains("owner.FinishSpecialShop()"), "Special first then gold merchant stage, not toggle sign");
            require(!lifecycle.Contains("Station visit = AddStation"), "No alternate-shop switch station left on map");
            string actorMotion = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/ShopDisplayActor.cs");
            string marketMotion = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/MarketLifecycle.cs");
            require(!actorMotion.Contains("SmoothStep(0.05f, 1, arrival)") && actorMotion.Contains("6 * travel * travel"), "Gold merchant falls ballistically at full authored size");
            require(actorMotion.Contains("-height * Mathf.SmoothStep") && actorMotion.Contains("launchOffset * Mathf.SmoothStep"), "Dream merchant rises and goods follow separate launch arcs");
            require(!marketMotion.Contains("1 - fold") && marketMotion.Contains("carpet.Display"), "Market root never shrinks; rug owns independent mesh curl");
            require(lifecycle.Contains("actor.Depart") && lifecycle.Contains("actor.Arrive(0)"), "Independent actor departures recover after failed boundary save");
            require(File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/UnitVisual.cs").Contains("unit.Faction != Faction.Enemy &&"), "Enemy attacks use body, never held friendly weapon");
            require(File.ReadAllText("Assets/ArcaneBrigade/Runtime/Core/CombatSimulation.cs").Contains("unit.Definition.Radius * 0.9f"), "Hostile spit originates from body rather than held muzzle");
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art.GiantGoldMerchant.Length == 4 && art.GiantEggMerchant.Length == 4 && art.GiantGoldMerchant.Concat(art.GiantEggMerchant).Distinct().Count() == 8, "Two new original giant merchants with four distinct imported states each");
            require(art.GiantGoldMerchant[0].texture != art.GiantEggMerchant[0].texture && CornerAlpha(art.GiantGoldMerchant[0]) < 0.01f && CornerAlpha(art.GiantEggMerchant[0]) < 0.01f, "Giant merchant identities have separate true-alpha source art");
            require(WorldShop.MerchantSize(false, 1) == WorldShop.MerchantSize(false, 2) && WorldShop.MerchantSize(true, 3) > 5.5f && WorldShop.MerchantSize(false, 3) > 5, "Large to colossal merchant sizes vary by hall and shop identity");
            require(lifecycle.Contains("owner.Content.RestViewHeight - MerchantHeight * 0.5f - 0.45f") && lifecycle.Contains("AvoidObstacle(unit.Position, MerchantCenter"), "Colossal staging respects camera height and avoidance follows actual NPC position");
            Directory.CreateDirectory("PreviewExports/K11"); File.WriteAllLines("PreviewExports/K11/Focused_Checks.txt", notes);
            Debug.Log("K11_FOCUSED_CHECKS_PASSED " + notes.Count);
        }
    }
}
