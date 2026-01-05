using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K3LootArt
    {
        private const string Root = "Assets/ArcaneBrigade/Art/K3Loot/";
        public static Sprite Create(LootKind kind)
        {
            Directory.CreateDirectory(Root);
            string path = Root + kind + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                var pixels = new Color[256 * 256];
                for (int vertical = 0; vertical < 256; vertical++)
                    for (int horizontal = 0; horizontal < 256; horizontal++)
                    {
                        Color total = Color.clear;
                        for (int sample = 0; sample < 4; sample++)
                        {
                            Vector2 point = new Vector2((horizontal + (sample % 2) * 0.5f) / 128f - 1, (vertical + (sample / 2) * 0.5f) / 128f - 1);
                            total += Pixel(kind, point) * 0.25f;
                        }
                        pixels[vertical * 256 + horizontal] = total;
                    }
                texture.SetPixels(pixels); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 256;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            Sprite result = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (result == null) throw new System.InvalidOperationException("Loot sprite import failed: " + path);
            return result;
        }

        private static Color Pixel(LootKind kind, Vector2 point)
        {
            Color ink = new Color(0.12f, 0.07f, 0.2f);
            float shape;
            if (kind == LootKind.Egg) shape = new Vector2(point.x / (0.55f - point.y * 0.1f), (point.y + 0.05f) / 0.78f).magnitude;
            else if (kind == LootKind.Heart)
            {
                Vector2 heart = new Vector2(point.x * 1.3f, point.y * 1.3f);
                float radial = heart.x * heart.x + heart.y * heart.y - 0.55f;
                shape = radial * radial * radial - heart.x * heart.x * heart.y * heart.y * heart.y < 0 ? 0 : 2;
            }
            else shape = Mathf.Max(Mathf.Abs(point.x) / 0.79f, Mathf.Abs(point.y + 0.07f) / 0.57f);
            if (shape > 1) return Color.clear;
            Color color = kind == LootKind.Egg ? new Color(0.47f, 0.9f, 0.84f) : kind == LootKind.Heart ? new Color(1f, 0.33f, 0.49f) : new Color(0.69f, 0.38f, 0.17f);
            if (shape > 0.9f) return ink;
            if (kind == LootKind.Chest)
            {
                if (Mathf.Abs(point.y - 0.12f) < 0.045f) return ink;
                if (Mathf.Abs(point.x) > 0.54f || Mathf.Abs(point.y + 0.49f) < 0.055f) color = new Color(1f, 0.75f, 0.25f);
                if (Mathf.Abs(point.x) < 0.11f && Mathf.Abs(point.y - 0.07f) < 0.19f) return new Color(1f, 0.86f, 0.36f);
            }
            if ((point - new Vector2(-0.18f, 0.16f)).sqrMagnitude < 0.006f || (point - new Vector2(0.18f, 0.16f)).sqrMagnitude < 0.006f) return ink;
            if ((point - new Vector2(-0.23f, 0.42f)).sqrMagnitude < 0.018f) return new Color(1f, 0.97f, 0.85f);
            if (kind == LootKind.Egg && Mathf.Abs(point.y + 0.18f + Mathf.Sin(point.x * 14) * 0.06f) < 0.028f) return new Color(0.17f, 0.56f, 0.6f);
            return Color.Lerp(color * 0.82f, color, Mathf.Clamp01(point.y + 0.65f));
        }
    }
}
