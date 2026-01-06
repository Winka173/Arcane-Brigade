using UnityEditor;

namespace ArcaneBrigade.Editor
{
    public static class K6Builder
    {
        [MenuItem("Arcane Brigade/K6/Check meta and UI")]
        public static void Check() { ApplyPresentation(); K6Checks.Run(); }
        [MenuItem("Arcane Brigade/K6/Full regression and Windows build")]
        public static void GateAndBuild() { ApplyPresentation(); K6Checks.Run(); K5Builder.GateAndBuild(); }
        private static void ApplyPresentation()
        {
            const string path = "Assets/ArcaneBrigade/Resources/LostShop_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096; importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false; importer.SaveAndReimport();
        }
    }
}
