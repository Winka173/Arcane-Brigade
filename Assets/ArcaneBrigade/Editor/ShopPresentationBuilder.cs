using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ShopPresentationBuilder
    {
        public static void Build()
        {
            // RarityBloom_v2 (12 frames per rarity, a seamless loop) replaces v1 (4 per rarity) as soon as it is delivered
            bool smooth = System.IO.File.Exists("Assets/ArcaneBrigade/Art/ShopPresentation/RarityBloom_v2.png");
            Sprite[] frames = smooth ? K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/ShopPresentation/RarityBloom_v2.png", "RarityBloom", 12, 4, Enumerable.Range(0, 48).ToArray())
                : K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/ShopPresentation/RarityBloom_v1.png", "RarityBloom", 4, 4, Enumerable.Range(0, 16).ToArray());
            const string auraPath = "Assets/ArcaneBrigade/Resources/ShopRarityArtwork.asset";
            ShopRarityArtwork aura = AssetDatabase.LoadAssetAtPath<ShopRarityArtwork>(auraPath);
            if (aura == null) { aura = ScriptableObject.CreateInstance<ShopRarityArtwork>(); AssetDatabase.CreateAsset(aura, auraPath); }
            aura.Frames = frames; EditorUtility.SetDirty(aura);
            Sprite[] furniture = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/ShopPresentation/GroundedCabinets_v1.png", "GroundedCabinet", 2, 1, new[] { 0, 1 }, componentRows: true);
            const string sceneryPath = "Assets/ArcaneBrigade/Resources/GroundedSceneryArtwork.asset";
            GroundedSceneryArtwork scenery = AssetDatabase.LoadAssetAtPath<GroundedSceneryArtwork>(sceneryPath);
            if (scenery == null) { scenery = ScriptableObject.CreateInstance<GroundedSceneryArtwork>(); AssetDatabase.CreateAsset(scenery, sceneryPath); }
            scenery.Cabinets = furniture; EditorUtility.SetDirty(scenery); AssetDatabase.SaveAssets();
            ShopRarityArtwork.Invalidate(); GroundedSceneryArtwork.Invalidate();
            SpriteInkMetricsBuilder.Build(); Check();
        }
        public static void Check()
        {
            var notes = new List<string>();
            void Require(bool value, string label) { if (!value) throw new InvalidOperationException("Shop presentation: " + label); notes.Add("PASS " + label); }
            ShopRarityArtwork aura = ShopRarityArtwork.Current;
            Require(aura != null && (aura.Frames.Length == 16 || aura.Frames.Length == 48), "Rarity animation cells imported (4 or 12 per rarity)");
            foreach (ItemRarity rarity in new[] { ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Mythic, ItemRarity.Crimson, ItemRarity.Rainbow })
            {
                Require(aura.Frame(rarity, -1) == aura.Frame(rarity, 0) && aura.Frame(rarity, 99) == aura.Frame(rarity, 3), "Aura frame bounds " + rarity);
                Require(Enumerable.Range(0, 4).Select(frame => aura.Frame(rarity, frame)).Distinct().Count() == 4, "Four different painted phases " + rarity);
                Require(ShopRarityGlow.AuraWidth(.8f, 1, rarity) > ShopRarityGlow.AuraWidth(.8f, 0, rarity), "Focused rarity expands " + rarity);
            }
            Require(aura.Frame(ItemRarity.Uncommon, 0) != aura.Frame(ItemRarity.Rare, 0) && aura.Frame(ItemRarity.Rare, 0) != aura.Frame(ItemRarity.Mythic, 0), "Blue purple and gold have distinct painted art");
            Require(GroundedSceneryArtwork.Current.Cabinets.Length == 2, "Two freestanding toy cabinet replacements imported");
            Require(ShopDisplayActor.FocusScale(1, 1, false) >= 1.17f && ShopDisplayActor.FocusScale(0, 1, false) == 1, "Offer focus has visible growth and returns to base");
            Require(ShopDisplayActor.FocusScale(1, .1f, false) != ShopDisplayActor.FocusScale(1, 1, false), "Offer has an event-driven settling bounce");
            Require(ShopDisplayActor.FocusScale(1, .1f, true) == ShopDisplayActor.FocusScale(1, 1, true), "Reduced focus is static but visible");
            Require(WorldShop.ExitInteractionRadius > WorldShop.InteractionRadius && WorldShop.ExitPosition.x==0, "Continue uses a broad lower-centre entrance");
            foreach (float stockRow in new[] { -1.1f, -2f, -3f })
                for (int index = -1; index <= 1; index++) Require(Vector2.Distance(WorldShop.ExitPosition, new Vector2(index * 3.2f, stockRow)) > 1.1f, "Exit centre remains below merchandise; nearest station retains focus " + stockRow + "/" + index);
            Require(Vector2.Distance(WorldShop.ExitPosition + Vector2.up * WorldShop.LayoutLift, OutsideStageChoice.ChallengePosition) > WorldShop.ExitInteractionRadius + 1.6f, "Continue is separate from optional Keeper entry");
            var parent = new GameObject("GroundingCheckParent"); var child = new GameObject("GroundingCheckSprite"); child.transform.SetParent(parent.transform, false);
            try
            {
                SpriteRenderer view = child.AddComponent<SpriteRenderer>();
                Sprite[] sprites = GroundedSceneryArtwork.Current.Cabinets.Concat(Resources.Load<CarnivalStageAssets>("CarnivalStageAssets").Stands).Concat(NightmareAssets.Current.Scenery).ToArray();
                foreach (Sprite sprite in sprites)
                    foreach (float scale in new[] { .3f, 1f, 2.4f })
                        foreach (float angle in new[] { -12f, 0, 12f })
                        {
                            parent.transform.localScale = new Vector3(1.3f, .8f, 1); view.sprite = sprite; child.transform.localScale = Vector3.one * scale; child.transform.localRotation = Quaternion.Euler(0, 0, angle);
                            Vector3 ground = new Vector3(2.7f, -1.8f, 0); WorldSpriteGrounding.Place(view, ground);
                            Require(Vector3.Distance(child.transform.TransformPoint(WorldSpriteGrounding.Foot(sprite)), ground) < .00001f, "Visible foot remains grounded " + sprite.name + "/" + scale + "/" + angle);
                            Vector3 before = child.transform.position; WorldSpriteGrounding.Place(view, ground);
                            Require(Vector3.Distance(before, child.transform.position) < .00001f, "Ground placement never accumulates drift " + sprite.name + "/" + scale + "/" + angle);
                        }
                Require(ShopRarityGlow.CanDecorate(view), "Physical merchandise may carry a stock aura");
                parent.AddComponent<UnitVisual>();
                Require(!ShopRarityGlow.CanDecorate(view), "Owned gameplay actors cannot carry a shop rarity aura");
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
            Require(WorldSpriteGrounding.SortAt(-2) > WorldSpriteGrounding.SortAt(2), "Depth sorting uses contact point not image centre");
            Directory.CreateDirectory("PreviewExports/ShopPresentation"); File.WriteAllLines("PreviewExports/ShopPresentation/Focused_Checks.txt", notes);
            Debug.Log("SHOP_PRESENTATION_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate()
        {
            Build(); CharacterActingBuilder.Build(); AnimationPolishBuilder.Run(); EnsembleBuilder.Check();
            OctoberContentBuilder.Regressions(); FullRunBuilder.BuildWindows();
        }
    }
}
