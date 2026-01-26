using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K20BossAnatomyBuilder
    {
        [MenuItem("Arcane Brigade/K20/Build unique boss anatomy")]
        public static void Build()
        {
            Sprite[][] sheets = new[] { "RoyalArchive", "SleepWax", "LanternStage", "UmbraWidow", "ChoirLuggage" }.Select(Import).ToArray();
            const string path = "Assets/ArcaneBrigade/Resources/BossAnatomy.asset";
            BossAnatomy asset = AssetDatabase.LoadAssetAtPath<BossAnatomy>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<BossAnatomy>(); AssetDatabase.CreateAsset(asset, path); }
            asset.Bosses = new[] {
                Entry("UndeliveredKing", sheets[0], 0, new[] { "Stamp Jaw", "Dead Letter Cannon", "Pendulum Crown" }, new[] { "No envelope bites.", "Smaller letter volleys.", "Slower attacks; no pendulum pulse." }, new[] { BossOrganRole.Lash, BossOrganRole.Volley, BossOrganRole.Tempo }),
                Entry("ScarletIndex", sheets[0], 8, new[] { "Quill Talon", "Binder Maw", "Ink Bladder" }, new[] { "No quill lashes.", "No falling paper traps.", "No ink pools." }, new[] { BossOrganRole.Lash, BossOrganRole.Props, BossOrganRole.Pool }),
                Entry("SleepCollector", sheets[1], 0, new[] { "Rocking Claw", "Drowsing Bell", "Dream Sack" }, new[] { "No claw lashes; slower walking.", "No paired sleep rings.", "No dream pools." }, new[] { BossOrganRole.Lash | BossOrganRole.Locomotion, BossOrganRole.Pulse, BossOrganRole.Pool }),
                Entry("WaxAdmiral", sheets[1], 8, new[] { "Anchor Claw", "Molten Broadside", "Wick Cluster" }, new[] { "No anchor sweep.", "Smaller molten volleys.", "No molten wax pools." }, new[] { BossOrganRole.Lash, BossOrganRole.Volley, BossOrganRole.Pool }),
                Entry("LastLantern", sheets[2], 0, new[] { "Wick Tendril", "Shutter Eye", "Fuel Chamber" }, new[] { "No needle lashes.", "Smaller light volleys.", "No falling fuel traps." }, new[] { BossOrganRole.Lash, BossOrganRole.Volley, BossOrganRole.Props }),
                Entry("MarqueeMaw", sheets[2], 8, new[] { "Stage Jaw", "Spotlight Eyes", "Curtain Tendon" }, new[] { "No stage bite.", "Fewer aimed spotlight shots.", "No thrown stage debris." }, new[] { BossOrganRole.Lash, BossOrganRole.Volley, BossOrganRole.Props }),
                Entry("UmbraCradle", sheets[3], 0, new[] { "Rocking Arm", "Nursery Bell", "Shadow Pouch" }, new[] { "No shadow lashes; cradle tilts and slows.", "No nursery rings.", "No shadow pools." }, new[] { BossOrganRole.Lash | BossOrganRole.Locomotion, BossOrganRole.Pulse, BossOrganRole.Pool }),
                Entry("CoinWidow", sheets[3], 8, new[] { "Coin Hook", "Coin Spitter", "Vault Abdomen" }, new[] { "No hook sweeps; slower crawling.", "Fewer coin shots.", "Returns stolen coins; no suction. Attacks faster." }, new[] { BossOrganRole.Lash | BossOrganRole.Locomotion, BossOrganRole.Volley, BossOrganRole.Vacuum | BossOrganRole.Rupture }),
                Entry("ChoirEngine", sheets[4], 0, new[] { "Bass Mouth", "Choir Pipes", "Bellows Heart" }, new[] { "No triple bass rings.", "Fewer choir shots.", "Slower rhythm; no bellows pulse." }, new[] { BossOrganRole.Pulse, BossOrganRole.Volley, BossOrganRole.Tempo }),
                Entry("SuitcaseLeviathan", sheets[4], 8, new[] { "Zipper Jaw", "Handle Tentacle", "Luggage Chamber" }, new[] { "No zipper bite.", "No thrown luggage.", "No tongue pools; burst suitcase attacks faster." }, new[] { BossOrganRole.Lash, BossOrganRole.Props, BossOrganRole.Pool | BossOrganRole.Rupture })
            };
            EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets(); BossAnatomy.Invalidate();
            Verify();
        }
        private static BossAnatomyEntry Entry(string id, Sprite[] art, int start, string[] names, string[] descriptions, BossOrganRole[] roles)
        {
            var entry = new BossAnatomyEntry { BossId = id, Organs = new BossOrganDefinition[3] };
            for (int part = 0; part < 3; part++)
            {
                Vector2 position = part == 0 ? new Vector2(-0.39f, -0.03f) : part == 1 ? new Vector2(0.39f, -0.03f) : new Vector2(0, -0.3f);
                if (part == 2 && (id == "UndeliveredKing" || id == "WaxAdmiral" || id == "LastLantern")) position = new Vector2(0, 0.34f);
                entry.Organs[part] = new BossOrganDefinition { Id = id + "-" + part, Name = names[part], BreakSummary = descriptions[part], Role = roles[part], Offset = position, Height = part == 2 ? 0.23f : 0.29f, Durability = part == 2 ? 0.16f : 0.18f, Rest = art[start + part], Charged = art[start + 4 + part], Scar = art[start + 3] };
            }
            return entry;
        }
        private static Sprite[] Import(string id)
        {
            string path = "Assets/ArcaneBrigade/Art/K20/" + id + "_v1.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("Transparent source required " + id);
            RectInt[] regions = K19RelicArtworkBuilder.ComponentBounds(pixels, texture.width, texture.height);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[16];
            for (int index = 0; index < rectangles.Length; index++)
            {
                RectInt region = regions[index];
                if (region.width <= 2 || region.height <= 2) throw new InvalidOperationException("Empty anatomy cell " + id + "/" + index);
                string name = id + "_" + index.ToString("D2");
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(region.x, region.y, region.width, region.height), pivot = Vector2.one * 0.5f, alignment = SpriteAlignment.Center, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/K20");
            K20BossAnatomyChecks.Run(); K19EncounterChecks.Verify();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K20/Windows_Build.txt", true);
        }
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K20");
    }
}
