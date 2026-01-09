using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K11ThemeBuilder
    {
        public static void Apply()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); catalog.HeroRevision = 1;
            foreach (UnitDefinition champion in catalog.Champions)
            {
                champion.RecruitThemes = champion.Id == "LanternKnight" ? new[] { "hearth", "clock", "orchard", "oddity" } : champion.Id == "Postmaster" ? new[] { "archive", "clock", "tidal" } : new[] { "frost", "astral", "tidal", "oddity" };
                champion.HeroMechanic = champion.Id == "LanternKnight" ? HeroMechanic.LanternStride : champion.Id == "Postmaster" ? HeroMechanic.ParcelExpress : HeroMechanic.DreamBreath;
                EditorUtility.SetDirty(champion);
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
    }
}
