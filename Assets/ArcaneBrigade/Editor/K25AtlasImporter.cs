using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K25AtlasImporter
    {
        public static Sprite[] Import(string path, string id, int columns, int rows, int[] cells, int[] rowEdges = null, bool componentRows = false, bool mainComponentsOnly = false, Rect[] regions = null)
        {
            if (regions != null && (regions.Length != columns * rows || componentRows || regions.Any(region => region.width <= 0 || region.height <= 0 || region.xMin < 0 || region.yMin < 0 || region.xMax > 1 || region.yMax > 1))) throw new ArgumentException("Invalid atlas regions " + id);
            if (columns <= 0 || rows <= 0 || cells.Any(cell => cell < 0 || cell >= columns * rows) || rowEdges != null && (rowEdges.Length != rows + 1 || rowEdges[0] != 0 || rowEdges[rows] != 1024)) throw new ArgumentException("Invalid K25 atlas layout " + id);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 128; importer.maxTextureSize = 4096; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a > 4) throw new InvalidOperationException("K25 transparent source required " + id);
            RectInt[] components = componentRows ? Components(pixels, texture.width, texture.height, columns, rows, rowEdges, mainComponentsOnly) : null;
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(rect => rect.name, rect => rect.spriteID);
            var rectangles = new SpriteRect[cells.Length];
            for (int index = 0; index < cells.Length; index++)
            {
                int cell = cells[index];
                int left = cell % columns * texture.width / columns, right = (cell % columns + 1) * texture.width / columns;
                int row = cell / columns;
                int bottom = texture.height - (rowEdges == null ? (row + 1) * texture.height / rows : rowEdges[row + 1] * texture.height / 1024);
                int top = texture.height - (rowEdges == null ? row * texture.height / rows : rowEdges[row] * texture.height / 1024);
                if (regions != null)
                {
                    Rect region = regions[cell]; left = Mathf.RoundToInt(region.xMin * texture.width); right = Mathf.RoundToInt(region.xMax * texture.width);
                    bottom = Mathf.RoundToInt(region.yMin * texture.height); top = Mathf.RoundToInt(region.yMax * texture.height);
                }
                int minX = right, minY = top, maxX = left, maxY = bottom;
                if (components != null)
                {
                    RectInt region = components[cell]; minX = region.xMin; minY = region.yMin; maxX = region.xMax - 1; maxY = region.yMax - 1;
                }
                else
                    for (int vertical = bottom; vertical < top; vertical++)
                        for (int horizontal = left; horizontal < right; horizontal++)
                        {
                            if (pixels[vertical * texture.width + horizontal].a < 32) continue;
                            minX = Mathf.Min(minX, horizontal); minY = Mathf.Min(minY, vertical); maxX = Mathf.Max(maxX, horizontal); maxY = Mathf.Max(maxY, vertical);
                        }
                if (maxX - minX < 8 || maxY - minY < 8) throw new InvalidOperationException("K25 empty cell " + id + "/" + cell);
                string name = id + "_" + index.ToString("D2");
                rectangles[index] = new SpriteRect { name = name, rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), pivot = new Vector2(0.5f, 0), alignment = SpriteAlignment.Custom, spriteID = previous.TryGetValue(name, out GUID guid) ? guid : GUID.Generate() };
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rectangles.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            foreach (string platform in new[] { "Standalone", "Android", "iPhone" }) importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = platform, overridden = true, maxTextureSize = 4096, format = platform == "Standalone" ? TextureImporterFormat.BC7 : TextureImporterFormat.ASTC_6x6 });
            importer.isReadable = false; importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        }
        private static RectInt[] Components(Color32[] pixels, int width, int height, int columns, int rows, int[] rowEdges, bool mainComponentsOnly)
        {
            var regions = new RectInt[columns * rows]; var seen = new bool[pixels.Length]; var queue = new Queue<int>();
            var mainCounts = new int[regions.Length];
            for (int index = 0; index < pixels.Length; index++) seen[index] = pixels[index].a < 32;
            for (int start = 0; start < pixels.Length; start++)
            {
                if (seen[start]) continue;
                seen[start] = true; queue.Enqueue(start);
                int count = 0, left = width, right = 0, bottom = height, top = 0;
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
                int column = Mathf.Clamp((left + right) * columns / (2 * width), 0, columns - 1);
                int center = 1024 - (bottom + top) * 512 / height, row = 0;
                while (row < rows - 1 && center >= rowEdges[row + 1]) row++;
                int slot = row * columns + column;
                if (mainComponentsOnly && count <= mainCounts[slot]) continue;
                if (regions[slot].width > 0 && !mainComponentsOnly)
                {
                    left = Mathf.Min(left, regions[slot].xMin); right = Mathf.Max(right, regions[slot].xMax - 1);
                    bottom = Mathf.Min(bottom, regions[slot].yMin); top = Mathf.Max(top, regions[slot].yMax - 1);
                }
                regions[slot] = new RectInt(left, bottom, right - left + 1, top - bottom + 1);
                mainCounts[slot] = count;
            }
            return regions;
        }
    }
}
