using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Imports the approved master-list deliveries and exports exact source sprites for image edits.</summary>
    public static class ArtMasterImporter
    {
        private const string Story = "Assets/ArcaneBrigade/Resources/MienCho/ActOne/";
        public static void Import()
        {
            AssetDatabase.Refresh();
            foreach (string folder in new[] { Story, "Assets/ArcaneBrigade/Art/ActOneBosses/", "Assets/ArcaneBrigade/Art/CombatVFX/" })
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string path in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                {
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    bool floor = Path.GetFileName(path).StartsWith("Floor_", StringComparison.Ordinal);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 256;
                    importer.alphaIsTransparency = !floor;
                    importer.mipmapEnabled = true;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.maxTextureSize = 2048;
                    importer.wrapMode = floor ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Trilinear;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.SaveAndReimport();
                }
            }
            HeroMechanicArtImporter.Import();
            AssetDatabase.SaveAssets();
            Debug.Log("ART_MASTER_IMPORT complete");
        }
        public static void ExportReferences()
        {
            Directory.CreateDirectory(".utmp/ArtRefs");
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            string[] ids = { "TeapotPilgrim", "HoodWisp", "PaperDevil" };
            string[] names = { "Hob", "Latch", "Tally" };
            for (int i = 0; i < ids.Length; i++)
            {
                var unit = catalog.Unit(ids[i]);
                Export(unit.Body, names[i]);
                for (int f = 0; f < unit.AnimationFrames.Length; f++) Export(unit.AnimationFrames[f], names[i] + "_f" + f);
            }
            var nightmare = NightmareAssets.Current;
            Export(nightmare.Customer.Body, "LastCustomer");
            for (int f = 0; f < nightmare.CustomerFrames.Length; f++) Export(nightmare.CustomerFrames[f], "LastCustomer_f" + f);
            Debug.Log("ART_MASTER_REFERENCES exported exact source pixels");
        }
        private static void Export(Sprite sprite, string name)
        {
            if (sprite == null) throw new InvalidOperationException("Missing reference " + name);
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(sprite.texture, target);
            RenderTexture.active = target;
            Rect rectangle = sprite.textureRect;
            var pixels = new Texture2D((int)rectangle.width, (int)rectangle.height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(rectangle, 0, 0); pixels.Apply();
            File.WriteAllBytes(".utmp/ArtRefs/" + name + ".png", pixels.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(pixels);
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target);
        }
    }
}
