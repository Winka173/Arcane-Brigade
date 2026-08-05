using UnityEditor;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    /// <summary>Authors design v3's 20 majors, 32 minors and roster-derived memberships without creating presentation assets.</summary>
    public static class BondReworkBuilder
    {
        [MenuItem("Arcane Brigade/Content/Put the bond rework in")]
        public static void Run()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            if (catalog == null) throw new System.InvalidOperationException("Missing RunCatalog resource");
            BondV3Catalog.Configure(catalog);
            int updated = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade/Data", "Assets/ArcaneBrigade/Resources" }))
            {
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (unit == null) continue;
                string[] tags = BondV3Catalog.Tags(unit.Id);
                if (tags.Length == 0) continue;
                unit.Tags = tags; EditorUtility.SetDirty(unit); updated++;
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log("BOND_REWORK_V3: " + catalog.Bonds.Length + " bonds; " + updated + " existing companion assets updated. New companions require rules-only catalog definitions.");
        }
    }
}
