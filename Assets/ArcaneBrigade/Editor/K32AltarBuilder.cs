using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K32AltarBuilder
    {
        [MenuItem("Arcane Brigade/K3/Apply altar slice and build")]
        public static void ApplyAndBuild() { Apply(); K32AltarChecks.Run(); FullRunBuilder.BuildWindows(); }

        public static void Apply()
        {
            K32Builder.Apply();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            if (catalog.AltarRevision > 0) return;
            catalog.AltarBaseChance = 0.1f;
            catalog.AltarChancePerEgg = 0.05f;
            foreach (ItemDefinition item in catalog.Items)
            {
                item.AltarCurrency = item.Rarity >= ItemRarity.Rare ? ShopCurrency.Health : ShopCurrency.Eggs;
                item.AltarPrice = item.AltarCurrency == ShopCurrency.Health ? Mathf.Max(5, Mathf.CeilToInt(item.Price * 0.3f)) : 2;
            }
            catalog.AltarRevision = 1;
            RunModifierRules.Validate(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}
