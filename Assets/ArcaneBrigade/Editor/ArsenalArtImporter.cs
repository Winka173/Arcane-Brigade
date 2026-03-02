using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Import settings for the Bizarre Arsenal art (Art/Arsenal, Docs/Bizarre_Arsenal.md §5.1): single sprites at 256 PPU,
    /// clean alpha, mipmapped. A held weapon pivots at its grip (20% across, middle height); every other piece at its centre.</summary>
    public sealed class ArsenalArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Art/Arsenal/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear; importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = path.Contains("_Held_") ? new Vector2(0.2f, 0.5f) : new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
        }
    }
}
