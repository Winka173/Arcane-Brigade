using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K19RelicArtworkBuilder
    {
        private static readonly string[] Ids = {
            "key-oil", "clock-spring", "honey-pot", "felt-boots", "coin-compass", "wandering-map", "guardian-canopy", "warm-tea", "oracle-lens", "twin-seal", "piggy-mail", "pillow-mail", "royal-key", "brigade-banner", "marching-bell", "coupon-book",
            "long-atlas", "starlight-quilt", "echo-postage", "tarnished-crown", "fortune-egg", "treasure-whisper", "coin-salve", "chest-thunder", "altar-ember", "k10-scoremultiplier", "k10-collisiondamage", "k10-meleedamage", "k10-rangeddamage", "k10-attackrange", "k10-dodge", "k10-income",
            "k10-bulletpierce", "k10-bulletsplit", "k10-bulletbounce", "k10-bulletsize", "k10-bulletspeed", "k10-bulletdistance", "k10-knockback", "k10-bodysize", "k10-companionsize", "k10-enemycount", "k10-enemyhealth", "k10-enemyspeed", "k10-hearthealing", "k10-chestquality", "k10-recruitdiscount", "k10-relicdiscount",
            "k10-shoprarity", "k10-healthcorrection", "k10-damagecorrection", "k10-speedcorrection", "k10-armorcorrection", "k10-healcorrection", "k10-collisionfrequency", "long-pocket", "velvet-first-aid", "brass-avalanche"
        };
        [MenuItem("Arcane Brigade/K19/Import dedicated relic artwork")]
        public static void Build()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var artwork = new List<Sprite>();
            foreach (string sheet in new[] { "A", "B", "C", "D" }) artwork.AddRange(Import(sheet));
            for (int index = 0; index < Ids.Length; index++)
            {
                ItemDefinition item = catalog.Item(Ids[index]);
                if (item == null) throw new InvalidOperationException("Unknown relic " + Ids[index]);
                item.Icon = artwork[index];
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            AssetRoleAudit.Run();
            K19ArtworkBuilder.Build();
        }
        private static Sprite[] Import(string sheet)
        {
            string path = "Assets/ArcaneBrigade/Art/K19/Relics" + sheet + "_v1.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 256; importer.maxTextureSize = 4096; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("Relic sheet requires real transparency " + sheet);
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rectangle => rectangle.name, rectangle => rectangle.spriteID);
            var rectangles = new SpriteRect[16];
            RectInt[] bounds = ComponentBounds(pixels, texture.width, texture.height);
            for (int index = 0; index < 16; index++)
            {
                RectInt region = bounds[index];
                if (region.width <= 1 || region.height <= 1) throw new InvalidOperationException("Empty relic " + sheet + "/" + index);
                string name = "Relic_" + sheet + "_" + index.ToString("D2");
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(region.x, region.y, region.width, region.height), pivot = new Vector2(0.5f, 0.5f), alignment = SpriteAlignment.Center, spriteID = previous.TryGetValue(name, out GUID identity) ? identity : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rectangle => new SpriteNameFileIdPair(rectangle.name, rectangle.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.BC7 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 4096, format = TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
        private static RectInt[] ComponentBounds(Color32[] pixels, int width, int height)
        {
            var result = new RectInt[16]; var seen = new bool[pixels.Length]; var queue = new Queue<int>();
            for (int index = 0; index < pixels.Length; index++) seen[index] = pixels[index].a < 33;
            for (int start = 0; start < pixels.Length; start++)
            {
                if (seen[start]) continue;
                queue.Enqueue(start); seen[start] = true;
                int count = 0, left = width, bottom = height, right = 0, top = 0;
                while (queue.Count > 0)
                {
                    int point = queue.Dequeue(), horizontal = point % width, vertical = point / width; count++;
                    left = Mathf.Min(left, horizontal); right = Mathf.Max(right, horizontal); bottom = Mathf.Min(bottom, vertical); top = Mathf.Max(top, vertical);
                    void Visit(int next)
                    {
                        if (next < 0 || next >= pixels.Length || seen[next] || Mathf.Abs(next % width - horizontal) > 1) return;
                        seen[next] = true; queue.Enqueue(next);
                    }
                    Visit(point - 1); Visit(point + 1); Visit(point - width); Visit(point + width);
                }
                if (count < 100) continue;
                int column = Mathf.Clamp((left + right) * 2 / width, 0, 3), row = 3 - Mathf.Clamp((bottom + top) * 2 / height, 0, 3);
                int slot = row * 4 + column;
                if (result[slot].width > 0)
                {
                    left = Mathf.Min(left, result[slot].xMin); right = Mathf.Max(right, result[slot].xMax - 1);
                    bottom = Mathf.Min(bottom, result[slot].yMin); top = Mathf.Max(top, result[slot].yMax - 1);
                }
                result[slot] = new RectInt(left, bottom, right - left + 1, top - bottom + 1);
            }
            for (int index = 0; index < result.Length; index++)
            {
                RectInt region = result[index];
                int left = Mathf.Max(0, region.xMin - 1), bottom = Mathf.Max(0, region.yMin - 1);
                int right = Mathf.Min(width, region.xMax + 1), top = Mathf.Min(height, region.yMax + 1);
                result[index] = new RectInt(left, bottom, right - left, top - bottom);
            }
            return result;
        }
    }
}
