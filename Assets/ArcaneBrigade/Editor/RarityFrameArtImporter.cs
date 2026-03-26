using UnityEditor;

namespace ArcaneBrigade.Editor
{
    /// <summary>Import settings for the rarity frame trim painted by Tools/Art/rarity_frame_art.py: plain textures loaded
    /// from Resources with clean alpha, mipmapped so they stay smooth drawn at tooltip size. Formats and sizes come from
    /// BuildSizePolicy (BC7 on PC, ASTC on phones; the pieces are resized to their largest draw, the strips exactly 1:2). The side strips repeat vertically (set at runtime); everything else clamps.</summary>
    public sealed class RarityFrameArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/RarityFrame/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = UnityEngine.FilterMode.Trilinear;
            importer.wrapMode = assetPath.EndsWith("_Side.png") || assetPath.EndsWith("_Glow.png") ? UnityEngine.TextureWrapMode.Repeat : UnityEngine.TextureWrapMode.Clamp;
            importer.isReadable = false;
        }
    }
}
